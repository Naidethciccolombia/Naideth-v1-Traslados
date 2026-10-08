using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.DTO;
using Naideth.Traslados.Dominio.Kernel.Exepciones;
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces;
using Naideth.Central.Infrastructura.AccesoDatos;

namespace Naideth.Traslados.Infrastructura.Comunes
{
    internal class ComunRepositorio : IComunRepositorio
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheRedisServicio _cacheServicio; 
        private readonly SemaphoreSlim _trmLock = new(1, 1);
        private readonly IServiceScopeFactory _scopeFactory;
        public ComunRepositorio(IHttpContextAccessor httpContextAccessor,IApplicationDbContext context, ICacheRedisServicio cacheServicio, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _cacheServicio = cacheServicio; 
            _scopeFactory = scopeFactory;   
        }
        public async Task<List<TasaCambioDTO>> ListaTRMAsync(CancellationToken tokenCancelacion)
        {
            const string cacheKey = "ListaTRMS";

            var cachedResult = await _cacheServicio
                .GetAsync<List<TasaCambioDTO>>(cacheKey, tokenCancelacion)
                .ConfigureAwait(false);

            if (cachedResult != null && cachedResult.Count > 2)
                return cachedResult;

            await _trmLock.WaitAsync(tokenCancelacion);
            try
            {
                //  double-check (MUY IMPORTANTE)
                cachedResult = await _cacheServicio
                    .GetAsync<List<TasaCambioDTO>>(cacheKey, tokenCancelacion)
                    .ConfigureAwait(false);

                if (cachedResult != null && cachedResult.Count > 2)
                    return cachedResult;

                //  ahora sí cargar
                var band=await StartListaTRM(tokenCancelacion).ConfigureAwait(false);

                cachedResult = await _cacheServicio
                    .GetAsync<List<TasaCambioDTO>>(cacheKey, tokenCancelacion)
                    .ConfigureAwait(false);

                return cachedResult ?? new List<TasaCambioDTO>();
            }
            finally
            {
                _trmLock.Release();
            }
        }

        public async Task<bool> StartListaTRM(CancellationToken tokenCancelacion)
        {
            string cacheKey = "ListaTRMS"; 

            List<TasaCambioDTO> tasas;
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                  
                tasas =await ListaTRM(context, tokenCancelacion).ConfigureAwait(false);

            }

            await _cacheServicio.SetAsync(
                cacheKey,
                tasas,
                TimeSpan.FromDays(1),
                tokenCancelacion);

