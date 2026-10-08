using Azure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Busquedas.Interfaces;
using Naideth.Traslados.Aplicacion.CacheRedis.DTO;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Ciudades.Interfaces;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Traslados.Aplicacion.PreReserva;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.PreReservas.Interfaces;
using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using Naideth.Traslados.Dominio.Busquedas;
using Naideth.Traslados.Dominio.Estados;
using Naideth.Traslados.Dominio.Kernel.Exepciones;
using Naideth.Traslados.Dominio.Trayectos;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Naideth.Traslados.Aplicacion.Busquedas
{
    public sealed class BusquedaServicio : IBusquedaServicio
    {
        private readonly IComunRepositorio _comunRepositorio; 

       //private readonly ITravelKitServicio _TravelKitServicio;
       
        private readonly IINeedToursServicio _INeedToursServicio;
        private readonly CacheSettings _settings; 
        private readonly ICiudadServicio _ciudadServicio;

        private readonly ICacheRedisServicio _cacheServicio; 
        private readonly IAppLoggerServicio _logger;

        private static readonly SemaphoreSlim _semaphore= new SemaphoreSlim(100);

        private readonly IPreReservaServicio _preResevaServicio;
         

        public BusquedaServicio(
            IPreReservaServicio preResevaServicio,
            MetodosComunes comunes,
            IComunRepositorio comunRepositorio, 
            IINeedToursServicio ineedToursServicio,
            //ITravelKitServicio TravelKitServicio,   
            ICacheRedisServicio cacheServicio,
            ICiudadServicio ciudadServicio,
            IAppLoggerServicio logger,  
            IOptions<CacheSettings> options
        )
        {
            //_TravelKitServicio = TravelKitServicio;  

            _comunRepositorio = comunRepositorio;
            _cacheServicio = cacheServicio;
            _ciudadServicio = ciudadServicio;
            _logger = logger;
            _preResevaServicio = preResevaServicio; 
            _INeedToursServicio = ineedToursServicio;
            _settings = options.Value;

        }

        #region publicos
        public async Task<TrasladoRsDTO> BusquedaAsync(BusquedaDTO peticion, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            Guid idBusqueda = Guid.NewGuid();
             
           
            try
            {
                using var timeoutBusqueda = new CancellationTokenSource(TimeSpan.FromSeconds(135));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutBusqueda.Token);

                var token = linkedCts.Token;

                TrasladoRsDTO respuesta = new();

                var disponibilidades = new List<DisponibilidadDTO>();

           //     var cacheHash = GenerarCacheKey(peticion);

                var cacheKey = $"TRASLADO:BUS:{idBusqueda}";
                   
                var trayectos = peticion.trayectos.Select(t => t.ToDominio()).ToList();

                var ubicacionIata = trayectos
                    .OrderBy(t => t.Numero)
                    .SelectMany(t => new[] { t.Destino, t.Origen })
                    .FirstOrDefault(u => u.EsIATA);

                var pais = ubicacionIata != null
                    ? await _ciudadServicio.GetCiudadesPorIatasAsync(ubicacionIata.Codigo, token).ConfigureAwait(false)
                    : string.Empty;

                var busqueda = Busqueda.Crear(idBusqueda, trayectos, pais, peticion.moneda, peticion.referencia, false, peticion.Filtros.MarkUps);
                   
                  
                var pasajeros = peticion.pasajeros.Select(p =>
                    Pasajero.Crear(p.cantidad, p.tipo, p.edad, (p.upgrade != null ? new List<int> { p.upgrade.Value } : new List<int>()))
                ).ToList();

                busqueda.ListPasajeros(pasajeros);


                if (_settings.IgnorarCacheBusqueda == false)
                {
                    var cached = await _cacheServicio
                        .GetAsync<CacheDTO>(cacheKey, token)
                        .ConfigureAwait(false);
                    if (cached != null)
                    {
                        respuesta.IdBusqueda = cached.idBusqueda;

                        disponibilidades = cached.disponibilidades;



                        if (cached.integradoresStatus != null)
                        {
                            respuesta.IntegradoresStatus = cached.integradoresStatus;
                        }

                        respuesta = await ListaFiltrosAsync(respuesta, disponibilidades, peticion, cached.esParcial, token).ConfigureAwait(false);

                        respuesta = await OrdenamientoComplementosExtras(disponibilidades, peticion, respuesta,  token).ConfigureAwait(false);

                        stopwatch.Stop();

                        respuesta.Tiempo = stopwatch.Elapsed.TotalSeconds.ToString("0.00");

                        _logger.RespuestaCache(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds, disponibilidades?.Count ?? 0);
                        _logger.Fin(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds);

                        return respuesta;
                    }
 
                }

                await _semaphore.WaitAsync(cancellationToken);

                var ListaCredenciales = await _comunRepositorio
                    .CredencialesAsync(token)
                    .ConfigureAwait(false);

                if (ListaCredenciales == null)
                    throw new CredencialesNoDisponibles();
                

                respuesta.IdBusqueda = busqueda.IdBusqueda;
                 
                //Integraciones
                var tareas = await ProcesarTareasIntegradoresImplAsync(busqueda, ListaCredenciales, peticion.Filtros,  cacheKey, token).ConfigureAwait(false);


                var esParcial = false;

                if (_settings.IgnorarCacheBusqueda == false)
                {
                    esParcial = (tareas.tareasPendiente > 0);
                }

                disponibilidades = tareas.disponibilidades;


                if (disponibilidades.Count == 0)
                    return respuesta;

                await Task.WhenAll(
              _cacheServicio.UpsertAsync(cacheKey, new CacheDTO(disponibilidades, busqueda.IdBusqueda, esParcial, tareas.integradoresStatus), TimeSpan.FromMinutes(30), token),
              _cacheServicio.UpsertAsync($"TRASLADO:Busqueda:{busqueda.IdBusqueda}", peticion, TimeSpan.FromMinutes(30), token)
              );

                respuesta.IntegradoresStatus = tareas.integradoresStatus;

                _logger.Respuesta(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds, disponibilidades?.Count ?? 0);

                 respuesta = await ListaFiltrosAsync(respuesta, disponibilidades, peticion, esParcial, token).ConfigureAwait(false);

                respuesta = await OrdenamientoComplementosExtras(disponibilidades, peticion, respuesta, token).ConfigureAwait(false);

                stopwatch.Stop();

                respuesta.Tiempo = stopwatch.Elapsed.TotalSeconds.ToString("0.00");

                _logger.Respuesta(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds, disponibilidades?.Count ?? 0);
                _logger.Fin(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds);



                return respuesta; 
                
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task<TrasladoPreReservaRsDTO> TrasladoIncluidaAsync(BusquedaDTO peticion, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            Guid idBusqueda = Guid.NewGuid();
            try
            {  
                using var timeoutBusqueda = new CancellationTokenSource(TimeSpan.FromSeconds(135));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutBusqueda.Token);

                var token = linkedCts.Token;

                TrasladoIncluidaRsDTO respuesta = new();

                DisponibilidadDTO disponibilidad = null;

                //var cacheHash = GenerarCacheKey(peticion);

                var cacheKey = $"TRASLADO:BUS:{idBusqueda}";

                var trayectos = peticion.trayectos.Select(t => t.ToDominio()).ToList();

                var ubicacionIata = trayectos
                    .OrderBy(t => t.Numero)
                    .SelectMany(t => new[] { t.Destino, t.Origen })
                    .FirstOrDefault(u => u.EsIATA);

                var pais = ubicacionIata != null
                    ? await _ciudadServicio.GetCiudadesPorIatasAsync(ubicacionIata.Codigo, token).ConfigureAwait(false)
                    : string.Empty;

                var busqueda = Busqueda.Crear(idBusqueda, trayectos, pais, peticion.moneda, peticion.referencia, true, peticion.Filtros.MarkUps);
                   
                  
                var pasajeros = peticion.pasajeros.Select(p =>
                    Pasajero.Crear(p.cantidad, p.tipo, p.edad, (p.upgrade != null ? new List<int> { p.upgrade.Value } : new List<int>()))
                ).ToList();

                busqueda.ListPasajeros(pasajeros);


                if (_settings.IgnorarCacheBusqueda == false)
                {
                    var cached = await _cacheServicio
                        .GetAsync<CacheDTO>(cacheKey, token)
                        .ConfigureAwait(false);
                    if (cached != null)
                    {
                        respuesta.IdBusqueda = cached.idBusqueda;

                        disponibilidad = cached.disponibilidades.FirstOrDefault();
                             
                        stopwatch.Stop();
                          
                        _logger.RespuestaCache(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds, (disponibilidad!=null?1:0));
                        _logger.Fin(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds);


                        return await _preResevaServicio.PreReservaAsync(new PreReservas.DTO.Rq.PreReservaDTO(idBusqueda, Guid.Parse(respuesta.Disponibilidad.Id), peticion.moneda), cancellationToken).ConfigureAwait(false);

                      //  return respuesta;
                    }
 
                }

                await _semaphore.WaitAsync(cancellationToken);

                var ListaCredenciales = await _comunRepositorio
                    .CredencialesAsync(token)
                    .ConfigureAwait(false);

                if (ListaCredenciales == null)
                    throw new CredencialesNoDisponibles();
                

                respuesta.IdBusqueda = busqueda.IdBusqueda;
                 
                //Integraciones
                var tareas = await ProcesarTareasIntegradoresImplAsync(busqueda, ListaCredenciales, peticion.Filtros, cacheKey, token).ConfigureAwait(false);


                var esParcial = false;

                if (_settings.IgnorarCacheBusqueda == false)
                {
                    esParcial = (tareas.tareasPendiente > 0);
                }

                disponibilidad = tareas.disponibilidades?.FirstOrDefault() ?? null;

                respuesta.Disponibilidad = disponibilidad;
 
                await Task.WhenAll(
                _cacheServicio.UpsertAsync(cacheKey, new CacheDTO(new List<DisponibilidadDTO> { disponibilidad }, busqueda.IdBusqueda, false, null), TimeSpan.FromMinutes(30), token),
                _cacheServicio.UpsertAsync($"TRASLADO:Busqueda:{busqueda.IdBusqueda}", peticion, TimeSpan.FromMinutes(30), token)
                );



                _logger.Respuesta(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds, (disponibilidad!=null?1: 0)); 
                stopwatch.Stop();
                 
                 
                _logger.Fin(peticion.referencia, idBusqueda, "Busqueda", stopwatch.Elapsed.TotalSeconds);


                return await _preResevaServicio.PreReservaAsync(new PreReservas.DTO.Rq.PreReservaDTO(respuesta.IdBusqueda, Guid.Parse(respuesta.Disponibilidad.Id), peticion.moneda), cancellationToken).ConfigureAwait(false);

                //return respuesta; 
                
            }
            finally
            {
                _semaphore.Release();
            }
        }


        #endregion publicos
        #region privados 

        private async Task<TrasladoRsDTO> ListaFiltrosAsync(
    TrasladoRsDTO resultado,
    List<DisponibilidadDTO> disponibilidades,
    BusquedaDTO peticion,
    bool esParcial,
    CancellationToken token)
        {
              
            //vuelos.MatrizDisponibilidades = await ObtenerMatrizMejoresOpciones(disponibilidades, token).ConfigureAwait(false);

            var precios = CalcularRangoDePrecios(disponibilidades); 
            var tFuentes = ObtenerFuentesAsync(disponibilidades, token);
            var tNombre = ObtenerNombreAsync(disponibilidades, token);
            await Task.WhenAll(tFuentes, tNombre);

            var listaordenamiento = ObtenerListaOrdenamiento();


            var fuentes = tFuentes.Result;
            var nombres = tNombre.Result;
       

            resultado.Filtros = new FiltroResponseDTO(
               precios,
               fuentes,
               nombres,
               listaordenamiento
                );

            resultado.EsParcial = esParcial;

            resultado.Disponibilidades = disponibilidades;

            return resultado;
        }

        private List<FiltroGeneralResponseDTO> ObtenerListaOrdenamiento()
        {

            return new List<FiltroGeneralResponseDTO>{
            new FiltroGeneralResponseDTO( "PRECIOMENOR",  "Precio Menor", 0),
            new FiltroGeneralResponseDTO( "PRECIOMAYOR",  "precio Mayor", 0),  
                };

        }
        private async Task<(int tareasPendiente, List<DisponibilidadDTO> disponibilidades, List<IntegradorStatusDTO> integradoresStatus)> ProcesarTareasIntegradoresImplAsync(
Busqueda busqueda,
List<IntegracionDTO> listaCredenciales,
FiltroRequestDTO? filtros, 
string cacheKey,
CancellationToken token)
        {
            var disponibilidades = new List<DisponibilidadDTO>();
            var integradoresStatus = new List<IntegradorStatusDTO>(); 
            var fechaInicio = DateTime.UtcNow;
            var tareasPendientes = await CrearTareasIntegradoresImplAsync(busqueda, listaCredenciales, filtros, token).ConfigureAwait(false);

            // Crear diccionario para mapear tarea -> nombre
            var tareasConNombres = tareasPendientes.ToDictionary(t => t.tarea, t => t.integracion);

            // Inicializar status para cada integrador
            foreach (var (integracion, _) in tareasPendientes)
            {
                integradoresStatus.Add(new IntegradorStatusDTO(integracion, fechaInicio));
            }

            // 🔹 Optimización: Dictionary para lookup O(1) en lugar de First() O(n)
            var integradoresStatusDict = integradoresStatus.ToDictionary(s => s.Nombre);

            // 🔹 Optimización: Lista separada de tareas para evitar Select() en cada iteración
            var tareasActivas = tareasPendientes.Select(t => t.tarea).ToList();

            while (tareasActivas.Count > 0 && !token.IsCancellationRequested)
            {
                // Obtener la primera tarea completada
                var tareaCompletada = await Task.WhenAny(tareasActivas)
                    .ConfigureAwait(false);

                // Obtener el nombre del integrador
                var nombreIntegrador = tareasConNombres[tareaCompletada];
                var status = integradoresStatusDict[nombreIntegrador];

                try
                {
                    // Remover la tarea de la lista
                    tareasActivas.Remove(tareaCompletada);

                    // Esperar el resultado
                    var result = await tareaCompletada.ConfigureAwait(false);

                    if (result.Estado != Estado.ERROR)
                    {
                        // 🔹 Optimización: Evitar null checks redundantes
                        int cantidad = 0;
                        if (result?.Data?.Disponibilidades != null)
                        {
                            var disponibilidadesResult = result.Data.Disponibilidades;
                            cantidad = disponibilidadesResult.Count;
                        }

                        status.MarcarCompletada(cantidad);

                        if (cantidad > 0)
                        {
                            disponibilidades.AddRange(result.Data.Disponibilidades);

                            if (_settings.IgnorarCacheBusqueda == false)
                            {
                                break; // 🚀 sales apenas tienes una válida
                            }
                        }
                    }
                    else
                    {

                        status.MarcarError(result.Estado.ToString(), 0);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorLog(busqueda.Referencia, busqueda.IdBusqueda, "Busqueda", nombreIntegrador, $"Error en tarea del integrador {nombreIntegrador}: {ex}");
                    status.MarcarError(ex.Message, 0);
                }
            }

            if (_settings.IgnorarCacheBusqueda == false)
            {
                // 🔥 BACKGROUND
                _ = ProcesarRestantesAsync(tareasPendientes, tareasConNombres, integradoresStatus, cacheKey, busqueda.IdBusqueda).ConfigureAwait(false);

            }

            return (tareasActivas.Count, disponibilidades, integradoresStatus);

        }

      



        private async Task<List<(string integracion, Task<BusquedaResponseDTO> tarea)>> CrearTareasIntegradoresImplAsync(
Busqueda busqueda,
List<IntegracionDTO> listaCredenciales,
FiltroRequestDTO? filtros,
CancellationToken token)
        {
            CancellationToken CrearTokenIntegrador()
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(TimeSpan.FromSeconds(1500));
                return cts.Token;
            }

            var listaTareas = new List<(string, Task<BusquedaResponseDTO>)>();

            //listaTareas.Add(("TravelKit", _TravelKitServicio.BusquedaAsync(busqueda, listaCredenciales, filtros, CrearTokenIntegrador())));
            listaTareas.Add(("INeedTours", _INeedToursServicio.BusquedaAsync(busqueda, listaCredenciales, filtros, CrearTokenIntegrador())));


            return listaTareas;

        }


        private async Task ProcesarRestantesAsync(
          List<(string integracion, Task<BusquedaResponseDTO> tarea)> tareasPendientes,
           Dictionary<Task<BusquedaResponseDTO>, string> tareasConNombres,
           List<IntegradorStatusDTO> integradoresStatus,
           string cacheKey,
           Guid idBusqueda)
        {
            try
            {
                var tareas = tareasPendientes.ToList();

                while (tareasPendientes.Count > 0)
                {
                    // Obtener la primera tarea completada
                    var tareaCompletada = await Task.WhenAny(tareasPendientes.Select(t => t.tarea))
                        .ConfigureAwait(false);

                    // Obtener el nombre del integrador
                    var nombreIntegrador = tareasConNombres[tareaCompletada];
                    var status = integradoresStatus.First(s => s.Nombre == nombreIntegrador);

                    try
                    {
                        // Remover la tarea de la lista
                        tareasPendientes.RemoveAll(t => t.tarea == tareaCompletada);

                        // Esperar el resultado
                        var result = await tareaCompletada.ConfigureAwait(false);

                        int cantidad = result?.Data?.Disponibilidades?.Count ?? 0;

                        status.MarcarCompletada(cantidad);

                        if (cantidad > 0)
                        {

                            await _cacheServicio.AppendAsync(
                                cacheKey,
                                result.Data.Disponibilidades,
                                TimeSpan.FromMinutes(30)
                            );

                        }

                        await _cacheServicio.UpdateIntegradoresStatusAsync(cacheKey, integradoresStatus, TimeSpan.FromMinutes(30));

                    }
                    catch (Exception ex)
                    {
                        status.MarcarError(ex.Message, 0);
                        await _cacheServicio.UpdateIntegradoresStatusAsync(cacheKey, integradoresStatus, TimeSpan.FromMinutes(30));

                    }
                }

            }
            finally
            {
                await _cacheServicio.UpdateEstadoAsync(cacheKey, false, TimeSpan.FromMinutes(30));
            }
        }


        private string GenerarCacheKey(BusquedaDTO peticion)
        {
            var cacheRaw =
                $"{peticion.trayectos.SerializeObjectText()}:{peticion.moneda}:" +
                $"{peticion.pasajeros.SerializeObjectText()}" + 
                 peticion.referencia;

            var cacheHash = Convert.ToHexString(
                       SHA256.HashData(Encoding.UTF8.GetBytes(cacheRaw))
                   );

            return cacheHash;
        }


        private static async Task<List<DisponibilidadDTO>> PaginarAsync(List<DisponibilidadDTO> all, BusquedaDTO peticion, int totalItems, int pagina, CancellationToken cancellationToken)
        {

            // Calcular los índices para la paginación
            int skip = (pagina - 1) * peticion.registrosPagina;

            var paginaHotels = all
                .Skip(skip)
                .Take(peticion.registrosPagina)
                .ToList();
             
            return await Task.FromResult(paginaHotels);

        }


        private static async Task<List<DisponibilidadDTO>> AplicarOrdenamientoAsync(List<DisponibilidadDTO> data,bool mayor,  CancellationToken cancellationToken)
        {
              
            // Aplica el ordenamiento dinámico.
            var sortedData = (mayor ?
                 data.OrderBy(dt=> dt.Precio.Total.Total).ToList()
                : data.OrderByDescending(dt => dt.Precio.Total.Total).ToList());

            // Simular operación asincrónica.
            return await Task.FromResult(sortedData).ConfigureAwait(false);
        }
      
      
        private static FiltroPrecioResponseDTO CalcularRangoDePrecios(List<DisponibilidadDTO> data)
        { 
            return new FiltroPrecioResponseDTO(
                data.Min(m=> m.Precio.Total.Total),
                data.Max(m => m.Precio.Total.Total)
            );
        }
 
       
         private Task<List<FiltroGeneralResponseDTO>> ObtenerFuentesAsync(
         List<DisponibilidadDTO> data,
         CancellationToken cancellationToken)
        {
            var resultado = data   
                .Select(p => new FiltroGeneralResponseDTO(
                    p.Source.ToString() 
                    ,p.SourceName.ToString()
                    ,0
                    )).Distinct()
                .ToList();

            return Task.FromResult(resultado);
        }

        private Task<List<FiltroGeneralResponseDTO>> ObtenerNombreAsync(
         List<DisponibilidadDTO> data,
         CancellationToken cancellationToken)
        {
            var resultado = data
                .Select(p => new FiltroGeneralResponseDTO(
                    p.Plan.ToString()
                    , p.Plan.ToString()
                    , 0
                    )).Distinct()
                .ToList();

            return Task.FromResult(resultado);
        }

        private static List<DisponibilidadDTO> AplicarOrdenamiento(
      List<DisponibilidadDTO> data,
      string? ordenamiento)
        {
            if (data == null || data.Count == 0)
                return new List<DisponibilidadDTO>();

            // Normalizar el criterio de ordenamiento
            var criterio = ordenamiento?.ToUpperInvariant()?.Trim() ?? "PRECIOMENOR";

            if (criterio == string.Empty)
                criterio = "PRECIOMENOR";

            // Aplicar ordenamiento según el criterio usando propiedades existentes
            return criterio switch
            {
                // 🔸 Ordenamiento por Precio
                "PRECIOMENOR" or "PRECIO_ASC" =>
                    data.OrderBy(r => r?.Precio?.Total?.Total ?? decimal.MaxValue).ToList(),

                "PRECIOMAYOR" or "PRECIO_DESC" =>
                    data.OrderByDescending(r => r?.Precio?.Total?.Total ?? decimal.MinValue).ToList(),
                // 🔸 Default: Por precio menor
                _ => data.OrderBy(r => r?.Precio?.Total?.Total ?? decimal.MaxValue).ToList()
            };
        }


        private static FiltroResponseDTO MarcarFiltrosAplicados(FiltroResponseDTO filtroResponse, BusquedaDTO peticion, CancellationToken cancellationToken)
        {
            List<FiltroGeneralResponseDTO> fuentes = null;
            FiltroPrecioResponseDTO precio = null;
            List<FiltroGeneralResponseDTO> ordenamientos = null;
            List<FiltroGeneralResponseDTO> nombres = null;

            var filtros = peticion.Filtros; 

            if (filtros?.Precio != null)
            {
                precio = new FiltroPrecioResponseDTO(filtroResponse.precios.minimo, filtroResponse.precios.maximo);
            }
             
            if (filtros?.Fuentes?.Any() == true)
            {
                fuentes = filtroResponse.fuentes.Select(f => new FiltroGeneralResponseDTO(f.codigo, f.nombre, (filtros.Fuentes.Any(req => f.codigo == req) ? 1 : 0))).ToList();

            }

            if (filtros?.Nombre?.Any() == true)
            {
                nombres = filtroResponse.nombres.Select(n => new FiltroGeneralResponseDTO(n.codigo, n.nombre, (filtros.Nombre.Any(req => n.codigo == req) ? 1 : 0))).ToList();

            }

            if (filtros?.Ordenamiento?.Any() == true)
            {
                ordenamientos = filtroResponse.ordenamientos.Select(o => new FiltroGeneralResponseDTO(o.codigo, o.nombre, (filtros.Ordenamiento == o.codigo ? 1 : 0))).ToList();

            }


            // 3️⃣ Retornar resultados filtrados
            return new FiltroResponseDTO( 
                (precio == null ? filtroResponse.precios : precio), 
                (fuentes == null ? filtroResponse.fuentes : fuentes),
                (nombres == null ? filtroResponse.nombres : nombres),
                (ordenamientos == null ? filtroResponse.ordenamientos : ordenamientos)
                );
        }

        private static List<DisponibilidadDTO> AplicarFiltros(
     List<DisponibilidadDTO> data,
     BusquedaDTO peticion,
     CancellationToken cancellationToken)
        {
            var query = data.AsEnumerable();
            var filtros = peticion.Filtros;

            //se condicciona a que si es publica solo se ofresca publica

            // 🔹 PRECIO
            if (filtros?.Precio != null)
            {
                query = query.Where(r =>
                    r.Precio.Total.Total >= filtros.Precio.minimo &&
                    r.Precio.Total.Total <= filtros.Precio.maximo);
            }

            // 🔹 FUENTES
            if (filtros?.Fuentes?.Any() == true)
            {
                query = query.Where(r =>
                    filtros.Fuentes.Contains(r.Source));
            }

            // 🔹 NOMBRE
            if (filtros?.Nombre?.Any() == true)
            {
                query = query.Where(r =>
                    filtros.Nombre.Contains(r.Plan));
            }
            return query.ToList();
        }


        private async Task<TrasladoRsDTO> OrdenamientoComplementosExtras(
List<DisponibilidadDTO> disponibilidades,
BusquedaDTO peticion,
TrasladoRsDTO resultado, 
CancellationToken token)
        {
             

            var totalItems = disponibilidades.Count;

            disponibilidades = AplicarOrdenamiento(disponibilidades, peticion.Filtros.Ordenamiento);

            disponibilidades = AplicarFiltros(disponibilidades, peticion, token);

            resultado.Filtros = MarcarFiltrosAplicados(resultado.Filtros, peticion, token);


            var totalPaginas = Math.Max(1,
                (int)Math.Ceiling((double)totalItems / peticion.registrosPagina));

            var pagina = Math.Clamp(peticion.pagina, 1, totalPaginas);

            resultado.Disponibilidades = Paginar(
                disponibilidades,
                peticion,
                totalItems,
                pagina
            );

            resultado.Pagina = pagina;
            resultado.RegistrosPagina = peticion.registrosPagina;
            resultado.TotalItems = totalItems;

            return resultado;
        }


        private static List<DisponibilidadDTO> Paginar(List<DisponibilidadDTO> all, BusquedaDTO peticion, int totalItems, int pagina)
        {
            // Calcular los índices para la paginación
            int skip = (pagina - 1) * peticion.registrosPagina;

            return all
                .Skip(skip)
                .Take(peticion.registrosPagina)
                .ToList();
        }



        #endregion privados
    }
}
