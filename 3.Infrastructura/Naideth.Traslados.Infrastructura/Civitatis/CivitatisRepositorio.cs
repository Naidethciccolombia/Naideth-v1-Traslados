using Microsoft.EntityFrameworkCore;
using Naideth.Central.Dominio.Hoteles;
using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs;
using Naideth.Traslados.Aplicacion.Componentes.Civitatis.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.ConexionesExternas.Interfaces;
using Naideth.Traslados.Dominio.Busquedas;
using Naideth.Traslados.Dominio.Estados;
using Naideth.Traslados.Dominio.Trayectos;
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces;
using Naideth.Traslados.Infrastructura.Civitatis.Models.Rq;
using Naideth.Traslados.Infrastructura.Civitatis.Models.Rs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Naideth.Traslados.Infrastructura.Civitatis
{
    internal class CivitatisRepositorio : ICivitatisRepositorio
    {
        private readonly IApplicationDbContext _context;
        private readonly IConexionesExternasRepositorio _conexionesExternasRepositorio;
        private readonly IAppLoggerServicio _logger;
        private readonly ICacheRedisServicio _cacheServicio;

        private const string TipoUbicacionIATA = "IATA";
        private const string TipoUbicacionCoordenadas = "COORDINATES";
        private const string TipoServicioIda = "ONE_WAY";
        private const string TipoServicioIdaVuelta = "ROUND_TRIP";
        private const string CategoriaTraslado = "Traslado";
        private const string TipoAdulto = "ADT";
        private const string RutaCotizar = "/transfers/quotes";
        private const string RutaReservar = "/transfers/bookings";
        private const string RutaConfirmarCarrito = "/cart/{0}/confirm";
        private const string RutaCancelarItems = "/bookings/{0}/items";
        private const string RutaDetallesReserva = "/bookings/transfers/{0}";
        private const string SufijoVersionV3 = "/v3";
        private const string SufijoVersionV2 = "/v2";
        private const int IntentosConfirmacionCarrito = 3;
        private const int EsperaReintentoConfirmacionSegundos = 2;
        private const string IdiomaPorDefecto = "ES";
        private const string PrefijoCacheCotizacion = "TRASLADO:CIV:Cotizacion:";
        private const string PrefijoCacheReserva = "TRASLADO:CIV:Reserva:";
        private const int TimeOutSegundos = 60;
        private const int LongitudMaximaComentarios = 500;

        private static readonly TimeSpan TtlCachePorDefecto = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan TtlCacheReserva = TimeSpan.FromDays(90);

        private static readonly JsonSerializerOptions OpcionesJson = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly JsonSerializerOptions OpcionesJsonReserva = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public CivitatisRepositorio(IApplicationDbContext context, IConexionesExternasRepositorio conexionesExternasRepositorio, IAppLoggerServicio logger, ICacheRedisServicio cacheServicio)
        {
            _context = context;
            _conexionesExternasRepositorio = conexionesExternasRepositorio;
            _logger = logger;
            _cacheServicio = cacheServicio;
        }

        public async Task<ResponseDTO> BusquedaAsync(Busqueda peticion, IntegracionDTO credencial, FiltroRequestDTO? filtros, CancellationToken TokenCancelacion)
        {
            const string evento = "Busqueda";
            var idBusqueda = peticion.IdBusqueda;

            if (credencial == null)
                return new ResponseDTO(Estado.ERROR, null);

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                if (peticion.Trayectos == null || peticion.Trayectos.Count == 0)
                {
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, credencial.codigo, "Civitatis requiere al menos un trayecto para la busqueda.");
                    return new ResponseDTO(Estado.ERROR, null);
                }

                var rq = await ObjetoPeticionBusquedaAsync(peticion, credencial, TokenCancelacion).ConfigureAwait(false);
                if (rq == null)
                    return new ResponseDTO(Estado.ERROR, null);

                var dataJson = JsonSerializer.Serialize(rq, OpcionesJson);
                var url = $"{credencial.EndPoint.TrimEnd('/')}{RutaCotizar}?currency={peticion.Moneda}&language={ObtenerIdioma(credencial)}";

                var headers = new Dictionary<string, string>
                {
                    { "Authorization", $"bearer {ObtenerToken(credencial)}" },
                    { "Accept", "application/json" }
                };

                _logger.FinPeticionGds(peticion.Referencia, idBusqueda, evento, credencial.codigo, 0, $"Inicio Peticion Civitatis COTIZAR = {dataJson}");

                using var ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(TokenCancelacion);
                ctsTimeout.CancelAfter(TimeSpan.FromSeconds(TimeOutSegundos));

                var respuesta = await _conexionesExternasRepositorio.PeticionExternaTravelKitAsync(
                    idBusqueda, peticion.Referencia, credencial.codigo,
                    new StringContent(dataJson, Encoding.UTF8, "application/json"),
                    headers, new List<string>(), url, "POST",
                    ctsTimeout.Token, TimeOutSegundos, false).ConfigureAwait(false);

                if (respuesta == null || respuesta.Status == false || respuesta.Ex != null)
                {
                    var error = respuesta?.Ex?.Message ?? "Sin respuesta del proveedor";
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, credencial.codigo, error);
                    return new ResponseDTO(Estado.ERROR, null);
                }

                var disponibilidades = await ProcesarRespuestaBusquedaAsync(respuesta.Data?.ToString(), peticion, credencial, TokenCancelacion).ConfigureAwait(false);
                if (disponibilidades == null)
                {
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, credencial.codigo, "No fue posible interpretar la respuesta de Civitatis.");
                    return new ResponseDTO(Estado.ERROR, null);
                }

                stopwatchTotal.Stop();
                _logger.Respuesta(peticion.Referencia, idBusqueda, evento, stopwatchTotal.Elapsed.TotalSeconds, disponibilidades.Count);

                return new ResponseDTO(Estado.OK, new DisponibilidadesDTO(credencial.CodigoIntegrador, credencial.nombre, disponibilidades));
            }
            catch (OperationCanceledException)
            {
                return new ResponseDTO(Estado.ERROR, null);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, credencial.codigo, ex.Message);
                return new ResponseDTO(Estado.ERROR, null);
            }
        }

        public async Task<ResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, string quoteId, CancellationToken TokenCancelacion)
        {
            const string evento = "PreReserva";
            var idBusqueda = peticion.IdBusqueda;
            var referencia = string.IsNullOrWhiteSpace(busqueda?.Referencia) ? idBusqueda.ToString() : busqueda.Referencia;

            if (credencial == null || string.IsNullOrWhiteSpace(quoteId))
                return new ResponseDTO(Estado.ERROR, null);

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                var jsonCotizacion = await _cacheServicio.GetAsync<string>($"{PrefijoCacheCotizacion}{quoteId}", TokenCancelacion).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(jsonCotizacion))
                {
                    _logger.ErrorLog(referencia, idBusqueda, evento, credencial.codigo, $"Civitatis: no se encontro la cotizacion {quoteId} en cache (pudo expirar). Se debe volver a cotizar.");
                    return new ResponseDTO(Estado.ERROR, null);
                }

                CotizacionRs? cotizacion;
                try
                {
                    cotizacion = JsonSerializer.Deserialize<CotizacionRs>(jsonCotizacion, OpcionesJson);
                }
                catch (JsonException)
                {
                    cotizacion = null;
                }

                if (cotizacion == null)
                {
                    _logger.ErrorLog(referencia, idBusqueda, evento, credencial.codigo, "Civitatis: la cotizacion en cache no pudo interpretarse.");
                    return new ResponseDTO(Estado.ERROR, null);
                }

                var disponibilidad = MapearCotizacion(cotizacion, busqueda, credencial);
                if (disponibilidad == null)
                {
                    _logger.ErrorLog(referencia, idBusqueda, evento, credencial.codigo, "Civitatis: no fue posible reconstruir la disponibilidad desde la cotizacion.");
                    return new ResponseDTO(Estado.ERROR, null);
                }

                stopwatchTotal.Stop();
                _logger.Respuesta(referencia, idBusqueda, evento, stopwatchTotal.Elapsed.TotalSeconds, 1);

                return new ResponseDTO(Estado.OK, new DisponibilidadesDTO(credencial.CodigoIntegrador, credencial.nombre, new List<DisponibilidadDTO> { disponibilidad }));
            }
            catch (OperationCanceledException)
            {
                return new ResponseDTO(Estado.ERROR, null);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(referencia, idBusqueda, evento, credencial.codigo, ex.Message);
                return new ResponseDTO(Estado.ERROR, null);
            }
        }

        private async Task<CotizarRq?> ObjetoPeticionBusquedaAsync(Busqueda peticion, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            var trayectosOrdenados = peticion.Trayectos.OrderBy(t => t.Numero).ToList();
            var trayectoIda = trayectosOrdenados.First();
            var trayectoVuelta = trayectosOrdenados.Last();
            var esIdaYVuelta = peticion.AgregarRegreso || trayectosOrdenados.Count > 1;

            var origen = await ObtenerUbicacionAsync(trayectoIda.Origen, peticion, credencial, TokenCancelacion).ConfigureAwait(false);
            var destino = await ObtenerUbicacionAsync(trayectoIda.Destino, peticion, credencial, TokenCancelacion).ConfigureAwait(false);

            if (origen == null || destino == null)
                return null;

            return new CotizarRq
            {
                Type = esIdaYVuelta ? TipoServicioIdaVuelta : TipoServicioIda,
                From = origen,
                To = destino,
                OutboundAt = trayectoIda.FechaHora.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                InboundAt = esIdaYVuelta ? trayectoVuelta.FechaHora.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture) : null,
                Passengers = peticion.Pasajeros.Sum(p => p.Cantidad)
            };
        }

        private async Task<UbicacionCotizarRq?> ObtenerUbicacionAsync(Ubicacion ubicacion, Busqueda peticion, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            if (ubicacion.EsIATA)
                return new UbicacionCotizarRq { Type = TipoUbicacionIATA, Value = ubicacion.Codigo };

            if (!Guid.TryParse(ubicacion.Codigo, out var idHotel))
                return null;

            var hotel = await _context.Hoteles.AsNoTracking()
                .FirstOrDefaultAsync(h => h.IdHotel == idHotel, TokenCancelacion).ConfigureAwait(false);

            if (hotel == null)
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "Busqueda", credencial.codigo, $"Civitatis: no se encontro el hotel {ubicacion.Codigo}.");
                return null;
            }

            if (!ParsearCoordenadas(hotel.Coordenadas, out var latitud, out var longitud))
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "Busqueda", credencial.codigo, $"Civitatis: el hotel {hotel.Nombre} no tiene coordenadas validas.");
                return null;
            }

            return new UbicacionCotizarRq
            {
                Type = TipoUbicacionCoordenadas,
                Latitude = latitud,
                Longitude = longitud,
                AddressText = string.IsNullOrWhiteSpace(hotel.Nombre) ? hotel.Direccion : hotel.Nombre
            };
        }

        private static bool ParsearCoordenadas(string coordenadas, out double latitud, out double longitud)
        {
            latitud = 0;
            longitud = 0;

            if (string.IsNullOrWhiteSpace(coordenadas))
                return false;

            string? latitudCruda = null;
            string? longitudCruda = null;

            foreach (var token in coordenadas.Split(','))
            {
                var limpio = token.Trim();

                if (string.IsNullOrWhiteSpace(limpio))
                    continue;

                // Formato con prefijos: "lat:XX.XXX, lon:YY.YYY"
                if (limpio.StartsWith("lat:", StringComparison.OrdinalIgnoreCase))
                    latitudCruda = limpio[4..].Trim();
                else if (limpio.StartsWith("lon:", StringComparison.OrdinalIgnoreCase))
                    longitudCruda = limpio[4..].Trim();
                // Formato simple: "latitud, longitud" (primer valor = lat, segundo = lon)
                else if (latitudCruda == null)
                    latitudCruda = limpio;
                else if (longitudCruda == null)
                    longitudCruda = limpio;
            }

            return double.TryParse(latitudCruda, NumberStyles.Float, CultureInfo.InvariantCulture, out latitud)
                && double.TryParse(longitudCruda, NumberStyles.Float, CultureInfo.InvariantCulture, out longitud);
        }

        private static string ObtenerIdioma(IntegracionDTO credencial)
        {
            return credencial.credencial.FirstOrDefault(c => c.nombre.Equals("Idioma", StringComparison.CurrentCultureIgnoreCase))?.valor ?? IdiomaPorDefecto;
        }

        private static string ObtenerToken(IntegracionDTO credencial)
        {
            var token = credencial.credencial.FirstOrDefault(c =>
                c.nombre.Equals("Token", StringComparison.CurrentCultureIgnoreCase) ||
                c.nombre.Equals("Jwt", StringComparison.CurrentCultureIgnoreCase) ||
                c.nombre.Equals("Authorization", StringComparison.CurrentCultureIgnoreCase))?.valor;

            if (!string.IsNullOrWhiteSpace(token))
                return token;

            return string.IsNullOrWhiteSpace(credencial.Clave) ? string.Empty : credencial.Clave;
        }

        private async Task<List<DisponibilidadDTO>?> ProcesarRespuestaBusquedaAsync(string? json, Busqueda peticion, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            CotizarRs? respuesta;
            try
            {
                respuesta = JsonSerializer.Deserialize<CotizarRs>(json, OpcionesJson);
            }
            catch (JsonException)
            {
                return null;
            }

            if (respuesta == null)
                return null;

            if (!string.IsNullOrWhiteSpace(respuesta.Error) || !string.IsNullOrWhiteSpace(respuesta.Message))
                return null;

            var lista = new List<DisponibilidadDTO>();

            foreach (var cotizacion in respuesta.Quotes ?? new List<CotizacionRs>())
            {
                var disponibilidad = MapearCotizacion(cotizacion, peticion, credencial);
                if (disponibilidad != null)
                    lista.Add(disponibilidad);
            }

            await CachearCotizacionesAsync(json, respuesta.Quotes, peticion, credencial, TokenCancelacion).ConfigureAwait(false);

            return lista;
        }

        private async Task CachearCotizacionesAsync(string json, List<CotizacionRs>? cotizaciones, Busqueda peticion, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            if (cotizaciones == null || cotizaciones.Count == 0)
                return;

            JsonDocument? documento;
            try
            {
                documento = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                return;
            }

            using (documento)
            {
                if (!documento.RootElement.TryGetProperty("quotes", out var quotesElement) || quotesElement.ValueKind != JsonValueKind.Array)
                    return;

                var quotesCrudos = quotesElement.EnumerateArray().ToList();
                for (var i = 0; i < cotizaciones.Count && i < quotesCrudos.Count; i++)
                {
                    await CachearCotizacionAsync(quotesCrudos[i].GetRawText(), cotizaciones[i], peticion, credencial, TokenCancelacion).ConfigureAwait(false);
                }
            }
        }

        private async Task CachearCotizacionAsync(string jsonCotizacion, CotizacionRs cotizacion, Busqueda peticion, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            if (string.IsNullOrWhiteSpace(cotizacion.Id))
                return;

            var ttl = CalcularTtlCotizacion(cotizacion.ExpiresAt);
            if (ttl <= TimeSpan.Zero)
                return;

            try
            {
                await _cacheServicio.SetAsync($"{PrefijoCacheCotizacion}{cotizacion.Id}", jsonCotizacion, ttl, TokenCancelacion).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "Busqueda", credencial.codigo, $"Civitatis: no fue posible cachear la cotizacion {cotizacion.Id}: {ex.Message}");
            }
        }

        private static TimeSpan CalcularTtlCotizacion(string? expiresAt)
        {
            if (string.IsNullOrWhiteSpace(expiresAt))
                return TtlCachePorDefecto;

            if (!DateTimeOffset.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var expira))
                return TtlCachePorDefecto;

            var ttl = expira - DateTimeOffset.UtcNow;
            return ttl > TimeSpan.Zero ? ttl : TimeSpan.Zero;
        }

        private DisponibilidadDTO? MapearCotizacion(CotizacionRs cotizacion, Busqueda peticion, IntegracionDTO credencial)
        {
            var segmentos = cotizacion.Segments ?? new List<TramoCotizacionRs>();
            var segmento = segmentos.FirstOrDefault();
            if (segmento == null)
                return null;

            var trayectosOrdenados = peticion.Trayectos.OrderBy(t => t.Numero).ToList();
            var trayectoIda = trayectosOrdenados.First();
            var trayectoVuelta = trayectosOrdenados.Last();

            var importe = (cotizacion.Price?.Total?.Amount ?? 0) / 100m;
            var moneda = cotizacion.Price?.Total?.Currency ?? peticion.Moneda;
            var pasajeros = cotizacion.Passengers ?? peticion.Pasajeros.Sum(p => p.Cantidad);

            var horaInicio = FormatearHora(segmento.PickupAt, trayectoIda.ObtenerHora());
            var horaSalida = FormatearHora(segmentos.Last().PickupAt, trayectoVuelta.ObtenerHora());

            var imagen = segmento.Vehicle?.Images?.FirstOrDefault() ?? string.Empty;
            var nombreConcepto = NombreConcepto(segmento);
            var beneficios = ExtraerBeneficios(segmento);
            var idCotizacion = cotizacion.Id ?? Guid.NewGuid().ToString();
            var total = new TotalDTO(moneda, importe, 0m, importe);

            var tarifa = new TarifaDTO
            {
                Id = idCotizacion,
                IdPreReserva = Guid.NewGuid().ToString(),
                Category = segmento.Service?.Category ?? string.Empty,
                Nombre = nombreConcepto,
                Beneficios = beneficios,
                RangoEdad = string.Empty,
                CantidadAdultos = pasajeros,
                CantidadNinos = 0,
                CantidadInfantes = 0,
                DatosTarifa = new DatosTarifaDTO
                {
                    TipoVehiculo = segmento.Vehicle?.Category,
                    ModeloVehiculo = segmento.Vehicle?.Models?.FirstOrDefault(),
                    ImagenVehiculo = imagen,
                    CantidadPasajeros = segmento.Vehicle?.PaxCapacity,
                    MaletasMaximas = segmento.Vehicle?.BaggageCapacity?.LargeBags,
                    EsperaMaxima = segmento.WaitSurcharge?.Minutes?.ToString(CultureInfo.InvariantCulture),
                    HoraInicio = horaInicio,
                    PoliticasCancelacion = ExtraerPoliticasCancelacion(segmento),
                    Extras = ExtraerExtras(segmento, moneda)
                }
            };

            var auxiliares = new List<TpaDTO>
            {
                new TpaDTO("MonedaOrigen", moneda),
                new TpaDTO("ExpiraCotizacion", cotizacion.ExpiresAt ?? string.Empty)
            };

            var requisitos = segmentos.SelectMany(s => s.BookingRequirements ?? new List<string>()).Distinct().ToList();
            if (requisitos.Count > 0)
                auxiliares.Add(new TpaDTO("BookingRequirements", string.Join("|", requisitos)));

            return new DisponibilidadDTO
            {
                Source = credencial.CodigoIntegrador,
                SourceName = credencial.nombre,
                Id = Guid.NewGuid().ToString(),
                Auxiliares = auxiliares,
                Plan = nombreConcepto,
                Duracion = string.Empty,
                Tipo = CategoriaTraslado,
                FechaInicio = peticion.Inicio,
                FechaSalida = peticion.Fin,
                HoraInicio = horaInicio,
                HoraSalida = horaSalida,
                Imagen = imagen,
                Precio = new PrecioDTO(total, new List<PTCTotalDTO> { new PTCTotalDTO(TipoAdulto, pasajeros, total) }),
                Tarifas = new List<TarifaDTO> { tarifa }
            };
        }

        private static string FormatearHora(string? valor, TimeSpan respaldo)
        {
            if (!string.IsNullOrWhiteSpace(valor) && DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
                return hora.ToString("HH:mm", CultureInfo.InvariantCulture);

            return respaldo.ToString(@"hh\:mm");
        }

        private static string NombreConcepto(TramoCotizacionRs segmento)
        {
            if (!string.IsNullOrWhiteSpace(segmento.Label))
                return segmento.Label;

            var tipo = segmento.Service?.Type;
            var categoria = segmento.Vehicle?.Category ?? segmento.Service?.Category;

            return string.Join(" ", new[] { tipo, categoria }.Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        private static List<BeneficiosDTO> ExtraerBeneficios(TramoCotizacionRs segmento)
        {
            var beneficios = new List<BeneficiosDTO>();

            if (segmento.Service?.MeetAndGreet == true)
                beneficios.Add(new BeneficiosDTO("Meet and Greet", "El conductor espera en el punto de recogida", "servicio"));

            if (segmento.Vehicle?.IsAdapted == true)
                beneficios.Add(new BeneficiosDTO("Adaptado PMR", "Vehiculo adaptado para movilidad reducida", "vehiculo"));

            if (segmento.CancellationPolicy?.FreeCancellationHours.HasValue == true)
                beneficios.Add(new BeneficiosDTO("Cancelacion gratuita", $"Cancelacion sin costo hasta {segmento.CancellationPolicy.FreeCancellationHours} horas antes", "politica"));

            if (segmento.WaitSurcharge != null)
                beneficios.Add(new BeneficiosDTO("Recargo por espera", $"Espera incluida de {segmento.WaitSurcharge.Minutes ?? 0} minutos", "espera"));

            return beneficios;
        }

        private static List<DatosExtraDTO>? ExtraerExtras(TramoCotizacionRs segmento, string monedaCotizacion)
        {
            var addons = segmento.AvailableAddons;
            if (addons == null || addons.Count == 0)
                return null;

            var extras = new List<DatosExtraDTO>();

            foreach (var addon in addons)
            {
                var importe = addon.Price?.Amount.HasValue == true
                    ? addon.Price.Amount.Value / 100m
                    : (decimal?)null;

                string? especificaciones = null;
                if (addon.Specs.HasValue)
                {
                    var specs = addon.Specs.Value;
                    if (specs.ValueKind != JsonValueKind.Undefined && specs.ValueKind != JsonValueKind.Null)
                    {
                        var texto = specs.GetRawText();
                        if (!string.Equals(texto, "{}", StringComparison.Ordinal))
                            especificaciones = texto;
                    }
                }

                extras.Add(new DatosExtraDTO
                {
                    Id = addon.Id ?? string.Empty,
                    Descripcion = addon.Description ?? string.Empty,
                    Importe = importe,
                    Moneda = addon.Price?.Currency ?? monedaCotizacion,
                    MaximoPermitido = addon.MaxAllowed,
                    Obligatorio = addon.IsMandatory == true,
                    Garantizado = addon.IsGuaranteed == true,
                    Especificaciones = especificaciones
                });
            }

            return extras;
        }

        private static List<PoliticaCancelacionDTO>? ExtraerPoliticasCancelacion(TramoCotizacionRs segmento)
        {
            var horas = segmento.CancellationPolicy?.FreeCancellationHours;
            if (!horas.HasValue)
                return null;

            return new List<PoliticaCancelacionDTO>
            {
                new PoliticaCancelacionDTO(null, null, null, null, null, null, $"Cancelacion gratuita hasta {horas} horas antes", true)
            };
        }

        public async Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, DisponibilidadDTO disponibilidadSeleccionada, CancellationToken TokenCancelacion)
        {
            const string evento = "Reserva";
            var idPreReserva = peticion.IdPreReserva;

            if (credencial == null)
                return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);

            var quoteId = disponibilidadSeleccionada?.Tarifas?.FirstOrDefault()?.Id;
            if (string.IsNullOrWhiteSpace(quoteId))
            {
                _logger.ErrorLog(busqueda.Referencia, idPreReserva, evento, credencial.codigo, "Civitatis requiere una tarifa seleccionada (quoteId) para la reserva.");
                return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
            }

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                var rq = await ObjetoPeticionReservaAsync(peticion, busqueda, credencial, quoteId, TokenCancelacion).ConfigureAwait(false);
                if (rq == null)
                    return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);

                var dataJson = JsonSerializer.Serialize(rq, OpcionesJsonReserva);
                var url = $"{credencial.EndPoint.TrimEnd('/')}{RutaReservar}";

                var headers = new Dictionary<string, string>
                {
                    { "Authorization", $"bearer {ObtenerToken(credencial)}" },
                    { "Accept", "application/json" }
                };

                _logger.FinPeticionGds(busqueda.Referencia, idPreReserva, evento, credencial.codigo, 0, $"Inicio Peticion Civitatis BOOK = {dataJson}");

                using var ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(TokenCancelacion);
                ctsTimeout.CancelAfter(TimeSpan.FromSeconds(TimeOutSegundos));

                var respuesta = await _conexionesExternasRepositorio.PeticionExternaTravelKitAsync(
                    idPreReserva, busqueda.Referencia, credencial.codigo,
                    new StringContent(dataJson, Encoding.UTF8, "application/json"),
                    headers, new List<string>(), url, "POST",
                    ctsTimeout.Token, TimeOutSegundos, false).ConfigureAwait(false);

                if (respuesta == null)
                {
                    _logger.ErrorLog(busqueda.Referencia, idPreReserva, evento, credencial.codigo, "Sin respuesta del proveedor.");
                    return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
                }

                var reserva = ProcesarRespuestaReserva(respuesta.Data?.ToString());
                if (reserva == null || string.IsNullOrWhiteSpace(reserva.CartId))
                {
                    var motivo = string.IsNullOrWhiteSpace(reserva?.Message)
                        ? (reserva?.Error ?? respuesta.Ex?.Message ?? "No fue posible confirmar la reserva en Civitatis.")
                        : reserva.Message;
                    _logger.ErrorLog(busqueda.Referencia, idPreReserva, evento, credencial.codigo, motivo);
                    return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
                }

                var precioReserva = MapearPrecioReserva(reserva, credencial);
                var cartId = reserva.CartId!;

                var confirmacion = await ConfirmarCarritoAsync(cartId, peticion, busqueda, credencial, TokenCancelacion).ConfigureAwait(false);
                if (!confirmacion.Confirmado)
                {
                    _logger.ErrorLog(busqueda.Referencia, idPreReserva, evento, credencial.codigo, confirmacion.Motivo ?? "No fue posible confirmar el carrito en Civitatis.");
                    return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
                }

                var localizador = cartId;

                if (!string.IsNullOrWhiteSpace(confirmacion.BookingId))
                    _logger.FinPeticionGds(busqueda.Referencia, idPreReserva, evento, credencial.codigo, 0, $"Civitatis reserva confirmada bookingId={confirmacion.BookingId} cartId={cartId}");

                await CachearConfirmacionAsync(cartId, confirmacion, credencial, TokenCancelacion).ConfigureAwait(false);

                stopwatchTotal.Stop();
                _logger.Respuesta(busqueda.Referencia, idPreReserva, evento, stopwatchTotal.Elapsed.TotalSeconds, 1);

                return new ReservaResponseDTO(Estado.OK, localizador, precioReserva);
            }
            catch (OperationCanceledException)
            {
                return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(busqueda.Referencia, idPreReserva, evento, credencial.codigo, ex.Message);
                return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
            }
        }

        public async Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            const string evento = "Cancelacion";

            if (credencial == null)
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Error Credenciales");

            if (string.IsNullOrWhiteSpace(peticion.Localizador))
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Civitatis: no hay localizador (cartId) para cancelar.");

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                var url = $"{ObtenerEndpointConfirmacion(credencial)}{string.Format(RutaCancelarItems, Uri.EscapeDataString(peticion.Localizador))}";

                var headers = new Dictionary<string, string>
                {
                    { "Authorization", $"bearer {ObtenerToken(credencial)}" },
                    { "Accept", "application/json" }
                };

                _logger.FinPeticionGds(peticion.Localizador, Guid.Empty, evento, credencial.codigo, 0, $"Inicio Peticion Civitatis CANCELAR = {url}");

                using var ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(TokenCancelacion);
                ctsTimeout.CancelAfter(TimeSpan.FromSeconds(TimeOutSegundos));

                var respuesta = await _conexionesExternasRepositorio.PeticionExternaTravelKitAsync(
                    Guid.Empty, peticion.Localizador, credencial.codigo,
                    null!, headers, new List<string>(), url, "DELETE",
                    ctsTimeout.Token, TimeOutSegundos, false).ConfigureAwait(false);

                if (respuesta == null)
                    return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Sin respuesta del proveedor al cancelar la reserva.");

                if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var motivo = "La reserva no existe en Civitatis o ya fue cancelada.";
                    _logger.ErrorLog(peticion.Localizador, Guid.Empty, evento, credencial.codigo, motivo);
                    return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, motivo);
                }

                if (!EsExitoso(respuesta.StatusCode))
                {
                    string? bodyJson = respuesta.Data == null ? null : respuesta.Data.ToString();
                    var errorProveedor = ProcesarRespuestaConfirmacion(bodyJson);
                    var motivo = errorProveedor?.ErrorDescription
                        ?? errorProveedor?.Errors
                        ?? respuesta.Ex?.Message
                        ?? $"Civitatis respondio {(int)respuesta.StatusCode} al cancelar la reserva.";
                    _logger.ErrorLog(peticion.Localizador, Guid.Empty, evento, credencial.codigo, motivo);
                    return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, motivo);
                }

                var mensaje = await ObtenerMensajeCancelacionAsync(peticion.Localizador, credencial, TokenCancelacion).ConfigureAwait(false);

                stopwatchTotal.Stop();
                _logger.FinPeticionGds(peticion.Localizador, Guid.Empty, evento, credencial.codigo, stopwatchTotal.Elapsed.TotalSeconds, $"Fin Cancelacion Civitatis = {mensaje}");

                return new CancelacionResponseDTO(Estado.OK, peticion.Source, peticion.Localizador, mensaje);
            }
            catch (OperationCanceledException)
            {
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Cancelacion abortada");
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, evento, credencial.codigo, ex.Message);
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, ex.Message);
            }
            finally
            {
                stopwatchTotal.Stop();
            }
        }

        private async Task<ReservarRq?> ObjetoPeticionReservaAsync(ReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, string quoteId, CancellationToken TokenCancelacion)
        {
            var trayectos = busqueda.Trayectos.OrderBy(t => t.Numero).ToList();
            if (trayectos.Count == 0)
            {
                _logger.ErrorLog(busqueda.Referencia, peticion.IdPreReserva, "Reserva", credencial.codigo, "Civitatis requiere al menos un trayecto para la reserva.");
                return null;
            }

            // Requisitos de cada tramo desde la cotizacion cacheada.
            var requisitos = await ObtenerRequisitosCotizacionAsync(quoteId, TokenCancelacion).ConfigureAwait(false);

            var rq = new ReservarRq
            {
                QuoteId = quoteId,
                Holder = ObtenerTitular(peticion),
                OutboundDetails = await ObjetoDetallesTransferAsync(trayectos[0], requisitos?.ElementAtOrDefault(0), peticion.Observaciones, busqueda, credencial, TokenCancelacion).ConfigureAwait(false)
            };

            if (trayectos.Count > 1)
            {
                rq.InboundDetails = await ObjetoDetallesTransferAsync(trayectos[1], requisitos?.ElementAtOrDefault(1), null, busqueda, credencial, TokenCancelacion).ConfigureAwait(false);
            }

            var addons = peticion.Pasajeros?
                .Where(p => p.Upgrade != null)
                .SelectMany(p => p.Upgrade)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .Select(u => new AddonReservarRq { Id = u, Quantity = 1 })
                .ToList();

            if (addons != null && addons.Count > 0)
                rq.Addons = addons;

            return rq;
        }

        private async Task<List<List<string>>?> ObtenerRequisitosCotizacionAsync(string quoteId, CancellationToken TokenCancelacion)
        {
            var jsonCotizacion = await _cacheServicio.GetAsync<string>($"{PrefijoCacheCotizacion}{quoteId}", TokenCancelacion).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(jsonCotizacion))
                return null;

            try
            {
                var cotizacion = JsonSerializer.Deserialize<CotizacionRs>(jsonCotizacion, OpcionesJson);
                return cotizacion?.Segments?.Select(s => s.BookingRequirements ?? new List<string>()).ToList();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private async Task<DetallesTransferReservarRq?> ObjetoDetallesTransferAsync(Trayecto trayecto, List<string>? requisitos, string? comentarios, Busqueda busqueda, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            var detalles = new DetallesTransferReservarRq();

            if (requisitos?.Contains("flight", StringComparer.OrdinalIgnoreCase) == true)
            {
                // El dominio de Traslados aun no transporta datos de vuelo; el proveedor validara el requisito.
                _logger.ErrorLog(busqueda.Referencia, busqueda.IdBusqueda, "Reserva", credencial.codigo,
                    "Civitatis: el tramo requiere datos de vuelo y la peticion no los incluye; el proveedor podria rechazar la reserva.");
            }

            var requiereUbicacion = requisitos?.Contains("location", StringComparer.OrdinalIgnoreCase) == true;
            var puntoHotel = trayecto.Destino.EsHotel ? trayecto.Destino : (trayecto.Origen.EsHotel ? trayecto.Origen : null);

            if (puntoHotel != null)
            {
                var descripcion = puntoHotel.Codigo;

                if (Guid.TryParse(puntoHotel.Codigo, out var idHotel))
                {
                    var hotel = await _context.Hoteles.AsNoTracking()
                        .FirstOrDefaultAsync(h => h.IdHotel == idHotel, TokenCancelacion).ConfigureAwait(false);

                    if (hotel != null && !string.IsNullOrWhiteSpace(hotel.Nombre))
                        descripcion = hotel.Nombre;
                }

                detalles.Location = new UbicacionReservarRq { Description = descripcion };
            }
            else if (requiereUbicacion)
            {
                detalles.Location = new UbicacionReservarRq { Description = $"{trayecto.Origen.Codigo} - {trayecto.Destino.Codigo}" };
            }

            if (!string.IsNullOrWhiteSpace(comentarios))
            {
                detalles.Comments = comentarios.Length > LongitudMaximaComentarios
                    ? comentarios.Substring(0, LongitudMaximaComentarios)
                    : comentarios;
            }

            return detalles.Location != null || !string.IsNullOrWhiteSpace(detalles.Comments) ? detalles : null;
        }

        private static HolderReservarRq ObtenerTitular(ReservaDTO peticion)
        {
            var holder = new HolderReservarRq
            {
                Email = peticion.Email ?? string.Empty,
                Phone = peticion.Telefono ?? string.Empty
            };

            var primerPasajero = peticion.Pasajeros?
                .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Nombres) || !string.IsNullOrWhiteSpace(p.Apellidos));

            if (primerPasajero != null)
            {
                holder.FirstName = primerPasajero.Nombres ?? string.Empty;
                holder.LastName = primerPasajero.Apellidos ?? string.Empty;
                return holder;
            }

            if (!string.IsNullOrWhiteSpace(peticion.NombreContacto))
            {
                var partes = peticion.NombreContacto.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                holder.FirstName = partes.Length > 0 ? partes[0] : string.Empty;
                holder.LastName = partes.Length > 1 ? partes[1] : "-";
            }

            return holder;
        }

        private static ReservarRs? ProcesarRespuestaReserva(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ReservarRs>(json, OpcionesJson);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static PrecioReservaDTO MapearPrecioReserva(ReservarRs reserva, IntegracionDTO credencial)
        {
            var moneda = string.IsNullOrWhiteSpace(reserva.Price?.Total?.Currency) ? "EUR" : reserva.Price.Total.Currency;
            var valor = (reserva.Price?.Total?.Amount ?? 0) / 100m;
            var total = new TotalReservaDTO(moneda, valor, 0m, valor);

            return new PrecioReservaDTO(
                credencial.CodigoIntegrador,
                total,
                new List<PTCTotalResevaDTO> { new PTCTotalResevaDTO(TipoAdulto, 1, total) });
        }

        private async Task<(bool Confirmado, string? BookingId, string? Motivo)> ConfirmarCarritoAsync(string cartId, ReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            const string evento = "ConfirmarCarrito";
            var walletKey = ObtenerWalletKey(credencial);
            if (string.IsNullOrWhiteSpace(walletKey))
                return (false, null, "Civitatis no tiene WalletKey configurada en las credenciales de la integracion.");

            var rq = new ConfirmarRq { WalletKey = walletKey };
            var dataJson = JsonSerializer.Serialize(rq, OpcionesJsonReserva);
            var url = $"{ObtenerEndpointConfirmacion(credencial)}{string.Format(RutaConfirmarCarrito, Uri.EscapeDataString(cartId))}";

            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"bearer {ObtenerToken(credencial)}" },
                { "Accept", "application/json" }
            };

            for (var intento = 1; intento <= IntentosConfirmacionCarrito; intento++)
            {
                _logger.FinPeticionGds(busqueda.Referencia, peticion.IdPreReserva, evento, credencial.codigo, 0, $"Inicio Peticion Civitatis CONFIRM (intento {intento}/{IntentosConfirmacionCarrito}) = {dataJson}");

                using var ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(TokenCancelacion);
                ctsTimeout.CancelAfter(TimeSpan.FromSeconds(TimeOutSegundos));

                var respuesta = await _conexionesExternasRepositorio.PeticionExternaTravelKitAsync(
                    peticion.IdPreReserva, busqueda.Referencia, credencial.codigo,
                    new StringContent(dataJson, Encoding.UTF8, "application/json"),
                    headers, new List<string>(), url, "POST",
                    ctsTimeout.Token, TimeOutSegundos, false).ConfigureAwait(false);

                if (respuesta == null)
                    return (false, null, "Sin respuesta del proveedor al confirmar el carrito.");

                string? bodyJson = respuesta.Data == null ? null : respuesta.Data.ToString();
                var confirmacion = ProcesarRespuestaConfirmacion(bodyJson);

                if (respuesta.Status && confirmacion != null)
                    return (true, confirmacion.BookingId, null);

                var confirmacionEnCurso = respuesta.StatusCode == System.Net.HttpStatusCode.Conflict
                    || confirmacion?.HttpStatus == 409
                    || confirmacion?.ErrorId == 57;

                if (confirmacionEnCurso && intento < IntentosConfirmacionCarrito)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(EsperaReintentoConfirmacionSegundos), TokenCancelacion).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return (false, null, "Confirmacion cancelada mientras se esperaba el reintento del carrito en curso.");
                    }

                    continue;
                }

                var motivo = confirmacion?.ErrorDescription
                    ?? confirmacion?.Errors
                    ?? respuesta.Ex?.Message
                    ?? "No fue posible confirmar el carrito en Civitatis.";

                if (confirmacion?.ItemErrors is { Count: > 0 } itemErrors)
                    motivo += $" | itemErrors: {string.Join("; ", itemErrors.Select(i => i.Code ?? i.ItemId ?? string.Empty))}";

                return (false, null, motivo);
            }

            return (false, null, "El carrito ya tiene una confirmacion en curso: no fue posible completarla tras los reintentos.");
        }

        private static ConfirmarRs? ProcesarRespuestaConfirmacion(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ConfirmarRs>(json, OpcionesJson);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string ObtenerEndpointConfirmacion(IntegracionDTO credencial)
        {
            var endpoint = (credencial.EndPoint ?? string.Empty).TrimEnd('/');

            if (endpoint.EndsWith(SufijoVersionV3, StringComparison.OrdinalIgnoreCase))
                return endpoint.Substring(0, endpoint.Length - SufijoVersionV3.Length) + SufijoVersionV2;

            return endpoint;
        }

        private static string ObtenerWalletKey(IntegracionDTO credencial)
        {
            var walletKey = credencial.credencial.FirstOrDefault(c =>
                c.nombre.Equals("WalletKey", StringComparison.CurrentCultureIgnoreCase) ||
                c.nombre.Equals("Wallet", StringComparison.CurrentCultureIgnoreCase) ||
                c.nombre.Equals("Cartera", StringComparison.CurrentCultureIgnoreCase))?.valor;

            if (!string.IsNullOrWhiteSpace(walletKey))
                return walletKey;

            return string.IsNullOrWhiteSpace(credencial.Clave) ? string.Empty : credencial.Clave;
        }

        private async Task CachearConfirmacionAsync(string cartId, ConfirmarRs confirmacion, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            try
            {
                var json = JsonSerializer.Serialize(confirmacion, OpcionesJsonReserva);
                await _cacheServicio.SetAsync($"{PrefijoCacheReserva}{cartId}", json, TtlCacheReserva, TokenCancelacion).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(cartId, Guid.Empty, "Reserva", credencial.codigo, $"No fue posible cachear la confirmacion de Civitatis: {ex.Message}");
            }
        }

        private async Task<string> ObtenerMensajeCancelacionAsync(string cartId, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            const string mensajeBase = "Reserva cancelada correctamente en Civitatis.";

            try
            {
                var confirmacion = await ObtenerConfirmacionCacheadaAsync(cartId, TokenCancelacion).ConfigureAwait(false);
                var bookingId = string.IsNullOrWhiteSpace(confirmacion?.BookingId)
                    ? confirmacion?.Items?.FirstOrDefault()?.Id
                    : confirmacion.BookingId;

                if (string.IsNullOrWhiteSpace(bookingId))
                    return mensajeBase;

                var detalles = await ConsultarDetallesReservaAsync(bookingId, credencial, TokenCancelacion).ConfigureAwait(false);
                if (detalles == null || string.IsNullOrWhiteSpace(detalles.Status))
                    return mensajeBase;

                return $"{mensajeBase} Estado en proveedor: {detalles.Status}.";
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(cartId, Guid.Empty, "Cancelacion", credencial.codigo, $"No fue posible verificar el estado tras cancelar: {ex.Message}");
                return mensajeBase;
            }
        }

        private async Task<ConfirmarRs?> ObtenerConfirmacionCacheadaAsync(string cartId, CancellationToken TokenCancelacion)
        {
            var json = await _cacheServicio.GetAsync<string>($"{PrefijoCacheReserva}{cartId}", TokenCancelacion).ConfigureAwait(false);
            return ProcesarRespuestaConfirmacion(json);
        }

        private async Task<DetallesReservaRs?> ConsultarDetallesReservaAsync(string bookingId, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            var url = $"{ObtenerEndpointConfirmacion(credencial)}{string.Format(RutaDetallesReserva, Uri.EscapeDataString(bookingId))}";

            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"bearer {ObtenerToken(credencial)}" },
                { "Accept", "application/json" }
            };

            _logger.FinPeticionGds(bookingId, Guid.Empty, "ConsultarDetallesReserva", credencial.codigo, 0, $"Inicio Peticion Civitatis DETALLES = {url}");

            using var ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(TokenCancelacion);
            ctsTimeout.CancelAfter(TimeSpan.FromSeconds(TimeOutSegundos));

            var respuesta = await _conexionesExternasRepositorio.PeticionExternaTravelKitAsync(
                Guid.Empty, bookingId, credencial.codigo,
                null!, headers, new List<string>(), url, "GET",
                ctsTimeout.Token, TimeOutSegundos, false).ConfigureAwait(false);

            if (respuesta == null || !EsExitoso(respuesta.StatusCode))
                return null;

            string? bodyJson = respuesta.Data == null ? null : respuesta.Data.ToString();
            return ProcesarRespuestaDetallesReserva(bodyJson);
        }

        private static DetallesReservaRs? ProcesarRespuestaDetallesReserva(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<DetallesReservaRs>(json, OpcionesJson);
            }
            catch (JsonException)
            {
            }

            try
            {
                var lista = JsonSerializer.Deserialize<List<DetallesReservaRs>>(json, OpcionesJson);
                return lista?.FirstOrDefault();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static bool EsExitoso(System.Net.HttpStatusCode codigo)
        {
            var valor = (int)codigo;
            return valor >= 200 && valor < 300;
        }
    }
}
