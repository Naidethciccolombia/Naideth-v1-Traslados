using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rs;
using Naideth.Traslados.Dominio.Busquedas;
using Naideth.Traslados.Dominio.Estados;
using Naideth.Traslados.Dominio.Kernel.Exepciones;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Naideth.Traslados.Aplicacion.Componentes.INeedTours
{
    public sealed class INeedToursServicio : IINeedToursServicio
    {
        private readonly IINeedToursRepositorio _INeedToursRepositorio;
        private readonly IAppLoggerServicio _logger;
        private readonly MetodosComunes _comunes;
        private readonly string codigoIntegracion = "INT";
        private readonly ICacheRedisServicio _cacheServicio;
        public INeedToursServicio(IINeedToursRepositorio INeedToursRepositorio, ICacheRedisServicio cacheRedisServicio, IAppLoggerServicio logger, MetodosComunes comunes)
        {
            _INeedToursRepositorio = INeedToursRepositorio;
            _logger = logger;
            _cacheServicio = cacheRedisServicio;
            _comunes = comunes;
        }
        public async Task<BusquedaResponseDTO> BusquedaAsync(Busqueda peticion, List<IntegracionDTO> listaCredenciales, FiltroRequestDTO? filtros, CancellationToken tokenCancelacion)
        {
            var idBusqueda = peticion.IdBusqueda;
            var stopwatch = Stopwatch.StartNew();

            // ----------------- Buscar credencial -----------------
            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == codigoIntegracion);

            string evento = "Busqueda";

            if (credencial == null)
            {
                _logger.CredencialesNoEncontradas(peticion.Referencia, idBusqueda, evento, codigoIntegracion);
                return new BusquedaResponseDTO(Estado.ERROR, null);
            }
            // ---------------- CONSULTA AL REPOSITORIO (GDS) ----------------
            var swRepo = Stopwatch.StartNew();

            var resultado = await _INeedToursRepositorio.BusquedaAsync(peticion, credencial, filtros, tokenCancelacion).ConfigureAwait(false);
            swRepo.Stop();
            _logger.TiempoRepositorio(peticion.Referencia, idBusqueda, evento, codigoIntegracion, swRepo.Elapsed.TotalSeconds);
            if (resultado?.Estado == Estado.OK)
            {
                try
                {
                    var disponibilidades = await _comunes.FormateadoTariff(
                        resultado.Data.Disponibilidades,
                        peticion.Moneda,
                        peticion.MarkUps,
                        tokenCancelacion).ConfigureAwait(false);

                    resultado = new ResponseDTO(
                        resultado.Estado,
                        new DisponibilidadesDTO(
                            resultado.Data.Source,
                            resultado.Data.SourceName,
                            disponibilidades
                        ));
                }
                catch (ExepcionConvitiendoTrm)
                {
                    // Sin TRM no descartamos la disponibilidad: devolvemos los precios
                    // en la moneda del proveedor (EUR) sin convertir.
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, codigoIntegracion,
                        $"INeedTours: sin TRM para convertir EUR -> {peticion.Moneda}. Se devuelven precios en EUR.");
                }
                catch (Exception ex)
                {
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, codigoIntegracion, $"Error formateando tarifas INeedTours: {ex}");
                    return new BusquedaResponseDTO(Estado.ERROR, null);
                }
            }

            stopwatch.Stop();

            // FINAL OK  
            _logger.FinIntegracion(peticion.Referencia, idBusqueda, evento, codigoIntegracion, stopwatch.Elapsed.TotalSeconds);

            return new BusquedaResponseDTO(resultado.Estado, resultado.Data);
        }
        public async Task<PreReservaResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, CacheDTO cachedResult, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion)
        {
            var stopwatch = Stopwatch.StartNew();
            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == codigoIntegracion);

            if (credencial == null)
            {
                return new PreReservaResponseDTO(Estado.ERROR, null);
            }
            try
            {
                var disponibilidadSeleccionada = cachedResult.disponibilidades.FirstOrDefault(r => r.Id == peticion.IdTarifa.ToString());

                var swRepo = Stopwatch.StartNew();

                var resultado = await _INeedToursRepositorio.PreReservaAsync(peticion, busqueda, credencial, disponibilidadSeleccionada, tokenCancelacion).ConfigureAwait(false);
                var disponibilidades = await _comunes.FormateadoTariff(
                   resultado.Data.Disponibilidades,
                   peticion.Moneda,
                   busqueda.MarkUps,
                   tokenCancelacion).ConfigureAwait(false);

                var resultadOrdenado = new PreReservaResponseDTO(
                     resultado.Estado,
                     new DisponibilidadesDTO(
                         resultado.Data.Source,
                         resultado.Data.SourceName,
                         disponibilidades
                     ));

                swRepo.Stop();

                return resultadOrdenado;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
            }

            return new PreReservaResponseDTO(Estado.ERROR, null);
        }
        public async Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, DisponibilidadDTO cachedResult, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion)
        {
            var stopwatch = Stopwatch.StartNew();
            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == codigoIntegracion);

            if (credencial == null)
            {           
                return new ReservaResponseDTO(Estado.ERROR, null, null);
            }
            var swRepo = Stopwatch.StartNew();
            
            var resultado = await _INeedToursRepositorio.ReservaAsync(peticion, busqueda, credencial, cachedResult, tokenCancelacion).ConfigureAwait(false);
            if (resultado == null)
                return new ReservaResponseDTO(Estado.ERROR, null, null);                                                                                                 
            var disponibilidades = await _comunes.FormateadoTariffReserva(
                resultado,
                cachedResult.Precio.Total.Moneda,
                busqueda.MarkUps,
                tokenCancelacion).ConfigureAwait(false);

            swRepo.Stop();

            return disponibilidades;
        }
        public async Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion)
        {
            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == codigoIntegracion);

            if (credencial == null)
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Error Credenciales");

            var resultado = await _INeedToursRepositorio.CancelacionAsync(peticion, credencial, tokenCancelacion).ConfigureAwait(false);
            if (resultado == null)
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Error");

            return resultado;
        }
    }
}
