using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using Naideth.Traslados.Aplicacion.ConexionesExternas.Interfaces;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rs;
using Naideth.Traslados.Dominio.Busquedas;
using Naideth.Traslados.Dominio.Estados;
using Naideth.Traslados.Dominio.Trayectos;
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces;
using Naideth.Traslados.Infrastructura.INeedTours.Models.Rq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Naideth.Central.Dominio.Hoteles;

namespace Naideth.Traslados.Infrastructura.INeedTours
{
    internal class INeedToursRepositorio : IINeedToursRepositorio
    {
        private readonly IApplicationDbContext _context;
        private readonly IConexionesExternasRepositorio _conexionesExternasRepositorio;
        private readonly IAppLoggerServicio _logger;
        private const string Codigointegracion = "INT";
        private const string IdiomaPorDefecto = "ES";
        private const string TipoAdulto = "ADT";
        private const string TipoAdultoLider = "ADL";
        private const string TipoNino = "CHL";
        private const string CategoriaTraslado = "Traslado";
        private const string MonedaPorDefecto = "EUR";
        private const string BeneficioContenido = "contenido";
        private const string BeneficioHorario = "horario";
        private const string TipoUbicacionAeropuerto = "A";
        private const string TipoUbicacionHotel = "H";
        // Nombres de nodos y atributos XML de INeedTours (repetidos entre parsers).
        private const string NodoFault = "Fault";
        private const string NodoFaultString = "faultstring";
        private const string NodoProducts = "Products";
        private const string NodoConcepts = "Concepts";
        private const string NodoDetails = "Details";
        private const string NodoTotal = "Total";
        private const string NodoTaxes = "Taxes";
        private const string NodoCommissions = "Commissions";
        private const string NodoNetAmount = "NetAmount";
        private const string NodoSellingPrice = "SellingPrice";
        private const string NodoBookResponseV2Product = "BookResponseV2Product";
        private const string NodoBookResponseV2Concept = "BookResponseV2Concept";
        private const string NodoBookResponseV2Detail = "BookResponseV2Detail";
        private const string NodoTitle = "Title";
        private const string NodoSmallContent = "SmallContent";
        private const string NodoCancellationFees = "CancellationFees";
        private const string NodoConceptBookingCode = "ConceptBookingCode";
        private const string AtributoCurrencyCode = "CurrencyCode";
        private const string AtributoTransactionIdentifier = "TransactionIdentifier";
        private const string AtributoResResponseType = "ResResponseType";
        private const string AtributoAmount = "Amount";
        private const string AtributoEchoToken = "EchoToken";

        public INeedToursRepositorio(IApplicationDbContext context, IConexionesExternasRepositorio conexionesExternasRepositorio, IAppLoggerServicio logger)
        {
            _context = context;
            _logger = logger;
            _conexionesExternasRepositorio = conexionesExternasRepositorio;
        }

        #region Publicos

        public async Task<ResponseDTO> BusquedaAsync(Busqueda peticion, IntegracionDTO credencial, FiltroRequestDTO? filtros, CancellationToken TokenCancelacion)
        {
            string evento = "Busqueda";
            var idBusqueda = peticion.IdBusqueda;
            if (credencial == null)
                return new ResponseDTO(Estado.ERROR, null);

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                if (peticion.Trayectos is null || peticion.Trayectos.Count == 0)
                {
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, credencial.codigo, "INeedTours requiere al menos un trayecto para la busqueda.");
                    return new ResponseDTO(Estado.ERROR, null);
                }

                var rq = await ObjetoPeticionBusqueda(peticion, credencial, TokenCancelacion);
                var dataXml = rq.SerializarXml();

                var (xmlRespuesta, _) = await EnviarSoapAsync(idBusqueda, peticion.Referencia, evento, "Peticion INeedTours SEARCH", dataXml, credencial, TokenCancelacion).ConfigureAwait(false);

                if (xmlRespuesta == null)
                    return new ResponseDTO(Estado.ERROR, null);

                // Diagnostico temporal: vuelca la respuesta cruda para contrastar la estructura
                // real de ConceptBookingCode / DetailBookingCode con la documentacion.
                _logger.FinPeticionGds(peticion.Referencia, idBusqueda, evento, credencial.codigo, 0,
                    $"Respuesta SEARCH cruda = {(xmlRespuesta.Length > 8000 ? xmlRespuesta.Substring(0, 8000) : xmlRespuesta)}");

                stopwatchTotal.Stop();

                var disponibilidades = ProcesarRespuestaBusqueda(xmlRespuesta, peticion, credencial);
                if (disponibilidades == null)
                {
                    _logger.ErrorLog(peticion.Referencia, idBusqueda, evento, credencial.codigo, "No fue posible interpretar la respuesta de INeedTours.");
                    return new ResponseDTO(Estado.ERROR, null);
                }

                _logger.Respuesta(peticion.Referencia, idBusqueda, evento, stopwatchTotal.Elapsed.TotalSeconds, disponibilidades.Disponibilidades?.Count ?? 0);

                return new ResponseDTO(Estado.OK, disponibilidades);
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
            finally
            {
                stopwatchTotal.Stop();
            }
        }