            return true;
        }
         
        public async Task<List<TasaCambioDTO>> ListaTRM(IApplicationDbContext context,  CancellationToken tokenCancelacion)
        {
          
            // Obtener la última fecha por moneda (sin restricción de días)
            var subquery = context.TasaCambio
                .AsNoTracking()
                .Where(t => t.Estado)
                .GroupBy(t => t.IdMoneda)
                .Select(g => new
                {
                    IdMoneda = g.Key,
                    Fecha = g.Max(x => x.Fecha)
                });

            return await (
                from t in context.TasaCambio.AsNoTracking()
                join s in subquery
                    on new { t.IdMoneda, t.Fecha } equals new { s.IdMoneda, s.Fecha }
                join m in context.Moneda.AsNoTracking().Where(x => x.Estado)
                    on t.IdMoneda equals m.IdMoneda
                select new TasaCambioDTO(
                    t.IdTasaCambio,
                    t.IdMoneda,
                    m.Equivalente,
                    t.Valor,
                    t.Fecha)
            ).ToListAsync(tokenCancelacion).ConfigureAwait(false);
             
        }


        public async Task<List<IntegracionDTO>> CredencialesAsync( CancellationToken token)
        {
            const string cacheKey = "CREDENCIAL_TRASLADO";

            // leer cache global
            var cache = await _cacheServicio.GetAsync<List<IntegracionDTO>>(cacheKey, token).ConfigureAwait(false);

            if (cache == null)
            {
                _ = await CredencialesForCacheAsync(token).ConfigureAwait(false); 
                cache = await _cacheServicio.GetAsync<List<IntegracionDTO>>(cacheKey, token).ConfigureAwait(false);

            }

            // obtener tenants del request
            var tenants = _context.GetTenants().ToHashSet();

            // filtrar en memoria
            var resultado = cache
                .Where(x => tenants.Contains(x.IdTenant.ToString()))
                .ToList();

            return resultado;
        }

        //public async Task<List<IntegracionDTO>> CredencialesAsync(CancellationToken TokenCancelacion)
        //{
        //    var tenants = _context.GetTenants();

        //    // 1. OBTENER INTEGRACIONES BASE
        //    var integraciones = await (
        //        from integracion in _context.IntegracionNombres
        //        join tipoProducto in _context.TipoProducto on integracion.IdTipoProducto equals tipoProducto.IdTipoProducto
        //        join integrador in _context.Integradores on integracion.IdIntegrador equals integrador.IdIntegrador
        //        join credencial in _context.Credenciales.FiltroTenant(tenants)
        //            on integracion.IdIntegracionNombre equals credencial.IdIntegracion
        //        where integracion.Estado &&
        //              tipoProducto.Estado &&
        //              credencial.Estado &&
        //              tipoProducto.Nombre == "VUELO" &&
        //              integrador.Estado &&
        //              _context.EdadIntegracion.Any(e => e.IdIntegracion == integracion.IdIntegracionNombre && e.Estado)
        //        select new IntegracionDTO(
        //            integracion.IdIntegracionNombre,
        //            integrador.IdIntegrador,
        //            integrador.Codigo,
        //            integracion.Codigo,
        //            integracion.Nombre,
        //            integracion.EndPoint,
        //            credencial.Usuario,
        //            credencial.Clave,
        //            new List<CredencialDTO>(),
        //            new List<EdadIntegracionDTO>()
        //        )
        //    ).ToListAsync(TokenCancelacion).ConfigureAwait(false);

        //    if (!integraciones.Any())
        //        throw new CredencialesNoDisponibles();

        //    // Crear diccionario por IdIntegracion
        //    var mapIntegraciones = integraciones.ToDictionary(x => x.idIntegracion);

        //    // 2. CREDENCIALES AGRUPADAS
        //    var credencialesAgrupadas = await (
        //        from parametro in _context.ParametroCredencial.FiltroTenant(tenants)
        //        join credencial in _context.Credenciales
        //            on parametro.IdIntegracionCredencial equals credencial.IdIntegracionCredencial
        //        where parametro.Estado
        //        group parametro by credencial.IdIntegracion into g
        //        select new
        //        {
        //            IdIntegracion = g.Key,
        //            Credenciales = g.Select(p => new CredencialDTO(p.Nombre, p.Valor)).ToList()
        //        }
        //    ).ToListAsync(TokenCancelacion).ConfigureAwait(false);

        //    // 3. EDADES AGRUPADAS
        //    var edadesAgrupadas = await (
        //        from e in _context.EdadIntegracion
        //        where e.Estado
        //        group e by e.IdIntegracion into g
        //        select new
        //        {
        //            IdIntegracion = g.Key,
        //            Edades = g.Select(e =>
        //                new EdadIntegracionDTO(e.Tipo, e.Minimo, e.Maximo)
        //            ).ToList()
        //        }
        //    ).ToListAsync(TokenCancelacion).ConfigureAwait(false);

        //    // 4. ASIGNACIÓN O(1) por diccionarios
        //    foreach (var cred in credencialesAgrupadas)
        //        if (mapIntegraciones.TryGetValue(cred.IdIntegracion, out var integ))
        //            integ.credencial.AddRange(cred.Credenciales);

        //    foreach (var edad in edadesAgrupadas)
        //        if (mapIntegraciones.TryGetValue(edad.IdIntegracion, out var integ))
        //            integ.edades.AddRange(edad.Edades);

        //    return integraciones;
        //}

        public async Task<List<IntegracionDTO>> CredencialesCancelacionAsync( CancellationToken TokenCancelacion)
        {
            var context = _httpContextAccessor.HttpContext;

            var userId = context?.Items["UserId"] as Guid?;

            // Consulta inicial optimizada
            var integraciones = await (
                from integracion in _context.IntegracionNombres
                join tipoProducto in _context.TipoProducto on integracion.IdTipoProducto equals tipoProducto.IdTipoProducto
                join integrador in _context.Integradores on integracion.IdIntegrador equals integrador.IdIntegrador
                join credencial in _context.Credenciales.FiltroTenant(_context.GetTenants())
                    on integracion.IdIntegracionNombre equals credencial.IdIntegracion
                where
                     
                    tipoProducto.Estado &&
                    credencial.Estado &&
                    tipoProducto.Nombre == "TRASLADO" &&
                    integrador.Estado
                 select new IntegracionDTO( 
                    integracion.IdIntegracionNombre,
                    integrador.IdIntegrador,
                    integrador.Codigo,
                    integracion.Codigo,
                    integracion.Nombre,
                    integracion.EndPoint,
                    credencial.Usuario,
                    credencial.Clave,
                    new List<CredencialDTO>(),
                    new List<EdadIntegracionDTO>(),
                    credencial.IdTenant
                )
            ).ToListAsync(TokenCancelacion).ConfigureAwait(false);

            // Consultas adicionales para credenciales y edades
            var credencialesAgrupadas = await (
      from parametroCredencial in _context.ParametroCredencial.FiltroTenant(_context.GetTenants())
      join credencial in _context.Credenciales on parametroCredencial.IdIntegracionCredencial equals credencial.IdIntegracionCredencial
      where parametroCredencial.Estado
      group new { parametroCredencial, credencial } by parametroCredencial.IdIntegracionCredencial into agrupacion
      select new
      {
          IdIntegracionCredencial = agrupacion.Key,
          IdIntegracion = agrupacion.FirstOrDefault().credencial.IdIntegracion, // Obtener IdIntegracion desde el primer elemento
          Credenciales = agrupacion.Select(c => new CredencialDTO(
              c.parametroCredencial.Nombre,
              c.parametroCredencial.Valor
          )).ToList()
      }
  ).ToListAsync(TokenCancelacion).ConfigureAwait(false);


            var edadesAgrupadas = await (
                from edadesCredenciales in _context.EdadIntegracion
                where edadesCredenciales.Estado
                group edadesCredenciales by edadesCredenciales.IdIntegracion into edades
                select new
                {
                    IdIntegracion = edades.Key,
                    Edades = edades.Select(e => new EdadIntegracionDTO(e.Tipo, e.Minimo, e.Maximo)).ToList()
                }
            ).ToListAsync(TokenCancelacion).ConfigureAwait(false);

            // Agregar credenciales y edades a las integraciones
            foreach (var integracion in integraciones)
            {
                var credenciales = credencialesAgrupadas
                    .FirstOrDefault(c => c.IdIntegracion == integracion.idIntegracion);
                if (credenciales != null)
                {
                    integracion.credencial.AddRange(credenciales.Credenciales);
                }

                var edades = edadesAgrupadas.FirstOrDefault(e => e.IdIntegracion == integracion.idIntegracion);
                if (edades != null)
                {
                    integracion.edades.AddRange(edades.Edades);
                }
            }



            if (integraciones.Count() == 0)
                throw new CredencialesNoDisponibles();


            return integraciones;
        }


        public async Task<bool> CredencialesForCacheAsync(CancellationToken token)
        {
          //  var tenants = _context.GetTenants();
            const string cacheKey = "CREDENCIAL_TRASLADO";
             
            // 1. OBTENER INTEGRACIONES BASE (SIN TENANT)
            var integraciones = await (
                from integracion in _context.IntegracionNombres
                join tipoProducto in _context.TipoProducto on integracion.IdTipoProducto equals tipoProducto.IdTipoProducto
                join integrador in _context.Integradores on integracion.IdIntegrador equals integrador.IdIntegrador
                join credencial in _context.Credenciales
                    on integracion.IdIntegracionNombre equals credencial.IdIntegracion
                where integracion.Estado &&
                      tipoProducto.Estado &&
                      credencial.Estado &&
                      tipoProducto.Nombre == "TRASLADO" &&
                      integrador.Estado &&
                      _context.EdadIntegracion.Any(e => e.IdIntegracion == integracion.IdIntegracionNombre && e.Estado)
                select new IntegracionDTO(
                    integracion.IdIntegracionNombre,
                    integrador.IdIntegrador,
                    integrador.Codigo,
                    integracion.Codigo,
                    integracion.Nombre,
                    integracion.EndPoint,
                    credencial.Usuario,
                    credencial.Clave,
                    new List<CredencialDTO>(),
                    new List<EdadIntegracionDTO>()   // IMPORTANTE
                 , credencial.IdTenant)
            ).ToListAsync(token).ConfigureAwait(false);

            if (!integraciones.Any())
                throw new CredencialesNoDisponibles();

            var mapIntegraciones = integraciones.ToDictionary(x => x.idIntegracion);

            // 2. CREDENCIALES AGRUPADAS
            var credencialesAgrupadas = await (
                from parametro in _context.ParametroCredencial
                join credencial in _context.Credenciales
                    on parametro.IdIntegracionCredencial equals credencial.IdIntegracionCredencial
                where parametro.Estado
                group parametro by credencial.IdIntegracion into g
                select new
                {
                    IdIntegracion = g.Key,
                    Credenciales = g.Select(p => new CredencialDTO(p.Nombre, p.Valor)).ToList()
                }
            ).ToListAsync(token).ConfigureAwait(false);

            // 3. EDADES AGRUPADAS
            var edadesAgrupadas = await (
                from e in _context.EdadIntegracion
                where e.Estado
                group e by e.IdIntegracion into g
                select new
                {
                    IdIntegracion = g.Key,
                    Edades = g.Select(e =>
                        new EdadIntegracionDTO(e.Tipo, e.Minimo, e.Maximo)
                    ).ToList()
                }
            ).ToListAsync(token).ConfigureAwait(false);

            // 4. ASIGNACIÓN O(1)
            foreach (var cred in credencialesAgrupadas)
                if (mapIntegraciones.TryGetValue(cred.IdIntegracion, out var integ))
                    integ.credencial.AddRange(cred.Credenciales);

            foreach (var edad in edadesAgrupadas)
                if (mapIntegraciones.TryGetValue(edad.IdIntegracion, out var integ))
                    integ.edades.AddRange(edad.Edades);


          

            await _cacheServicio.SetAsync(cacheKey, integraciones, TimeSpan.FromHours(12), token).ConfigureAwait(false);
 
            return true;
        }


    }

}
