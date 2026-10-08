using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Naideth.Central.Dominio.Ciudades;
using Naideth.Central.Dominio.CiudadesAeropuertos;
using Naideth.Traslados.Aplicacion.Ciudades.Interfaces; 
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces;


namespace Naideth.Traslados.Infrastructura.Ciudades
{
    internal class CiudadRepositorio : ICiudadRepositorio
    {
        private readonly IApplicationDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;

        public CiudadRepositorio(IApplicationDbContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
        }
        public async Task<string> GetCiudadesPorIatasAsync(
    string id,
    CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                return string.Empty;

            var tipo=(id.Contains('-') ? "CIUDAD" : "AEROPUERTO");

            string codigoPais=string.Empty;

            if (tipo == "CIUDAD")
            {
              codigoPais = await (
              from a in _context.Ciudades.AsNoTracking()
              join c in _context.Paises.AsNoTracking()  on a.IdPais equals c.IdPais 
              where a.IdCiudad.ToString()==id
              select c.IATA)
              .FirstOrDefaultAsync(cancellationToken);
                  
            }

            if (tipo == "AEROPUERTO")
            {
                
                codigoPais = await (
             from ae in _context.Aeropuertos.AsNoTracking()
             join ca in _context.CiudadAeropuerto.AsNoTracking() on ae.IdAeropuerto equals ca.IdAeropuerto
             join ci in _context.Ciudades.AsNoTracking() on ca.IdCiudad equals ci.IdCiudad
             join c in _context.Paises.AsNoTracking() on ci.IdPais equals c.IdPais
             where ae.Codigo == id
             select c.IATA)
             .FirstOrDefaultAsync(cancellationToken);
            }
             
            return codigoPais;
        }
       
        //public async Task<((Ciudad ciudad, List<string> aeropuertos), (Ciudad ciudad, List<string> aeropuertos))> GetTrayectoPorIatasAsync(TrayectoDTO trayecto, CancellationToken cancellationToken)
        //{
        //    var origen = await GetBusquedaPorIatasAsync(trayecto.iataOrigen,cancellationToken).ConfigureAwait(false);
        //    var destino = await GetBusquedaPorIatasAsync(trayecto.iataDestino, cancellationToken).ConfigureAwait(false);


        //     return (origen, destino);
        //}

        private async Task<(Ciudad ciudad, List<string> aeropuertos)> GetBusquedaPorIatasAsync(
            string iata, CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();


                // Consulta 1: Buscar ciudades por su código IATA
                var ciudadesPorIata = await (
                from c in context.Ciudades.AsNoTracking()
                where c.Iata != null && c.Iata == iata && c.Estado
                select new
                {
                    Ciudad = c,
                    IataBuscado = c.Iata,
                    Aeropuertos = (
                        from ca in context.CiudadAeropuerto.AsNoTracking()
                        join a in context.Aeropuertos.AsNoTracking()
                            on ca.IdAeropuerto equals a.IdAeropuerto
                        where ca.IdCiudad == c.IdCiudad   && a.Codigo != c.Iata 
                        && a.Estado
                        select a.Codigo
                    ).ToList()
                }
            ).ToListAsync(cancellationToken);

                // Consulta 2: Buscar ciudades por código de aeropuerto
                var ciudadesPorAeropuerto = await (
                    from a in context.Aeropuertos.AsNoTracking()
                    join ca in context.CiudadAeropuerto.AsNoTracking() on a.IdAeropuerto equals ca.IdAeropuerto
                    join c in context.Ciudades.AsNoTracking() on ca.IdCiudad equals c.IdCiudad
                    where a.Codigo== iata && c.Estado && a.Estado
                    select new
                    {
                        Ciudad = c,
                        IataBuscado = c.Iata,
                        Aeropuertos = (
                            from ca2 in context.CiudadAeropuerto.AsNoTracking()
                            join a2 in context.Aeropuertos.AsNoTracking()
                                on ca2.IdAeropuerto equals a2.IdAeropuerto
                            where ca2.IdCiudad == c.IdCiudad && a2.Estado
                            //&& a2.Codigo != iata
                            select a2.Codigo
                        ).ToList()
                    }
                ).ToListAsync(cancellationToken);

                // Combinar ambas consultas
                var datos = ciudadesPorIata
                    .Concat(ciudadesPorAeropuerto)
                    .GroupBy(x => x.IataBuscado)
                    .Select(g => g.OrderByDescending(a => a.Aeropuertos.Count).First())
                    .ToList();

                // También buscar aeropuertos que no tienen ciudad asociada
                var aeropuertosSinCiudad = await (
                    from a in context.Aeropuertos.AsNoTracking()
                    where iata == a.Codigo && a.Estado
                        && !context.CiudadAeropuerto.Any(ca => ca.IdAeropuerto == a.IdAeropuerto) 
                    select new
                    {
                        Ciudad = (Ciudad)null,
                        Aeropuertos = new List<string> { a.Codigo },
                        CodigoAeropuerto = a.Codigo
                    }
                ).ToListAsync(cancellationToken);

                var resultado = new List<(Ciudad Ciudad, List<string> Aeropuertos)>();


                // Buscar en los datos combinados (ciudad o aeropuerto)
                var match = datos.FirstOrDefault(x => (x.IataBuscado == iata) || (x.Aeropuertos.Any(ae => ae == iata)));

                if (match != null)
                {
                    // Retornar la ciudad encontrada con su lista de aeropuertos
                    var ciudadData = (match.Ciudad, match.Aeropuertos.Where(a => a != match.Ciudad.Iata).ToList() ?? new List<string>());
                    return ciudadData;


                }
                else
                {
                    // Buscar en aeropuertos sin ciudad
                    var aeropuertoSinCiudad = aeropuertosSinCiudad.FirstOrDefault(x => x.CodigoAeropuerto == iata);


                    if (aeropuertoSinCiudad != null)
                    {
                        // Crear ciudad temporal con el código del aeropuerto
                        return (Ciudad.Crear(Guid.Empty, iata, iata, true), new List<string>());
                    }
                    else
                    {
                        // No se encontró ni ciudad ni aeropuerto - crear ciudad temporal
                        return (Ciudad.Crear(Guid.Empty, iata, iata, true), new List<string>());
                    }

                }
            }

        }

        //public async Task<Dictionary<string, Ciudad>> GetCiudadesPorIatasAsync(
        // List<string> iatas,
        // CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (iatas == null || iatas.Count == 0)
        //            return new Dictionary<string, Ciudad>();

        //        var diccionario = await (
        //            from a in _context.Aeropuertos.AsNoTracking()
        //            join ca in _context.CiudadAeropuerto.AsNoTracking()
        //                on a.IdAeropuerto equals ca.IdAeropuerto
        //            join c in _context.Ciudades.AsNoTracking()
        //                on ca.IdCiudad equals c.IdCiudad 
        //            where a.Estado
        //                && !string.IsNullOrEmpty(c.Iata) 
        //                && (iatas.Contains(a.Codigo)|| iatas.Contains(c.Iata))
        //            select new
        //            {
        //                a.Codigo,
        //                Ciudad = c
        //            }
        //        )
        //        .ToDictionaryAsync(x => x.Codigo, x => x.Ciudad, cancellationToken)
        //        .ConfigureAwait(false);

        //        return diccionario;
        //    }
        //    catch
        //    {

        //    }
        //    return null;
        //}



    }
}