        public async Task<PreReservaResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, IntegracionDTO credenciales, DisponibilidadDTO disponibilidadSeleccionada, CancellationToken TokenCancelacion)
        {
            string evento = "PreReserva";

            var idBusqueda = busqueda.IdBusqueda;

            if (credenciales == null)
                return new PreReservaResponseDTO(Estado.ERROR, null);

            var tarifaSeleccionada = disponibilidadSeleccionada?.Tarifas?.FirstOrDefault();

            if (tarifaSeleccionada == null)
            {
                _logger.ErrorLog(busqueda.Referencia, idBusqueda, evento, credenciales.codigo, "INeedTours requiere una tarifa seleccionada para el pre-book.");
                return new PreReservaResponseDTO(Estado.ERROR, null);
            }

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                var dataXml = ObjetoPeticionPreReserva(credenciales, tarifaSeleccionada);
                
                var (xmlRespuesta, _) = await EnviarSoapAsync(idBusqueda, busqueda.Referencia, evento, "Peticion INeedTours BOOK", dataXml, credenciales, TokenCancelacion).ConfigureAwait(false);

                if (xmlRespuesta == null)
                    return new PreReservaResponseDTO(Estado.ERROR, null);

                var disponibilidad = ProcesarRespuestaPreReserva(xmlRespuesta, disponibilidadSeleccionada, tarifaSeleccionada, busqueda, credenciales);

                if (disponibilidad == null)
                {
                    _logger.ErrorLog(busqueda.Referencia, idBusqueda, evento, credenciales.codigo, "No fue posible interpretar la respuesta de INeedTours.");
                    return new PreReservaResponseDTO(Estado.ERROR, null);
                }

                stopwatchTotal.Stop();
                _logger.Respuesta(busqueda.Referencia, idBusqueda, evento, stopwatchTotal.Elapsed.TotalSeconds, 1);

                return new PreReservaResponseDTO(Estado.OK, new DisponibilidadesDTO(credenciales.CodigoIntegrador, credenciales.nombre, new List<DisponibilidadDTO> { disponibilidad }));
            }
            catch (OperationCanceledException)
            {
                return new PreReservaResponseDTO(Estado.ERROR, null);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(busqueda.Referencia, idBusqueda, evento, credenciales.codigo, ex.Message);
                return new PreReservaResponseDTO(Estado.ERROR, null);
            }
            finally
            {
                stopwatchTotal.Stop();
            }
        }

        public async Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, DisponibilidadDTO disponibilidadSeleccionada, CancellationToken TokenCancelacion)
        {
            string evento = "Reserva";
            var idPreReserva = peticion.IdPreReserva;
            if (credencial == null)
                return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);

            var tarifaSeleccionada = disponibilidadSeleccionada?.Tarifas?.FirstOrDefault();
            if (tarifaSeleccionada == null)
            {
                _logger.ErrorLog(busqueda.Referencia, idPreReserva, evento, credencial.codigo, "INeedTours requiere una tarifa seleccionada para la reserva.");
                return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
            }

            // Sin ConceptoReserva (devuelto por BOOK) no se puede confirmar el commit.
            if (string.IsNullOrWhiteSpace(tarifaSeleccionada.DatosTarifa?.ConceptoReserva))
            {
                _logger.ErrorLog(busqueda.Referencia, idPreReserva, evento, credencial.codigo,
                    "INeedTours: no hay ConceptoReserva (respuesta BOOK) para armar el commit.");
                return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
            }

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                var rq = ObjetoPeticionReserva(peticion, busqueda, credencial, disponibilidadSeleccionada, tarifaSeleccionada);

                var dataXml = rq.SerializarXml();

                var (xmlRespuesta, _) = await EnviarSoapAsync(idPreReserva, busqueda.Referencia, evento, "Peticion INeedTours COMMIT", dataXml, credencial, TokenCancelacion).ConfigureAwait(false);

                if (xmlRespuesta == null)
                    return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);

                var (localizador, precioReserva) = ProcesarRespuestaReserva(xmlRespuesta, busqueda, credencial);

                if (string.IsNullOrEmpty(localizador))
                {
                    _logger.ErrorLog(busqueda.Referencia, idPreReserva, evento, credencial.codigo, "No fue posible confirmar la reserva en INeedTours.");
                    return new ReservaResponseDTO(Estado.ERROR, string.Empty, null);
                }

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
            finally
            {
                stopwatchTotal.Stop();
            }
        }

        public async Task<CancelacionResponseDTO> PoliciesAsync(CancelacionDTO peticion, IntegracionDTO Credenciales, CancellationToken TokenCancelacion)
        {
            string evento = "Policies";

            if (Credenciales == null)
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Error Credenciales");

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                var rq = ObjetoPeticionPolicies(peticion, Credenciales);

                var dataXml = rq.SerializarXml();

                var (xmlRespuesta, error) = await EnviarSoapAsync(Guid.Empty, peticion.Localizador, evento, "Peticion INeedTours Policies", dataXml, Credenciales, TokenCancelacion).ConfigureAwait(false);

                if (xmlRespuesta == null)
                    return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, error);

                var resultado = ProcesarRespuestaPolicies(xmlRespuesta, peticion, Credenciales);

                if (resultado == null)
                {
                    _logger.ErrorLog(peticion.Localizador, Guid.Empty, evento, Credenciales.codigo, "No fue posible interpretar la respuesta de INeedTours.");
                    return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "No fue posible interpretar la respuesta de INeedTours.");
                }

                stopwatchTotal.Stop();

                return resultado;
            }
            catch (OperationCanceledException)
            {
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Policies abortada");
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, evento, Credenciales.codigo, ex.Message);
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, ex.Message);
            }
            finally
            {
                stopwatchTotal.Stop();
            }
        }

        public async Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, IntegracionDTO Credenciales, CancellationToken TokenCancelacion)
        {
            string evento = "Cancelacion";

            if (Credenciales == null)
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Error Credenciales");

            var stopwatchTotal = Stopwatch.StartNew();

            try
            {
                var rq = ObjetoPeticionCancelacion(peticion, Credenciales);

                var dataXml = rq.SerializarXml();

                var (xmlRespuesta, error) = await EnviarSoapAsync(Guid.Empty, peticion.Localizador, evento, "Peticion INeedTours Cancelacion", dataXml, Credenciales, TokenCancelacion).ConfigureAwait(false);

                if (xmlRespuesta == null)
                    return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, error);

                stopwatchTotal.Stop();

                return ProcesarRespuestaCancelacion(xmlRespuesta, peticion, Credenciales);
            }
            catch (OperationCanceledException)
            {
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Cancelacion abortada");
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, evento, Credenciales.codigo, ex.Message);
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, ex.Message);
            }
            finally
            {
                stopwatchTotal.Stop();
            }
        }

        #endregion

        #region Privados

        private async Task<(string? Xml, string Error)> EnviarSoapAsync(Guid id, string referencia, string evento, string nombreLog, string dataXml, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            var stopwatchExterno = Stopwatch.StartNew();

            _logger.FinPeticionGds(referencia, id, evento, credencial.codigo, stopwatchExterno.Elapsed.TotalSeconds, $"Inicio {nombreLog} = {dataXml}");

            // Postman no envia SOAPAction para este ASMX y responde correctamente;
            // un SOAPAction vacio provoca un 500 en el servidor, por eso se omite.
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "Content-Type", "text/xml" }
            };

            using var ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(TokenCancelacion);
            ctsTimeout.CancelAfter(TimeSpan.FromSeconds(60));

            var respuesta = await _conexionesExternasRepositorio.PeticionExternaSoapAsync(
                id, referencia, credencial.codigo, dataXml, headers, credencial.EndPoint,
                ctsTimeout.Token, 60, false).ConfigureAwait(false);

            stopwatchExterno.Stop();
            _logger.FinPeticionGds(referencia, id, evento, credencial.codigo, stopwatchExterno.Elapsed.TotalSeconds, $"Fin {nombreLog}");

            if (respuesta == null || respuesta.Status == false)
            {
                var error = respuesta?.Ex?.Message ?? "Sin respuesta del proveedor";
                _logger.ErrorLog(referencia, id, evento, credencial.codigo, error);
                return (null, error);
            }

            return (respuesta.Data?.ToString() ?? string.Empty, string.Empty);
        }

        private static string ObtenerIdioma(IntegracionDTO credencial)
        {
            return credencial.credencial.FirstOrDefault(c => c.nombre.Equals("Idioma", StringComparison.CurrentCultureIgnoreCase))?.valor ?? IdiomaPorDefecto;
        }

        private static string? ObtenerFaultString(XDocument doc)
        {
            var fault = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == NodoFault);
            if (fault == null)
                return null;

            return fault.Elements().FirstOrDefault(e => e.Name.LocalName == NodoFaultString)?.Value ?? "Fault SOAP sin detalle";
        }

        private CancelRq ObjetoPeticionCancelacion(CancelacionDTO peticion, IntegracionDTO credencial)
        {
            var idioma = ObtenerIdioma(credencial);

            var rq = new CancelRq();

            rq.Body.DestServicesCancelV2.ObjCredentials.Source.RequestorID.ID = credencial.Usuario;
            rq.Body.DestServicesCancelV2.ObjCredentials.Source.RequestorID.MessagePassword = credencial.Clave;

            var objRequest = rq.Body.DestServicesCancelV2.ObjRequest;

            objRequest.PrimaryLangID = idioma;
            objRequest.TransactionIdentifier = peticion.Localizador;

            // CancelBookingItems queda null: se cancela la reserva completa
            // (los items solo aplican para cancelaciones parciales).

            return rq;
        }

        private async Task<SearchRq> ObjetoPeticionBusqueda(Busqueda peticion, IntegracionDTO credencial, CancellationToken TokenCancelacion)
        {
            var idioma = ObtenerIdioma(credencial);

            var rq = new SearchRq();

            rq.Body.DestServicesAvailV2.ObjCredentials.Source.RequestorID.ID = credencial.Usuario;
            rq.Body.DestServicesAvailV2.ObjCredentials.Source.RequestorID.MessagePassword = credencial.Clave;

            var objRequest = rq.Body.DestServicesAvailV2.ObjRequest;

            objRequest.PrimaryLangID = idioma;
            objRequest.ServiceType = "T";

            // Occupations: una entrada por cada edad de pasajero (ADT -> ADL, CHD/INF -> CHL).
            foreach (var pasajero in peticion.Pasajeros)
            {
                var tipo = pasajero.Tipo == TipoAdulto ? TipoAdultoLider : TipoNino;

                foreach (var edad in pasajero.Edades)
                {
                    objRequest.Occupations.Add(new Occupation { Type = tipo, Age = edad });
                }
            }

            var trayectosOrdenados = peticion.Trayectos.OrderBy(t => t.Numero).ToList();
            var trayectoIda = trayectosOrdenados.First();
            var trayectoVuelta = trayectosOrdenados.Last();

            objRequest.StayDateRange.Start = trayectoIda.Fecha.ToString("yyyy-MM-dd");
            objRequest.StayDateRange.End = trayectoVuelta.Fecha.ToString("yyyy-MM-dd");

            var esIdaYVuelta = peticion.AgregarRegreso || trayectosOrdenados.Count > 1;

            objRequest.TransferOptions.Type = esIdaYVuelta ? "ROUNDTRIP" : "ONEWAYTRIP";
            objRequest.TransferOptions.LocationOriginTime = trayectoIda.ObtenerHora().ToString(@"hh\:mm");
            objRequest.TransferOptions.LocationDestinationTime = trayectoVuelta.ObtenerHora().ToString(@"hh\:mm");

            objRequest.TransferOptions.LocationOrigin = await ObtenerLocationInfoAsync(trayectoIda.Origen, TokenCancelacion);
            objRequest.TransferOptions.LocationDestination = await ObtenerLocationInfoAsync(trayectoIda.Destino, TokenCancelacion);

            objRequest.DataOptions = new DataOptions { Contents = true, Images = true, Attributes = true };

            return rq;
        }

        private async Task<LocationInfo?> ObtenerLocationInfoAsync(Ubicacion ubicacion, CancellationToken TokenCancelacion)
        {
            if (ubicacion.EsIATA)
                return new LocationInfo { Type = TipoUbicacionAeropuerto, IATA = ubicacion.Codigo };

            if (!Guid.TryParse(ubicacion.Codigo, out var idHotel))
                return null;

            var hotel = await _context.Hoteles.AsNoTracking()
                .FirstOrDefaultAsync(h => h.IdHotel == idHotel, TokenCancelacion)
                .ConfigureAwait(false);

            if (hotel == null)
                return null;

            var (latitud, longitud) = ParsearCoordenadas(hotel.Coordenadas);

            return new LocationInfo
            {
                Type = TipoUbicacionHotel,
                Name = hotel.Nombre ?? string.Empty,
                Address = hotel.Direccion ?? string.Empty,
                Coordinates = new Coordinates
                {
                    Latitude = latitud ?? string.Empty,
                    Longitude = longitud ?? string.Empty
                }
            };
        }

        private static (string? Latitud, string? Longitud) ParsearCoordenadas(string coordenadas)
        {
            if (string.IsNullOrWhiteSpace(coordenadas))
                return (null, null);

            string? latitud = null;
            string? longitud = null;

            foreach (var token in coordenadas.Split(','))
            {
                var limpio = token.Trim();

                if (string.IsNullOrWhiteSpace(limpio))
                    continue;

                // Formato con prefijos: "lat:XX.XXX, lon:YY.YYY"
                if (limpio.StartsWith("lat:", StringComparison.OrdinalIgnoreCase))
                    latitud = limpio[4..].Trim();
                else if (limpio.StartsWith("lon:", StringComparison.OrdinalIgnoreCase))
                    longitud = limpio[4..].Trim();
                // Formato simple: "latitud, longitud" (primer valor = lat, segundo = lon)
                else if (latitud == null)
                    latitud = limpio;
                else if (longitud == null)
                    longitud = limpio;
            }

            return (latitud, longitud);
        }

        private DisponibilidadesDTO? ProcesarRespuestaBusqueda(string xmlRespuesta, Busqueda peticion, IntegracionDTO credencial)
        {
            XDocument doc;

            try
            {
                doc = XDocument.Parse(SanearEntidadesHtml(xmlRespuesta));
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "Busqueda", credencial.codigo, $"Error al parsear respuesta SEARCH: {ex.Message}");
                return null;
            }

            // Fault SOAP: se registra el faultstring y se devuelve null.
            var faultString = ObtenerFaultString(doc);

            if (faultString != null)
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "Busqueda", credencial.codigo, faultString);
                return null;
            }

            var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "DestServicesAvailV2Result");

            if (result == null)
                return null;

            var echoToken = result.Attribute(AtributoEchoToken)?.Value ?? string.Empty;
            var totalPax = peticion.Pasajeros.Sum(p => p.Cantidad);

            var disponibilidades = new List<DisponibilidadDTO>();

            foreach (var producto in result.Elements()
                .FirstOrDefault(e => e.Name.LocalName == NodoProducts)?
                .Elements().Where(e => e.Name.LocalName == "AvailResponseV2Product") ?? Enumerable.Empty<XElement>())
            {
                var productoAvail = MapearProducto(producto);

                foreach (var concepto in producto.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == NodoConcepts)?
                    .Elements().Where(e => e.Name.LocalName == "AvailResponseV2Concept") ?? Enumerable.Empty<XElement>())
                {
                    var disponibilidad = MapearConcepto(concepto, productoAvail, echoToken, totalPax, peticion, credencial);

                    if (disponibilidad != null)
                        disponibilidades.Add(disponibilidad);
                }
            }

            return new DisponibilidadesDTO(credencial.CodigoIntegrador, credencial.nombre, disponibilidades);
        }

        private record ProductoAvail(
            string? Nombre,
            string Categoria,
            string Dias,
            string CodigoProducto,
            string ProductCode,
            string Imagen,
            string MonedaDesde,
            decimal? ImporteDesde);

        private static ProductoAvail MapearProducto(XElement producto)
        {
            // Precio "desde" del producto como respaldo si el concepto no trae detalle.
            var precioDesde = producto.Elements().FirstOrDefault(e => e.Name.LocalName == "PriceFrom");

            return new ProductoAvail(
                Valor(producto, "ProductName"),
                Valor(producto, "ProductCategory") ?? CategoriaTraslado,
                Valor(producto, "ProductDays") ?? "1",
                Valor(producto, "ProductBookingCode") ?? string.Empty,
                Valor(producto, "ProductCode") ?? string.Empty,
                ExtraerImagenProducto(producto),
                precioDesde?.Attribute(AtributoCurrencyCode)?.Value ?? MonedaPorDefecto,
                DecimalElemento(precioDesde, "Price"));
        }

        private static DisponibilidadDTO? MapearConcepto(XElement concepto, ProductoAvail producto, string echoToken, int totalPax, Busqueda peticion, IntegracionDTO credencial)
        {
            var conceptoBookingCode = Valor(concepto, NodoConceptBookingCode);

            if (string.IsNullOrWhiteSpace(conceptoBookingCode))
                return null;

            var conceptCode = Valor(concepto, "ConceptCode") ?? string.Empty;
            var nombreConcepto = Valor(concepto, "ConceptName") ?? producto.Nombre;

            var detalle = concepto.Elements().FirstOrDefault(e => e.Name.LocalName == NodoDetails)?
                .Elements().FirstOrDefault(e => e.Name.LocalName == "AvailResponseV2Detail");

            // Localizador del detalle seleccionado, requerido por DestServicesBookV2.
            // En la respuesta es un ELEMENTO (<DetailBookingCode>...), no un atributo.
            var localizador = Valor(detalle, "DetailBookingCode") ?? string.Empty;

            var impuestos = detalle?.Elements().FirstOrDefault(e => e.Name.LocalName == NodoTotal)?
                .Elements().FirstOrDefault(e => e.Name.LocalName == NodoTaxes);

            var importe = impuestos?.Attribute(AtributoAmount) != null
                ? DecimalAtributo(impuestos, AtributoAmount) ?? 0m
                : producto.ImporteDesde ?? 0m;

            var moneda = impuestos?.Attribute(AtributoCurrencyCode)?.Value ?? producto.MonedaDesde;

            var comisiones = impuestos?.Elements().FirstOrDefault(e => e.Name.LocalName == NodoCommissions);

            var precioVenta = DecimalElemento(comisiones, NodoSellingPrice) ?? importe;
            var precioNeto = DecimalElemento(comisiones, NodoNetAmount) ?? precioVenta;

            var impuesto = precioVenta - precioNeto;

            // Horarios recalculados por el proveedor (si los devuelve).
            var transfer = concepto.Elements().FirstOrDefault(e => e.Name.LocalName == "Transfer");
            var horaRecogida = transfer?.Attribute("PickUpTime")?.Value ?? string.Empty;
            var horaEntrega = transfer?.Attribute("DeliveryTime")?.Value ?? string.Empty;

            var beneficios = ExtraerBeneficios(concepto, horaRecogida, horaEntrega);
            var modeloVehiculo = ExtraerModeloVehiculo(concepto);
            var imagen = ExtraerImagenConcepto(concepto) ?? producto.Imagen;

            var rangoEdad = $"{Valor(concepto, "AgeFrom")}-{Valor(concepto, "AgeTo")}";

            var total = new TotalDTO(moneda, precioNeto, impuesto, precioVenta);

            var precio = new PrecioDTO(total,
                new List<PTCTotalDTO> { new PTCTotalDTO(TipoAdulto, 1, total) });

            return new DisponibilidadDTO
            {
                Source = credencial.CodigoIntegrador,
                SourceName = credencial.nombre,
                Id = Guid.NewGuid().ToString(),
                Auxiliares = new List<TpaDTO> { new TpaDTO("MonedaOrigen", moneda) },
                Plan = nombreConcepto ?? string.Empty,
                Duracion = producto.Dias,
                Tipo = CategoriaTraslado,
                FechaInicio = peticion.Inicio,
                FechaSalida = peticion.Fin,
                HoraInicio = horaRecogida,
                HoraSalida = horaEntrega,
                Imagen = imagen,
                Precio = precio,
                Tarifas = new List<TarifaDTO>
                {
                    new TarifaDTO
                    {
                        Id = conceptoBookingCode,
                        IdPreReserva = Guid.NewGuid().ToString(),
                        Category = producto.Categoria,
                        Nombre = nombreConcepto ?? string.Empty,
                        Beneficios = beneficios,
                        RangoEdad = rangoEdad,
                        CantidadAdultos = totalPax,
                        CantidadNinos = 0,
                        CantidadInfantes = 0,
                        DatosTarifa = new DatosTarifaDTO
                        {
                            Localizador = localizador,
                            CodigoProducto = producto.CodigoProducto,
                            CodigoAuxiliar = $"{echoToken}|{producto.ProductCode}|{conceptCode}",
                            TipoVehiculo = producto.Nombre ?? nombreConcepto,
                            ModeloVehiculo = modeloVehiculo,
                            ImagenVehiculo = imagen,
                            HoraInicio = horaRecogida
                        }
                    }
                }
            };
        }

        private static string ExtraerImagenProducto(XElement producto)
        {
            return Valor(producto, "ProductImage")
                ?? ExtraerImagenConcepto(producto)
                ?? string.Empty;
        }

        private static string? ExtraerImagenConcepto(XElement contenedor)
        {
            return contenedor.Elements().FirstOrDefault(e => e.Name.LocalName == "Images")?
                .Elements().FirstOrDefault(e => e.Name.LocalName == "AvailResponseV2Image")?
                .Elements().FirstOrDefault(e => e.Name.LocalName == "URL")?.Value;
        }

        private static IEnumerable<XElement> ObtenerContenidos(XElement concepto)
        {
            return concepto.Elements().FirstOrDefault(e => e.Name.LocalName == "Contents")?
                .Elements().Where(e => e.Name.LocalName == "AvailResponseV2Content") ?? Enumerable.Empty<XElement>();
        }

        private static List<BeneficiosDTO> ExtraerBeneficios(XElement concepto, string horaRecogida, string horaEntrega)
        {
            var beneficios = new List<BeneficiosDTO>();

            foreach (var contenido in ObtenerContenidos(concepto))
            {
                var titulo = Valor(contenido, NodoTitle);
                var detalleContenido = Valor(contenido, NodoSmallContent);

                if (!string.IsNullOrWhiteSpace(titulo) || !string.IsNullOrWhiteSpace(detalleContenido))
                    beneficios.Add(new BeneficiosDTO(titulo ?? string.Empty, detalleContenido ?? string.Empty, BeneficioContenido));
            }

            if (!string.IsNullOrWhiteSpace(horaRecogida))
                beneficios.Add(new BeneficiosDTO("Horario de recogida", horaRecogida, BeneficioHorario));

            if (!string.IsNullOrWhiteSpace(horaEntrega))
                beneficios.Add(new BeneficiosDTO("Horario de entrega", horaEntrega, BeneficioHorario));

            return beneficios;
        }

        // Modelo del vehículo: Title del primer Content (o SmallContent si no hay Title).
        private static string? ExtraerModeloVehiculo(XElement concepto)
        {
            var contenidos = ObtenerContenidos(concepto);

            return contenidos
                .Select(c => Valor(c, NodoTitle))
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t))
                ?? contenidos
                    .Select(c => Valor(c, NodoSmallContent))
                    .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        }

        private static string? Valor(XElement? padre, string nombreLocal)
        {
            return padre?.Elements().FirstOrDefault(e => e.Name.LocalName == nombreLocal)?.Value;
        }

        private static decimal? DecimalElemento(XElement? padre, string nombreLocal)
        {
            var valor = Valor(padre, nombreLocal);

            return decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultado) ? resultado : null;
        }

        private static decimal? DecimalAtributo(XElement? elemento, string nombreAtributo)
        {
            var valor = elemento?.Attribute(nombreAtributo)?.Value;

            return decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultado) ? resultado : null;
        }

        private static string SanearEntidadesHtml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return xml;

            // El proveedor incrusta entidades HTML (&aacute;, &bull;, &nbsp;, ...) que no son
            // predefinidas en XML y romperian XDocument.Parse. Se convierten a su caracter
            // literal antes de parsear. No se tocan las 5 entidades predefinidas de XML
            // (&amp; &lt; &gt; &quot; &apos;) ni las numericas (&#39;), que ya son validas.
            var entidades = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                // Acentos y caracteres del español
                { "&aacute;", "á" }, { "&eacute;", "é" }, { "&iacute;", "í" },
                { "&oacute;", "ó" }, { "&uacute;", "ú" }, { "&ntilde;", "ñ" },
                { "&Aacute;", "Á" }, { "&Eacute;", "É" }, { "&Iacute;", "Í" },
                { "&Oacute;", "Ó" }, { "&Uacute;", "Ú" }, { "&Ntilde;", "Ñ" },
                { "&uuml;", "ü" }, { "&Uuml;", "Ü" }, { "&iquest;", "¿" }, { "&iexcl;", "¡" },
                { "&ordm;", "º" }, { "&ordf;", "ª" },
                // Otras vocales latinas
                { "&agrave;", "à" }, { "&egrave;", "è" }, { "&igrave;", "ì" },
                { "&ograve;", "ò" }, { "&ugrave;", "ù" },
                { "&Agrave;", "À" }, { "&Egrave;", "È" }, { "&Igrave;", "Ì" },
                { "&Ograve;", "Ò" }, { "&Ugrave;", "Ù" },
                { "&acirc;", "â" }, { "&ecirc;", "ê" }, { "&icirc;", "î" },
                { "&ocirc;", "ô" }, { "&ucirc;", "û" },
                { "&atilde;", "ã" }, { "&otilde;", "õ" },
                { "&ccedil;", "ç" }, { "&Ccedil;", "Ç" },
                // Espacios, puntuación y símbolos frecuentes en FullContent/SmallContent
                { "&nbsp;", " " }, { "&bull;", "•" }, { "&middot;", "·" },
                { "&mdash;", "—" }, { "&ndash;", "–" }, { "&hellip;", "…" },
                { "&ldquo;", "\"" }, { "&rdquo;", "\"" }, { "&lsquo;", "'" }, { "&rsquo;", "'" },
                { "&laquo;", "«" }, { "&raquo;", "»" },
                { "&euro;", "€" }, { "&pound;", "£" }, { "&copy;", "©" }, { "&reg;", "®" }, { "&trade;", "™" },
                { "&deg;", "°" }, { "&frac12;", "½" }, { "&frac14;", "¼" }, { "&times;", "×" }, { "&divide;", "÷" },
                // Marcas de dirección que no aportan contenido visible
                { "&lrm;", "" }, { "&rlm;", "" }, { "&shy;", "" },
            };

            var resultado = xml;

            foreach (var entidad in entidades)
                resultado = resultado.Replace(entidad.Key, entidad.Value);

            return resultado;
        }

        private string ObjetoPeticionPreReserva(IntegracionDTO credencial, TarifaDTO tarifa)
        {
            var idioma = ObtenerIdioma(credencial);

            var rq = new BookRq();

            rq.Body.DestServicesBookV2.ObjCredentials.Source.RequestorID.ID = credencial.Usuario;
            rq.Body.DestServicesBookV2.ObjCredentials.Source.RequestorID.MessagePassword = credencial.Clave;

            var objRequest = rq.Body.DestServicesBookV2.ObjRequest;

            objRequest.PrimaryLangID = idioma;

            // CodigoAuxiliar = "EchoToken|ProductCode|ConceptCode".
            var auxiliares = tarifa.DatosTarifa?.CodigoAuxiliar?.Split('|') ?? Array.Empty<string>();

            objRequest.EchoToken = auxiliares.ElementAtOrDefault(0) ?? string.Empty;

            if (int.TryParse(tarifa.DatosTarifa?.CodigoProducto, out var productBookingCode))
            {
                objRequest.ProductBookingCode = productBookingCode;
                objRequest.ProductBookingCodeSpecified = true;
            }

            // ConceptBookingCode = codigo del concepto devuelto en DestServicesAvailV2,
            // que llega en el nodo ConceptCode (no el ConceptBookingCode slash-separado).
            var concepto = new BookRequestV2Concept
            {
                ConceptBookingCode = auxiliares.ElementAtOrDefault(2) ?? string.Empty,
                Quantity = 1,
                QuantitySpecified = true
            };

            if (int.TryParse(auxiliares.ElementAtOrDefault(1), out var productCode))
            {
                concepto.ProductCode = productCode;
                concepto.ProductCodeSpecified = true;
            }

            if (int.TryParse(auxiliares.ElementAtOrDefault(2), out var conceptCode))
            {
                concepto.ConceptCode = conceptCode;
                concepto.ConceptCodeSpecified = true;
            }

            // Detalle opcional (DetailBookingCode) si el proveedor lo devolvio en la busqueda.
            if (!string.IsNullOrWhiteSpace(tarifa.DatosTarifa?.Localizador))
            {
                concepto.OptionalDetails = new List<OptionalDetail>
                {
                    new OptionalDetail { DetailBookingCode = tarifa.DatosTarifa.Localizador, Quantity = 1 }
                };
            }

            objRequest.Concepts.Add(concepto);

            return rq.SerializarXml();
        }

        private CommitRq ObjetoPeticionReserva(ReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, DisponibilidadDTO disponibilidad, TarifaDTO tarifa)
        {
            var idioma = ObtenerIdioma(credencial);

            var rq = new CommitRq();

            rq.Body.DestServicesCommitV2.ObjCredentials.Source.RequestorID.ID = credencial.Usuario;
            rq.Body.DestServicesCommitV2.ObjCredentials.Source.RequestorID.MessagePassword = credencial.Clave;

            var objRequest = rq.Body.DestServicesCommitV2.ObjRequest;

            objRequest.PrimaryLangID = idioma;
            objRequest.EchoToken = disponibilidad.EchoTokenReserva ?? string.Empty;
            objRequest.TransactionIdentifier = disponibilidad.LocalizadorReserva ?? string.Empty;
            objRequest.ClientReference = busqueda.Referencia;

            // ConceptBookingCode = valor devuelto por BOOK (Products>BookResponseV2Concept>ConceptBookingCode).
            // No usar tarifa.Id (ConceptBookingCode slash-separado de SEARCH): el proveedor lo rechaza.
            var concepto = new CommitRequestV2Concept
            {
                ConceptBookingCode = tarifa.DatosTarifa?.ConceptoReserva ?? string.Empty,
                Comments = peticion.Observaciones
            };

            var esPrimerPasajero = true;

            foreach (var pasajero in peticion.Pasajeros)
            {
                DateTime.TryParse(pasajero.FechaNacimiento, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaNacimiento);

                var guest = new CommitGuest
                {
                    GivenName = pasajero.Nombres,
                    Surname = pasajero.Apellidos,
                    BirthDate = fechaNacimiento != default ? fechaNacimiento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : pasajero.FechaNacimiento,
                    Age = fechaNacimiento != default ? CalcularEdad(fechaNacimiento) : 0,
                    DocumentID = string.IsNullOrWhiteSpace(pasajero.Documento) ? null : pasajero.Documento,
                    Gender = string.IsNullOrWhiteSpace(pasajero.IdSexo) ? null : pasajero.IdSexo,
                    IsChild = !pasajero.Tipo.Equals(TipoAdulto, StringComparison.OrdinalIgnoreCase)
                };

                // Datos de contacto en el primer pasajero.
                if (esPrimerPasajero)
                {
                    guest.PhoneNumber = peticion.Telefono;
                    guest.Email = peticion.Email;
                    esPrimerPasajero = false;
                }

                concepto.Guests.Add(guest);
            }

            // Telefono de contacto del transfer (solo si el request lo trae).
            if (!string.IsNullOrWhiteSpace(peticion.Telefono))
            {
                concepto.Transfer = new CommitTransfer { ContactPhone = peticion.Telefono };
            }

            objRequest.Concepts.Add(concepto);

            return rq;
        }

        private static int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;

            if (fechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;

            return edad;
        }

        private static IEnumerable<XElement> ExtraerConceptosReserva(XElement result)
        {
            return result.Elements()
                .FirstOrDefault(e => e.Name.LocalName == NodoProducts)?
                .Elements().Where(e => e.Name.LocalName == NodoBookResponseV2Product)
                .SelectMany(p => p.Elements().FirstOrDefault(e => e.Name.LocalName == NodoConcepts)?
                    .Elements().Where(e => e.Name.LocalName == NodoBookResponseV2Concept) ?? Enumerable.Empty<XElement>())
                ?? Enumerable.Empty<XElement>();
        }

        private static PrecioReservaDTO? CalcularPrecioReserva(IEnumerable<XElement> conceptos, string nombreIntegrador)
        {
            decimal precioNeto = 0m;
            decimal precioVenta = 0m;
            var moneda = MonedaPorDefecto;
            var hayPrecio = false;

            foreach (var concepto in conceptos)
            {
                var taxes = ObtenerTaxesConcepto(concepto);

                if (taxes?.Attribute(AtributoAmount) == null)
                    continue;

                hayPrecio = true;

                var importe = DecimalAtributo(taxes, AtributoAmount) ?? 0m;
                var comisiones = taxes.Elements().FirstOrDefault(e => e.Name.LocalName == NodoCommissions);

                var venta = DecimalElemento(comisiones, NodoSellingPrice) ?? importe;
                var neto = DecimalElemento(comisiones, NodoNetAmount) ?? venta;

                moneda = taxes.Attribute(AtributoCurrencyCode)?.Value ?? moneda;
                precioVenta += venta;
                precioNeto += neto;
            }

            if (!hayPrecio)
                return null;

            var total = new TotalReservaDTO(moneda, precioNeto, precioVenta - precioNeto, precioVenta);

            return new PrecioReservaDTO(
                nombreIntegrador,
                total,
                new List<PTCTotalResevaDTO> { new PTCTotalResevaDTO(TipoAdulto, 1, total) });
        }

        private (string Localizador, PrecioReservaDTO? Precio) ProcesarRespuestaReserva(string xmlRespuesta, Busqueda peticion, IntegracionDTO credencial)
        {
            XDocument doc;

            try
            {
                doc = XDocument.Parse(SanearEntidadesHtml(xmlRespuesta));
            }
            catch
            {
                return (string.Empty, null);
            }

            var faultString = ObtenerFaultString(doc);

            if (faultString != null)
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "Reserva", credencial.codigo, faultString);
                return (string.Empty, null);
            }

            var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "DestServicesCommitV2Result");

            if (result == null)
                return (string.Empty, null);

            var estado = result.Attribute(AtributoResResponseType)?.Value ?? string.Empty;

            if (estado.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "Reserva", credencial.codigo, $"INeedTours rechazo el commit: {estado}");
                return (string.Empty, null);
            }

            // El proveedor puede responder "Pending" con un nodo <Errors>; se registra el motivo real.
            var erroresProveedor = ObtenerErroresProveedor(result);
            if (erroresProveedor.Count > 0)
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "Reserva", credencial.codigo,
                    $"INeedTours: el proveedor rechazo el commit. {string.Join(" | ", erroresProveedor)}");
                return (string.Empty, null);
            }

            // Pendiente de pruebas del flujo completo: evaluar devolver BookingItemIdentifier
            // (identificador del elemento a cancelar) como localizador en lugar del locator global.
            //var primerProducto = result.Elements()
            //    .FirstOrDefault(e => e.Name.LocalName == "Products")?
            //    .Elements().FirstOrDefault(e => e.Name.LocalName == "BookResponseV2Product");
            //
            //var bookingItem = primerProducto?.Elements()
            //    .FirstOrDefault(e => e.Name.LocalName == "BookingItemIdentifier")?.Value;
            //
            //var localizador = string.IsNullOrEmpty(bookingItem)
            //    ? result.Attribute("TransactionIdentifier")?.Value ?? string.Empty
            //    : bookingItem;

            var localizador = result.Attribute(AtributoTransactionIdentifier)?.Value ?? string.Empty;

            // Precio final: suma de los conceptos de todos los productos
            // (Total > Taxes a nivel de concepto, misma estructura que BOOK-RS).
            var precio = CalcularPrecioReserva(ExtraerConceptosReserva(result), credencial.nombre);

            return (localizador, precio);
        }

        private static (decimal Importe, string Moneda) ExtraerPenalidad(XElement result)
        {
            var cancellationFees = result.Elements().FirstOrDefault(e => e.Name.LocalName == NodoCancellationFees);
            var importe = DecimalAtributo(cancellationFees, AtributoAmount) ?? 0m;
            var moneda = cancellationFees?.Attribute(AtributoCurrencyCode)?.Value ?? MonedaPorDefecto;

            return (importe, moneda);
        }

        private CancelacionResponseDTO ProcesarRespuestaCancelacion(string xmlRespuesta, CancelacionDTO peticion, IntegracionDTO credencial)
        {
            XDocument doc;

            try
            {
                doc = XDocument.Parse(SanearEntidadesHtml(xmlRespuesta));
            }
            catch
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, "Cancelacion", credencial.codigo, "Respuesta SOAP de cancelacion no interpretable.");
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "Respuesta del proveedor no interpretable.");
            }

            var faultString = ObtenerFaultString(doc);

            if (faultString != null)
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, "Cancelacion", credencial.codigo, faultString);
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, faultString);
            }

            var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "DestServicesCancelV2Result");

            if (result == null)
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, "Cancelacion", credencial.codigo, "La respuesta de cancelacion no contiene DestServicesCancelV2Result.");
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, peticion.Localizador, "La respuesta de cancelacion no contiene el resultado esperado.");
            }

            var estado = result.Attribute(AtributoResResponseType)?.Value ?? string.Empty;
            var localizador = result.Attribute(AtributoTransactionIdentifier)?.Value ?? peticion.Localizador;

            if (estado.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, "Cancelacion", credencial.codigo, $"INeedTours rechazo la cancelacion: {estado}");
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, localizador, "El proveedor rechazo la cancelacion de la reserva.");
            }

            if (estado.Equals("Commited", StringComparison.OrdinalIgnoreCase))
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, "Cancelacion", credencial.codigo, $"La reserva sigue confirmada tras la cancelacion: {estado}");
                return new CancelacionResponseDTO(Estado.ERROR, peticion.Source, localizador, "La reserva sigue confirmada; no fue posible cancelarla.");
            }

            var (importe, moneda) = ExtraerPenalidad(result);

            if (estado.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return new CancelacionResponseDTO(Estado.PENDIENTE, peticion.Source, localizador, "La cancelacion quedo pendiente de confirmacion por el proveedor.");

            // Cancelled (o sin estado): la cancelacion se aplico.
            var mensaje = importe > 0m
                ? $"Reserva cancelada correctamente. Penalidad: {importe.ToString("0.##", CultureInfo.InvariantCulture)} {moneda}."
                : "Reserva cancelada correctamente.";

            return new CancelacionResponseDTO(Estado.OK, peticion.Source, localizador, mensaje);
        }

        private DisponibilidadDTO? ProcesarRespuestaPreReserva(string xmlRespuesta, DisponibilidadDTO disponibilidadSeleccionada, TarifaDTO tarifaSeleccionada, Busqueda peticion, IntegracionDTO credencial)
        {
            XDocument doc;

            try
            {
                doc = XDocument.Parse(SanearEntidadesHtml(xmlRespuesta));
            }
            catch
            {
                return null;
            }

            var faultString = ObtenerFaultString(doc);
            if (faultString != null)
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "PreReserva", credencial.codigo, faultString);
                return null;
            }
            var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "DestServicesBookV2Result");
            if (result == null)
                return null;

            var localizadorReserva = result.Attribute(AtributoTransactionIdentifier)?.Value ?? string.Empty;
            var estadoReserva = result.Attribute(AtributoResResponseType)?.Value ?? string.Empty;
            // Identificadores requeridos por DestServicesCommitV2 (confirmacion).
            var echoTokenReserva = result.Attribute(AtributoEchoToken)?.Value ?? string.Empty;
            if (estadoReserva.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "PreReserva", credencial.codigo, $"INeedTours rechazo la reserva: {estadoReserva}");
                return null;
            }

            // El proveedor responde "Pending" con <Errors> cuando rechaza el BOOK (ej. ConceptBookingCode invalido).
            var erroresProveedor = ObtenerErroresProveedor(result);
            if (erroresProveedor.Count > 0)
            {
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "PreReserva", credencial.codigo,
                    $"INeedTours: el proveedor rechazo la pre-reserva. {string.Join(" | ", erroresProveedor)}");
                return null;
            }

            var conceptos = ExtraerConceptosReserva(result).ToList();
            var concepto = conceptos.FirstOrDefault();

            var precio = ExtraerPrecioReserva(conceptos);

            if (precio?.Total?.Total == 0m)
            {
                var detalle = DiagnosticoPrecio(result, conceptos);
                _logger.ErrorLog(peticion.Referencia, peticion.IdBusqueda, "PreReserva", credencial.codigo,
                    $"INeedTours: el precio de la pre-reserva llego en 0. {detalle}");
            }

            var penalidades = ExtraerPenalidadesReserva(result);

            var duracion = Valor(concepto, "Duration");
            var datosTarifaOriginal = tarifaSeleccionada.DatosTarifa ?? new DatosTarifaDTO();
            var datosTarifaActualizado = new DatosTarifaDTO
            {
                Localizador = datosTarifaOriginal.Localizador,
                CodigoProducto = datosTarifaOriginal.CodigoProducto,
                CodigoAuxiliar = datosTarifaOriginal.CodigoAuxiliar,
                TipoVehiculo = datosTarifaOriginal.TipoVehiculo,
                ModeloVehiculo = datosTarifaOriginal.ModeloVehiculo,
                ImagenVehiculo = datosTarifaOriginal.ImagenVehiculo,
                Flexible = datosTarifaOriginal.Flexible,
                DuracionViaje = duracion,
                HoraInicio = datosTarifaOriginal.HoraInicio,
                EsperaMaxima = "10 minutos",
                CantidadPasajeros = tarifaSeleccionada.CantidadAdultos + tarifaSeleccionada.CantidadNinos + tarifaSeleccionada.CantidadInfantes,
                MaletasMaximas = datosTarifaOriginal.MaletasMaximas,
                PoliticasCancelacion = penalidades.Count > 0 ? penalidades : null,
                ConceptoReserva = Valor(concepto, NodoConceptBookingCode)
            };

            var tarifaActualizada = new TarifaDTO
            {
                Id = tarifaSeleccionada.Id,
                IdPreReserva = tarifaSeleccionada.IdPreReserva,
                Category = tarifaSeleccionada.Category,
                Nombre = tarifaSeleccionada.Nombre,
                Beneficios = tarifaSeleccionada.Beneficios,
                RangoEdad = tarifaSeleccionada.RangoEdad,
                CantidadAdultos = tarifaSeleccionada.CantidadAdultos,
                CantidadNinos = tarifaSeleccionada.CantidadNinos,
                CantidadInfantes = tarifaSeleccionada.CantidadInfantes,
                DatosTarifa = datosTarifaActualizado
            };
            return new DisponibilidadDTO
            {
                Source = disponibilidadSeleccionada.Source,
                SourceName = disponibilidadSeleccionada.SourceName,
                Id = disponibilidadSeleccionada.Id,
                Auxiliares = disponibilidadSeleccionada.Auxiliares,
                Plan = disponibilidadSeleccionada.Plan,
                Duracion = disponibilidadSeleccionada.Duracion,
                Tipo = disponibilidadSeleccionada.Tipo,
                FechaInicio = disponibilidadSeleccionada.FechaInicio,
                FechaSalida = disponibilidadSeleccionada.FechaSalida,
                Imagen = disponibilidadSeleccionada.Imagen,
                Precio = precio,
                Tarifas = new List<TarifaDTO> { tarifaActualizada },
                LocalizadorReserva = localizadorReserva,
                EstadoReserva = estadoReserva,
                EchoTokenReserva = echoTokenReserva
            };
        }

        private static PrecioDTO ExtraerPrecioReserva(IEnumerable<XElement> conceptos)
        {
            decimal precioNeto = 0m;
            decimal precioVenta = 0m;
            var moneda = MonedaPorDefecto;
            var hayPrecio = false;

            foreach (var concepto in conceptos)
            {
                var taxes = ObtenerTaxesConcepto(concepto);
                if (taxes?.Attribute(AtributoAmount) == null)
                    continue;

                hayPrecio = true;

                var importe = DecimalAtributo(taxes, AtributoAmount) ?? 0m;
                var comisiones = taxes.Elements().FirstOrDefault(e => e.Name.LocalName == NodoCommissions);
                var venta = DecimalElemento(comisiones, NodoSellingPrice) ?? importe;
                var neto = DecimalElemento(comisiones, NodoNetAmount) ?? venta;

                moneda = taxes.Attribute(AtributoCurrencyCode)?.Value ?? moneda;
                precioVenta += venta;
                precioNeto += neto;
            }

            if (!hayPrecio)
            {
                var totalVacio = new TotalDTO(moneda, 0m, 0m, 0m);
                return new PrecioDTO(totalVacio, new List<PTCTotalDTO> { new PTCTotalDTO(TipoAdulto, 1, totalVacio) });
            }

            var total = new TotalDTO(moneda, precioNeto, precioVenta - precioNeto, precioVenta);
            return new PrecioDTO(total, new List<PTCTotalDTO> { new PTCTotalDTO(TipoAdulto, 1, total) });
        }

        private static XElement? ObtenerTaxesConcepto(XElement? concepto)
        {
            // BOOK/COMMIT: Total > Taxes a nivel de concepto. Fallback al detalle
            // (BookResponseV2Detail > Total > Taxes) por si el concepto no trae Total.
            var totalConcepto = concepto?.Elements().FirstOrDefault(e => e.Name.LocalName == NodoTotal);
            var taxesConcepto = totalConcepto?.Elements().FirstOrDefault(e => e.Name.LocalName == NodoTaxes);

            if (taxesConcepto != null)
                return taxesConcepto;

            return concepto?.Elements().FirstOrDefault(e => e.Name.LocalName == NodoDetails)?
                .Elements().FirstOrDefault(e => e.Name.LocalName == NodoBookResponseV2Detail)?
                .Elements().FirstOrDefault(e => e.Name.LocalName == NodoTotal)?
                .Elements().FirstOrDefault(e => e.Name.LocalName == NodoTaxes);
        }

        private static List<string> ObtenerErroresProveedor(XElement result)
        {
            // <Errors><Error Type="N"><Code>...</Code><Description>...</Description></Error></Errors>
            var errores = new List<string>();
            var nodoErrors = result.Elements().FirstOrDefault(e => e.Name.LocalName == "Errors");

            if (nodoErrors == null)
                return errores;

            foreach (var error in nodoErrors.Elements().Where(e => e.Name.LocalName == "Error"))
            {
                var codigo = error.Elements().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value?.Trim();
                var descripcion = error.Elements().FirstOrDefault(e => e.Name.LocalName == "Description")?.Value?.Trim();

                if (string.IsNullOrWhiteSpace(codigo) && string.IsNullOrWhiteSpace(descripcion))
                    continue;

                errores.Add(!string.IsNullOrWhiteSpace(codigo) && !string.IsNullOrWhiteSpace(descripcion)
                    ? $"{codigo}: {descripcion}"
                    : codigo ?? descripcion ?? string.Empty);
            }

            return errores;
        }

        private static string DiagnosticoPrecio(XElement result, List<XElement> conceptos)
        {
            // Vuelca la estructura de precios real para diagnosticar por que llega en 0.
            var partes = new List<string> { $"Concepts={conceptos.Count}" };

            for (var i = 0; i < conceptos.Count; i++)
            {
                var c = conceptos[i];
                var totalConcepto = c.Elements().FirstOrDefault(e => e.Name.LocalName == NodoTotal);
                var taxesConcepto = totalConcepto?.Elements().FirstOrDefault(e => e.Name.LocalName == NodoTaxes);
                var detail = c.Elements().FirstOrDefault(e => e.Name.LocalName == NodoDetails)?
                    .Elements().FirstOrDefault(e => e.Name.LocalName == NodoBookResponseV2Detail);
                var taxesDetail = detail?.Elements().FirstOrDefault(e => e.Name.LocalName == NodoTotal)?
                    .Elements().FirstOrDefault(e => e.Name.LocalName == NodoTaxes);

                partes.Add($"C{i}=Concept:{taxesConcepto?.Attribute(AtributoAmount)?.Value ?? "null"}/{taxesConcepto?.Attribute(AtributoCurrencyCode)?.Value ?? "-"} Detail:{taxesDetail?.Attribute(AtributoAmount)?.Value ?? "null"}/{taxesDetail?.Attribute(AtributoCurrencyCode)?.Value ?? "-"}");
            }

            var xml = result.ToString(SaveOptions.DisableFormatting);
            if (xml.Length > 1500)
                xml = xml.Substring(0, 1500);

            return $"{string.Join(" | ", partes)} || XML={xml}";
        }


        private static List<PoliticaCancelacionDTO> ExtraerPenalidadesReserva(XElement result)
        {
            // Penalidades de cancelacion del producto (una por tramo/auto).
            var penalidades = new List<PoliticaCancelacionDTO>();
            var productos = result.Elements()
                .FirstOrDefault(e => e.Name.LocalName == NodoProducts)?
                .Elements().Where(e => e.Name.LocalName == NodoBookResponseV2Product)
                ?? Enumerable.Empty<XElement>();

            foreach (var producto in productos)
            {
                var descripcion = Valor(producto, "CancelPenaltiesDescription");

                foreach (var penalty in producto.Elements().FirstOrDefault(e => e.Name.LocalName == "CancelPenalties")?
                    .Elements().Where(e => e.Name.LocalName == "CancelPenalty") ?? Enumerable.Empty<XElement>())
                {
                    var amountPercent = penalty.Elements().FirstOrDefault(e => e.Name.LocalName == "AmountPercent");
                    penalidades.Add(new PoliticaCancelacionDTO(
                        penalty.Attribute("Start")?.Value,
                        penalty.Attribute("End")?.Value,
                        IntAtributo(amountPercent, "Percent"),
                        IntAtributo(amountPercent, "NmbrOfNights"),
                        DecimalAtributo(amountPercent, AtributoAmount),
                        amountPercent?.Attribute(AtributoCurrencyCode)?.Value,
                        descripcion,
                        false));
                }
            }

            return penalidades;
        }

        private static int? IntAtributo(XElement? elemento, string nombreAtributo)
        {
            var valor = elemento?.Attribute(nombreAtributo)?.Value;

            return int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var resultado) ? resultado : null;
        }

        private PoliciesRq ObjetoPeticionPolicies(CancelacionDTO peticion, IntegracionDTO credencial)
        {
            var idioma = ObtenerIdioma(credencial);

            var rq = new PoliciesRq();

            rq.Body.DestServicesCancellationFeesV2.ObjCredentials.Source.RequestorID.ID = credencial.Usuario;
            rq.Body.DestServicesCancellationFeesV2.ObjCredentials.Source.RequestorID.MessagePassword = credencial.Clave;

            var objRequest = rq.Body.DestServicesCancellationFeesV2.ObjRequest;

            objRequest.PrimaryLangID = idioma;
            objRequest.TransactionIdentifier = peticion.Localizador;

            return rq;
        }

        private CancelacionResponseDTO? ProcesarRespuestaPolicies(string xmlRespuesta, CancelacionDTO peticion, IntegracionDTO credencial)
        {
            XDocument doc;

            try
            {
                doc = XDocument.Parse(SanearEntidadesHtml(xmlRespuesta));
            }
            catch
            {
                return null;
            }

            var faultString = ObtenerFaultString(doc);

            if (faultString != null)
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, "Policies", credencial.codigo, faultString);
                return null;
            }

            var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "DestServicesCancellationFeesV2Result");

            if (result == null)
                return null;

            var localizador = result.Attribute(AtributoTransactionIdentifier)?.Value ?? peticion.Localizador;
            var estadoReserva = result.Attribute(AtributoResResponseType)?.Value ?? string.Empty;

            if (estadoReserva.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                _logger.ErrorLog(peticion.Localizador, Guid.Empty, "Policies", credencial.codigo, $"INeedTours rechazo la peticion de politicas: {estadoReserva}");
                return null;
            }

            var (importe, moneda) = ExtraerPenalidad(result);

            var mensaje = $"Penalidad: {importe.ToString("0.##", CultureInfo.InvariantCulture)} {moneda}";

            return new CancelacionResponseDTO(Estado.OK, peticion.Source, localizador, mensaje);
        }

        #endregion
    }
}
