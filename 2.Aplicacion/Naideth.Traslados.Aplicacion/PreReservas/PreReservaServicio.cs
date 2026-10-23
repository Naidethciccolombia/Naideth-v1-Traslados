using Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Ciudades.Interfaces;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.PreReservas.Interfaces;
using Naideth.Traslados.Dominio.Busquedas;
using Naideth.Traslados.Dominio.Estados;
using Naideth.Traslados.Dominio.Kernel.Exepciones;
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
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Naideth.Traslados.Aplicacion.PreReserva
{
    public sealed class PreReservaServicio : IPreReservaServicio
    {
        private readonly IComunRepositorio _comunRepositorio;

        //private readonly ITravelKitServicio _TravelKitServicio;
        private readonly IINeedToursServicio _INeedToursServicio;
        private readonly ICacheRedisServicio _cacheServicio;
        private readonly IAppLoggerServicio _logger;
        private readonly ICiudadServicio _ciudadServicio;

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(100);

        private readonly IServiceProvider _serviceProvider;
         

        public PreReservaServicio(IServiceProvider serviceProvider, ICiudadServicio ciudadServicio, IComunRepositorio comunRepositorio, IINeedToursServicio iNeedToursServicio, ICacheRedisServicio cacheServicio, IAppLoggerServicio logger
        )
        {
            //_TravelKitServicio = travelKitServicioServicio;
            _INeedToursServicio = iNeedToursServicio;
            _comunRepositorio = comunRepositorio;
            _cacheServicio = cacheServicio;
            _ciudadServicio = ciudadServicio;
            _logger = logger;
            _serviceProvider = serviceProvider;  
              

        }

        #region publicos 
        public async Task<TrasladoPreReservaRsDTO> PreReservaAsync(PreReservaDTO peticion, CancellationToken TokenCancelacion)
        {
            Guid idPreReserva = Guid.NewGuid();
             
            TrasladoPreReservaRsDTO result = new();

                var cachedBusquedaResult = await _cacheServicio.GetAsync<BusquedaDTO>($"TRASLADO:Busqueda:{peticion.IdBusqueda}", TokenCancelacion).ConfigureAwait(false);

                if (cachedBusquedaResult == null)
                {
                    
                    throw new CacheBusquedaNoEncontrada();
                }
                 

            try
            {

                var cacheKey = $"TRASLADO:BUS:{peticion.IdBusqueda}";

                //obtener cache de disponibilidades
                var cachedResult = await _cacheServicio.GetAsync<CacheDTO>(cacheKey, TokenCancelacion).ConfigureAwait(false);
                 

                if (cachedResult == null)
                { 
                    throw new CacheBusquedaNoEncontrada();
                }

                string SourceName = string.Empty;

      
                if (cachedResult != null)
                {
         
                  var disponibilidadSeleccionada = cachedResult.disponibilidades.Where(r => r.Id==peticion.IdTarifa.ToString());
                     
                    SourceName = disponibilidadSeleccionada.FirstOrDefault().SourceName;

                  var listaCredenciales = await _comunRepositorio.CredencialesAsync(TokenCancelacion).ConfigureAwait(false);

                    if (listaCredenciales == null) throw new CredencialesNoDisponibles();


                    var trayectos = cachedBusquedaResult.trayectos.Select(t => t.ToDominio()).ToList();

                    var ubicacionIata = trayectos
                        .OrderBy(t => t.Numero)
                        .SelectMany(t => new[] { t.Destino, t.Origen })
                        .FirstOrDefault(u => u.EsIATA);

                    var pais = ubicacionIata != null
                        ? await _ciudadServicio.GetCiudadesPorIatasAsync(ubicacionIata.Codigo, TokenCancelacion).ConfigureAwait(false)
                        : string.Empty;

                    var busqueda = Busqueda.Crear(peticion.IdBusqueda, trayectos, pais, cachedBusquedaResult.moneda, cachedBusquedaResult.referencia, false, cachedBusquedaResult.Filtros.MarkUps);
   
                    var    respuesta = SourceName switch
                        {
                            //"TravelKit" => await _TravelKitServicio.PreReservaAsync(peticion, busqueda, cachedResult, ListaCredenciales, TokenCancelacion).ConfigureAwait(false),
                            "INT" => await _INeedToursServicio.PreReservaAsync(peticion, busqueda, cachedResult, listaCredenciales, TokenCancelacion).ConfigureAwait(false),
                          _ => null
                        };
                     
                    result.IdPreReserva = idPreReserva;
                    result.Disponibilidad = respuesta?.Data?.Disponibilidades?.FirstOrDefault();

                    await _cacheServicio.SetAsync($"TRASLADO:PreReserva:{idPreReserva}", (respuesta?.Data?.Disponibilidades?.FirstOrDefault(), cachedBusquedaResult), TimeSpan.FromMinutes(30), TokenCancelacion).ConfigureAwait(false);
                     
                }
                
                 
            }  
            finally
            {
                _semaphore.Release();
            }
              
            return result;
        }

        public async Task<TrasladoPreReservaRsDTO> PreReservaSeleccionadaAsync(PreReservaSeleccionadaDTO peticion, CancellationToken TokenCancelacion)
        {
            var cachedResult = await _cacheServicio.GetAsync<(DisponibilidadDTO disponibilidad, BusquedaDTO busqueda)>($"TRASLADO:PreReserva:{peticion.IdPreReserva}", TokenCancelacion).ConfigureAwait(false);

            var dispCache = cachedResult.disponibilidad;

            if (dispCache == null)
            {
                throw new CacheBusquedaNoEncontrada();
            }

            return new TrasladoPreReservaRsDTO
            {
                IdPreReserva = peticion.IdPreReserva.Value,
                Disponibilidad = dispCache
            };
        }


        #endregion publicos
        }
    }
