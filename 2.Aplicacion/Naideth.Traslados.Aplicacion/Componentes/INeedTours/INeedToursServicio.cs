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
                        filtros?.noFormateo ?? false,
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
        public async Task<PreReservaResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, CacheDTO cachedResult, bool noFormateo, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion)
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

                if (disponibilidadSeleccionada == null)
                {
                    _logger.ErrorLog(peticion.IdBusqueda.ToString(), peticion.IdBusqueda, "PreReserva", codigoIntegracion, $"INeedTours: no se encontro la disponibilidad {peticion.IdTarifa} en el cache de la busqueda.");
                    return new PreReservaResponseDTO(Estado.ERROR, null);
                }

                var swRepo = Stopwatch.StartNew();

                var resultado = await _INeedToursRepositorio.PreReservaAsync(peticion, busqueda, credencial, disponibilidadSeleccionada, tokenCancelacion).ConfigureAwait(false);

                if (resultado?.Estado != Estado.OK || resultado.Data?.Disponibilidades == null)
                {
                    _logger.ErrorLog(peticion.IdBusqueda.ToString(), peticion.IdBusqueda, "PreReserva", codigoIntegracion, $"INeedTours: la pre-reserva del proveedor no devolvio disponibilidad (Estado {resultado?.Estado}). Revise el log del repositorio.");
                    return new PreReservaResponseDTO(Estado.ERROR, null);
                }

                var disponibilidades = await ObtenerDisponibilidadesFormateadas(
                    resultado.Data.Disponibilidades,
                    peticion.Moneda,
                    busqueda.MarkUps,
                    noFormateo,
                    peticion.IdBusqueda,
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
                _logger.ErrorLog(peticion.IdBusqueda.ToString(), peticion.IdBusqueda, "PreReserva", codigoIntegracion, $"Error en pre-reserva INeedTours: {ex}");
            }

            return new PreReservaResponseDTO(Estado.ERROR, null);
        }

        private async Task<List<DisponibilidadDTO>> ObtenerDisponibilidadesFormateadas(
            List<DisponibilidadDTO> disponibilidades,
            string monedaDestino,
            decimal markUps,
            bool noFormateo,
            Guid idBusqueda,
            CancellationToken tokenCancelacion)
        {
            try
            {
                return await _comunes.FormateadoTariff(
                    disponibilidades,
                    monedaDestino,
                    markUps,
                    noFormateo,
                    tokenCancelacion).ConfigureAwait(false);
            }
            catch (ExepcionConvitiendoTrm)
            {
                // Sin TRM no descartamos la pre-reserva: devolvemos los precios
                // en la moneda del proveedor sin convertir.
                _logger.ErrorLog(idBusqueda.ToString(), idBusqueda, "PreReserva", codigoIntegracion,
                    $"INeedTours: sin TRM para convertir a {monedaDestino}. Se devuelve la pre-reserva en la moneda del proveedor.");
                return disponibilidades;
            }
        }
        public async Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, DisponibilidadDTO cachedResult, bool noFormateo, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion)
        {
            const string evento = "Reserva";
            var idPreReserva = peticion.IdPreReserva;
            var referencia = string.IsNullOrWhiteSpace(busqueda?.Referencia) ? idPreReserva.ToString() : busqueda.Referencia;
            var stopwatch = Stopwatch.StartNew();

            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == codigoIntegracion);

            if (credencial == null)
            {
                _logger.CredencialesNoEncontradas(referencia, idPreReserva, evento, codigoIntegracion);
                return new ReservaResponseDTO(Estado.ERROR, null, null);
            }

            if (cachedResult == null)
            {
                _logger.ErrorLog(referencia, idPreReserva, evento, codigoIntegracion, "INeedTours: no hay pre-reserva cacheada para confirmar (cache miss).");
                return new ReservaResponseDTO(Estado.ERROR, null, null);
            }

            ReservaResponseDTO? resultado = null;

            try
            {
                var swRepo = Stopwatch.StartNew();

                resultado = await _INeedToursRepositorio.ReservaAsync(peticion, busqueda, credencial, cachedResult, tokenCancelacion).ConfigureAwait(false);
                swRepo.Stop();
                _logger.TiempoRepositorio(referencia, idPreReserva, evento, codigoIntegracion, swRepo.Elapsed.TotalSeconds);

                if (resultado == null || resultado.Estado != Estado.OK)
                {
                    _logger.ErrorLog(referencia, idPreReserva, evento, codigoIntegracion, "INeedTours: no fue posible confirmar la reserva.");
                    return new ReservaResponseDTO(Estado.ERROR, null, null);
                }

                if (resultado.Precio == null)
                {
                    _logger.ErrorLog(referencia, idPreReserva, evento, codigoIntegracion, "INeedTours: la reserva se confirmo pero no se pudo extraer el precio del COMMIT.");
                    return new ReservaResponseDTO(Estado.ERROR, null, null);
                }

                var disponibilidades = await _comunes.FormateadoTariffReserva(
                    resultado,
                    cachedResult.Precio.Total.Moneda,
                    busqueda.MarkUps,
                    noFormateo,
                    tokenCancelacion).ConfigureAwait(false);

                stopwatch.Stop();
                _logger.FinIntegracion(referencia, idPreReserva, evento, codigoIntegracion, stopwatch.Elapsed.TotalSeconds);

                return disponibilidades;
            }
            catch (ExepcionConvitiendoTrm)
            {
                _logger.ErrorLog(referencia, idPreReserva, evento, codigoIntegracion,
                    $"INeedTours: sin TRM para convertir a {cachedResult?.Precio?.Total?.Moneda}. Se devuelve la reserva en la moneda del proveedor.");
                return resultado ?? new ReservaResponseDTO(Estado.ERROR, null, null);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(referencia, idPreReserva, evento, codigoIntegracion, $"Error en reserva INeedTours: {ex.Message}");
            }

            return new ReservaResponseDTO(Estado.ERROR, null, null);
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
