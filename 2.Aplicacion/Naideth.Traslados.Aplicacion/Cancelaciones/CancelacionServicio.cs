 
using Microsoft.Extensions.Logging; 
using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs;
using Naideth.Traslados.Aplicacion.Cancelaciones.Interfaces;
using Naideth.Traslados.Aplicacion.Ciudades.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Traslados.Dominio.Estados;
using Naideth.Traslados.Dominio.Kernel.Exepciones; 
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;

namespace Naideth.Traslados.Aplicacion.Cancelaciones
{
    public sealed class CancelacionServicio : ICancelacionServicio
    {
        private readonly IComunRepositorio _comunRepositorio;

        //private readonly ITravelKitServicio _travelKitServicio;
        private readonly IINeedToursServicio _INeedToursServicio;

        private readonly IAppLoggerServicio _logger; 

        private readonly IServiceProvider _serviceProvider;

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(100);


        public CancelacionServicio(IServiceProvider serviceProvider, ICiudadServicio ciudadServicio, IComunRepositorio comunRepositorio, IINeedToursServicio iNeedToursServicio, ICacheRedisServicio cacheServicio, IAppLoggerServicio logger
        )
        {
            _serviceProvider = serviceProvider;
            //_travelKitServicio = travelKitServicioServicio;
            _INeedToursServicio = iNeedToursServicio;
            _comunRepositorio = comunRepositorio;
            _logger = logger;

        }


        #region publicos 
        public async Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, CancellationToken TokenCancelacion)
        {
            var stopwatch = Stopwatch.StartNew();           
          
            try
            { 
                   
               var ListaCredenciales = await _comunRepositorio.CredencialesAsync(TokenCancelacion).ConfigureAwait(false);

               if (ListaCredenciales == null) throw new CredencialesNoDisponibles();


                var separarMaletas = peticion.Localizador.Contains('.');

                CancelacionResponseDTO respuesta = null;

               
                    respuesta = peticion.Source switch
                    {
                        //"TK" => await _travelKitServicio.CancelacionAsync(peticion, ListaCredenciales, TokenCancelacion).ConfigureAwait(false),
                        "INT" => await _INeedToursServicio.CancelacionAsync(peticion, ListaCredenciales, TokenCancelacion).ConfigureAwait(false),
                        _ => null
                    };
                 
                if (respuesta == null)
                    return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Integracion no encontrada");




                stopwatch.Stop();
                _logger.FinAlternos(peticion.Localizador, peticion.Localizador, "Cancelacion", stopwatch.ElapsedMilliseconds);

                return new CancelacionResponseDTO(respuesta.Estado, respuesta.Source, respuesta.Localizador, respuesta.Mensaje);
                  
            } 
            finally
            {
                _semaphore.Release();
            }
             

        }

        #endregion publicos
      
    }
}
