using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs;
using Naideth.Traslados.Aplicacion.Componentes.Civitatis.Interfaces;
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.Componentes.Civitatis
{
    public sealed class CivitatisServicio : ICivitatisServicio
    {
        private readonly ICivitatisRepositorio _civitatisRepositorio;
        private readonly IAppLoggerServicio _logger;
        private readonly MetodosComunes _comunes;
        private const string CodigoIntegracion = "CIV";

        public CivitatisServicio(ICivitatisRepositorio civitatisRepositorio, IAppLoggerServicio logger, MetodosComunes comunes)
        {
            _civitatisRepositorio = civitatisRepositorio;
            _logger = logger;
            _comunes = comunes;
        }

        public async Task<BusquedaResponseDTO> BusquedaAsync(Busqueda peticion, List<IntegracionDTO> listaCredenciales, FiltroRequestDTO? filtros, CancellationToken tokenCancelacion)
        {
            var idBusqueda = peticion.IdBusqueda;
            var stopwatch = Stopwatch.StartNew();

            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == CodigoIntegracion);

            const string evento = "Busqueda";

            if (credencial == null)
            {
                _logger.CredencialesNoEncontradas(peticion.Referencia, idBusqueda, evento, CodigoIntegracion);
                return new BusquedaResponseDTO(Estado.ERROR, null);
            }

            var swRepo = Stopwatch.StartNew();

            var resultado = await _civitatisRepositorio.BusquedaAsync(peticion, credencial, filtros, tokenCancelacion).ConfigureAwait(false);
            swRepo.Stop();
            _logger.TiempoRepositorio(peticion.Referencia, idBusqueda, evento, CodigoIntegracion, swRepo.Elapsed.TotalSeconds);

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
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, CodigoIntegracion,
                        $"Civitatis: sin TRM para convertir {peticion.Moneda}. Se devuelven los precios en la moneda del proveedor.");
                }
                catch (Exception ex)
                {
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, CodigoIntegracion, $"Error formateando tarifas Civitatis: {ex}");
                    return new BusquedaResponseDTO(Estado.ERROR, null);
                }
            }

            stopwatch.Stop();

            _logger.FinIntegracion(peticion.Referencia, idBusqueda, evento, CodigoIntegracion, stopwatch.Elapsed.TotalSeconds);

            return new BusquedaResponseDTO(resultado.Estado, resultado.Data);
        }

        public async Task<PreReservaResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, CacheDTO cachedResult, bool noFormateo, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion)
        {
            const string evento = "PreReserva";
            var idBusqueda = peticion.IdBusqueda;
            var referencia = string.IsNullOrWhiteSpace(busqueda?.Referencia) ? idBusqueda.ToString() : busqueda.Referencia;
            var stopwatch = Stopwatch.StartNew();

            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == CodigoIntegracion);

            if (credencial == null)
            {
                _logger.CredencialesNoEncontradas(referencia, idBusqueda, evento, CodigoIntegracion);
                return new PreReservaResponseDTO(Estado.ERROR, null);
            }

            try
            {
                var disponibilidadSeleccionada = cachedResult?.disponibilidades?.FirstOrDefault(r => r.Id == peticion.IdTarifa.ToString());
                var quoteId = disponibilidadSeleccionada?.Tarifas?.FirstOrDefault()?.Id;

                if (string.IsNullOrWhiteSpace(quoteId))
                {
                    _logger.ErrorLog(referencia, idBusqueda, evento, CodigoIntegracion, $"Civitatis: no se encontro la disponibilidad {peticion.IdTarifa} en la busqueda cacheada.");
                    return new PreReservaResponseDTO(Estado.ERROR, null);
                }

                var swRepo = Stopwatch.StartNew();

                var resultado = await _civitatisRepositorio.PreReservaAsync(peticion, busqueda, credencial, quoteId, tokenCancelacion).ConfigureAwait(false);
                swRepo.Stop();
                _logger.TiempoRepositorio(referencia, idBusqueda, evento, CodigoIntegracion, swRepo.Elapsed.TotalSeconds);

                if (resultado?.Estado != Estado.OK || resultado.Data?.Disponibilidades == null)
                {
                    _logger.ErrorLog(referencia, idBusqueda, evento, CodigoIntegracion, "Civitatis: no fue posible obtener la pre-reserva desde la cotizacion cacheada.");
                    return new PreReservaResponseDTO(Estado.ERROR, null);
                }

                var disponibilidades = await _comunes.FormateadoTariff(
                    resultado.Data.Disponibilidades,
                    peticion.Moneda,
                    busqueda.MarkUps,
                    noFormateo,
                    tokenCancelacion).ConfigureAwait(false);

                stopwatch.Stop();
                _logger.FinIntegracion(referencia, idBusqueda, evento, CodigoIntegracion, stopwatch.Elapsed.TotalSeconds);

                return new PreReservaResponseDTO(
                    resultado.Estado,
                    new DisponibilidadesDTO(
                        resultado.Data.Source,
                        resultado.Data.SourceName,
                        disponibilidades));
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(referencia, idBusqueda, evento, CodigoIntegracion, $"Civitatis: error en pre-reserva: {ex.Message}");
            }

            return new PreReservaResponseDTO(Estado.ERROR, null);
        }

        public async Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, DisponibilidadDTO cachedResult, bool noFormateo, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion)
        {
            const string evento = "Reserva";
            var idPreReserva = peticion.IdPreReserva;
            var referencia = string.IsNullOrWhiteSpace(busqueda?.Referencia) ? idPreReserva.ToString() : busqueda.Referencia;
            var stopwatch = Stopwatch.StartNew();

            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == CodigoIntegracion);

            if (credencial == null)
            {
                _logger.CredencialesNoEncontradas(referencia, idPreReserva, evento, CodigoIntegracion);
                return new ReservaResponseDTO(Estado.ERROR, null, null);
            }

            ReservaResponseDTO? resultado = null;

            try
            {
                var swRepo = Stopwatch.StartNew();

                resultado = await _civitatisRepositorio.ReservaAsync(peticion, busqueda, credencial, cachedResult, tokenCancelacion).ConfigureAwait(false);
                swRepo.Stop();
                _logger.TiempoRepositorio(referencia, idPreReserva, evento, CodigoIntegracion, swRepo.Elapsed.TotalSeconds);

                if (resultado == null || resultado.Estado != Estado.OK)
                {
                    _logger.ErrorLog(referencia, idPreReserva, evento, CodigoIntegracion, "Civitatis: no fue posible confirmar la reserva.");
                    return new ReservaResponseDTO(Estado.ERROR, null, null);
                }

                var disponibilidades = await _comunes.FormateadoTariffReserva(
                    resultado,
                    cachedResult.Precio.Total.Moneda,
                    busqueda.MarkUps,
                    noFormateo,
                    tokenCancelacion).ConfigureAwait(false);

                stopwatch.Stop();
                _logger.FinIntegracion(referencia, idPreReserva, evento, CodigoIntegracion, stopwatch.Elapsed.TotalSeconds);

                return disponibilidades;
            }
            catch (ExepcionConvitiendoTrm)
            {
                _logger.ErrorLog(referencia, idPreReserva, evento, CodigoIntegracion,
                    $"Civitatis: sin TRM para convertir {cachedResult?.Precio?.Total?.Moneda}. Se devuelve el precio en la moneda del proveedor.");
                return resultado ?? new ReservaResponseDTO(Estado.ERROR, null, null);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(referencia, idPreReserva, evento, CodigoIntegracion, $"Civitatis: error en reserva: {ex.Message}");
            }

            return new ReservaResponseDTO(Estado.ERROR, null, null);
        }

        public async Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion)
        {
            const string evento = "Cancelacion";
            var stopwatch = Stopwatch.StartNew();

            var credencial = listaCredenciales?
                .FirstOrDefault(cr => cr.CodigoIntegrador == CodigoIntegracion);

            if (credencial == null)
            {
                _logger.CredencialesNoEncontradas(peticion.Localizador, Guid.Empty, evento, CodigoIntegracion);
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Error Credenciales");
            }

            try
            {
                var swRepo = Stopwatch.StartNew();

                var resultado = await _civitatisRepositorio.CancelacionAsync(peticion, credencial, tokenCancelacion).ConfigureAwait(false);
                swRepo.Stop();
                _logger.TiempoRepositorio(peticion.Localizador, Guid.Empty, evento, CodigoIntegracion, swRepo.Elapsed.TotalSeconds);

                if (resultado == null)
                    return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Error");

                stopwatch.Stop();
                _logger.FinIntegracion(peticion.Localizador, Guid.Empty, evento, CodigoIntegracion, stopwatch.Elapsed.TotalSeconds);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, evento, CodigoIntegracion, $"Civitatis: error en cancelacion: {ex.Message}");
            }

            return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Error");
        }
    }
}
