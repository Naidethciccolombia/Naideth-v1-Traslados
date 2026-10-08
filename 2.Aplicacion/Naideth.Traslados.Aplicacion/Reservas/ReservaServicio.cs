using Azure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Ciudades.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Reservas.Interfaces;
using Naideth.Traslados.Dominio.Busquedas;
using Naideth.Traslados.Dominio.Kernel.Exepciones;
using Naideth.Traslados.Dominio.Trayectos;
using Naideth.Central.Dominio.Aerolineas;
using Naideth.Central.Dominio.Aeropuertos;
using Naideth.Central.Dominio.Hoteles;
using Naideth.Central.Dominio.Kernel.Extensiones;
using Naideth.Central.Dominio.Paises;
using Naideth.Central.Dominio.TipoMaletas; 
using Newtonsoft.Json.Linq;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces;

namespace Naideth.Traslados.Aplicacion.Reservas
{
    public sealed class ReservaServicio : IReservaServicio
    {
        private readonly IComunRepositorio _comunRepositorio;

        //private readonly ITravelKitServicio _TravelKitServicio;
        private readonly IINeedToursServicio _INeedToursServicio;
        private readonly ICacheRedisServicio _cacheServicio;
        private readonly IAppLoggerServicio _logger;
        private readonly ICiudadServicio _ciudadServicio;

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(100);
         

        public ReservaServicio(ICiudadServicio ciudadServicio, IComunRepositorio comunRepositorio, IINeedToursServicio ineedToursServicio, ICacheRedisServicio cacheServicio, IAppLoggerServicio logger
        )
        {
            //_TravelKitServicio = travelKitServicioServicio;
            _INeedToursServicio = ineedToursServicio;
            _comunRepositorio = comunRepositorio;
            _cacheServicio = cacheServicio;
            _ciudadServicio = ciudadServicio;
            _logger = logger;
              

        }

        #region publicos 
        public async Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, CancellationToken TokenCancelacion)
        {

            ReservaResponseDTO result = null;
             
           
                var cacheKey = $"TRASLADO:PreReserva:{peticion.IdPreReserva}";

                //obtener cache de disponibilidades
                var cachedResult = await _cacheServicio.GetAsync<(DisponibilidadDTO disponibilidad, BusquedaDTO busqueda) > (cacheKey, TokenCancelacion).ConfigureAwait(false);
                 

                if (cachedResult.busqueda == null)
                { 
                    throw new CacheBusquedaNoEncontrada();
                }

                string SourceName = string.Empty;

      
                if (cachedResult.disponibilidad != null)
                {
         
                  var disponibilidadSeleccionada = cachedResult.disponibilidad;
                     
                    SourceName = disponibilidadSeleccionada.SourceName;

                  var ListaCredenciales = await _comunRepositorio.CredencialesAsync(TokenCancelacion).ConfigureAwait(false);

                    if (ListaCredenciales == null) throw new CredencialesNoDisponibles();


                    var trayectos = cachedResult.busqueda.trayectos.Select(t => t.ToDominio()).ToList();

                    var ubicacionIata = trayectos
                        .OrderBy(t => t.Numero)
                        .SelectMany(t => new[] { t.Destino, t.Origen })
                        .FirstOrDefault(u => u.EsIATA);

                    var pais = ubicacionIata != null
                        ? await _ciudadServicio.GetCiudadesPorIatasAsync(ubicacionIata.Codigo, TokenCancelacion).ConfigureAwait(false)
                        : string.Empty;

                    var busqueda = Busqueda.Crear(Guid.NewGuid(), trayectos, pais, cachedResult.busqueda.moneda, cachedResult.busqueda.referencia, false, cachedResult.busqueda.Filtros.MarkUps);

                    var pasajeros = cachedResult.busqueda.pasajeros.Select(p =>
                        Pasajero.Crear(p.cantidad, p.tipo, p.edad, (p.upgrade != null ? new List<int> { p.upgrade.Value } : new List<int>()))
                    ).ToList();

                    busqueda.ListPasajeros(pasajeros);

                    result = SourceName switch
                        {
                            //"TravelKit" => await _TravelKitServicio.ReservaAsync(peticion, busqueda, cachedResult.disponibilidad, ListaCredenciales, TokenCancelacion).ConfigureAwait(false),
                            "INT" => await _INeedToursServicio.ReservaAsync(peticion, busqueda, cachedResult.disponibilidad, ListaCredenciales, TokenCancelacion).ConfigureAwait(false),
                            _ => null
                        };

                      
                 
                }
                
           
              
            return result;
        }

        #endregion publicos
    }
}
