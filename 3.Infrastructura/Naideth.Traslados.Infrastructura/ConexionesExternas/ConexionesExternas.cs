 
using Microsoft.Extensions.Logging;
using Naideth.Traslados.Aplicacion.ConexionesExternas.DTO;
using Naideth.Traslados.Aplicacion.ConexionesExternas.Interfaces;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Central.Dominio.Clientes; 
using Naideth.Traslados.Aplicacion.ConexionesExternas.DTO;
using Naideth.Traslados.Aplicacion.ConexionesExternas.Interfaces; 
using Naideth.Traslados.Aplicacion.Kernel; 
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net; 
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Naideth.Traslados.Infrastructura.ConexionesExternas
{
    public class ConexionesExternasRepositorio : IConexionesExternasRepositorio
    {
        private readonly ILogger<ConexionesExternasRepositorio> _logger;
        private readonly HttpClient _httpClient;


        public ConexionesExternasRepositorio(HttpClient httpClient, ILogger<ConexionesExternasRepositorio> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient.DefaultRequestHeaders.ConnectionClose = false; // Forzar Keep-Alive
        }
        public async Task<ResponseTravelKitStatusDTO> PeticionExternaTravelKitGetAsync(
   Guid IdBusqueda, string referencia, string codigo,
   Dictionary<string, string> headers, List<string> parameters, string Url, string verb,
   CancellationToken TokenCancelacion, int TimeOut, bool Logs)
        {
            var result = new ResponseTravelKitStatusDTO { Status = false };

            try
            {
                if (TokenCancelacion.IsCancellationRequested)
                {
                    result.Ex = new Exception("Cancelación solicitada antes de la ejecución.");
                    _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Cancelación solicitada antes de la ejecución.");
                    return result;
                }

                using var httpRequest = new HttpRequestMessage(new HttpMethod(verb), Url);
              
                foreach (var header in headers.Where(h => h.Key != "Content-Type"))
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, TokenCancelacion).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogCritical($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Se agotaron los reintentos tras recibir varios 429 Too Many Requests.");
                    result.Ex = new Exception("Se agotaron los intentos tras múltiples respuestas 429.");
                    return result;
                }

                string responseContent = await response.Content.ReadAsStringAsync(TokenCancelacion).ConfigureAwait(false);

                result.StatusCode = response.StatusCode;


                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    result.SetData(responseContent); // Guardamos una copia segura del JSON
                    responseContent = null;
                    result.Status = true;
                }

                if (!response.IsSuccessStatusCode)
                {

                    string error = string.Empty;
                    if (result.Data != null)
                    {
                        try
                        {
                            error = result.Data.errors[0].message.ToString();
                        }
                        catch
                        {
                            error = result.Data.error.message.ToString();
                        }
                    }
                    result.Ex = new Exception($"Error {response.StatusCode} {error}");

                    _logger.LogError($"Error enpeticion externa:{response.StatusCode} - {error}");
                }

            }
            catch (TaskCanceledException ex) when (!TokenCancelacion.IsCancellationRequested)
            {
                _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Timeout en petición externa: {ex.Message}");
                result.Ex = ex;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion: {codigo}, Error en petición externa {ex.Message} ");
                result.Ex = ex;
            }



            return result;
        }

        public async Task<ResponseTravelKitStatusDTO> PeticionExternaTravelKitAsync(
   Guid IdBusqueda, string referencia, string codigo, StringContent data,
   Dictionary<string, string> headers, List<string> parameters, string Url, string verb,
   CancellationToken TokenCancelacion, int TimeOut, bool Logs)
        {
            var result = new ResponseTravelKitStatusDTO { Status = false };

            try
            {
                if (TokenCancelacion.IsCancellationRequested)
                {
                    result.Ex = new Exception("Cancelación solicitada antes de la ejecución.");
                    _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Cancelación solicitada antes de la ejecución.");
                    return result;
                }

                using var httpRequest = new HttpRequestMessage(new HttpMethod(verb), Url);
                httpRequest.Content = data;

                foreach (var header in headers.Where(h => h.Key != "Content-Type"))
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, TokenCancelacion).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogCritical($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Se agotaron los reintentos tras recibir varios 429 Too Many Requests.");
                    result.Ex = new Exception("Se agotaron los intentos tras múltiples respuestas 429.");
                    return result;
                }

                string responseContent = await response.Content.ReadAsStringAsync(TokenCancelacion).ConfigureAwait(false);

                result.StatusCode = response.StatusCode;


                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    result.SetData(responseContent); // Guardamos una copia segura del JSON
                    responseContent = null;
                    result.Status = true;
                }

                if (!response.IsSuccessStatusCode)
                {

                    string error = string.Empty;
                    if (result.Data != null)
                    {
                        try
                        {
                            error = result.Data.errors[0].message.ToString();
                        }
                        catch
                        {
                            error = result.Data.error.message.ToString();
                        }
                    }
                    result.Ex = new Exception($"Error {response.StatusCode} {error}");

                    _logger.LogError($"Error enpeticion externa:{response.StatusCode} - {error}");
                }

            }
            catch (TaskCanceledException ex) when (!TokenCancelacion.IsCancellationRequested)
            {
                _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Timeout en petición externa: {ex.Message}");
                result.Ex = ex;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion: {codigo}, Error en petición externa {ex.Message} ");
                result.Ex = ex;
            }



            return result;
        }


        private string ObtenerError(JsonDocument body)
        {
            try
            {
                var arrayErrores = body.RootElement.GetJsonPropertyOrEmpty("error").EnumerateArray();
                //var body = JsonNode.Parse(responseContent);
                return arrayErrores.FirstOrDefault().GetSafeString("message") ?? "Error desconocido";
            }
            catch
            {
                return "Respuesta no válida";
            }
        }


        public async Task<ResponseTravelKitStatusDTO> PeticionExternaSoapAsync(
            Guid IdBusqueda, string referencia, string codigo, string dataXml,
            Dictionary<string, string> headers, string Url,
            CancellationToken TokenCancelacion, int TimeOut, bool Logs)
        {
            var result = new ResponseTravelKitStatusDTO { Status = false };

            try
            {
                if (TokenCancelacion.IsCancellationRequested)
                {
                    result.Ex = new Exception("Cancelación solicitada antes de la ejecución.");
                    _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Cancelación solicitada antes de la ejecución.");
                    return result;
                }

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, Url);
                httpRequest.Content = new StringContent(dataXml, Encoding.UTF8, "text/xml");

                foreach (var header in headers.Where(h => h.Key != "Content-Type"))
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, TokenCancelacion).ConfigureAwait(false);

                string responseContent = await response.Content.ReadAsStringAsync(TokenCancelacion).ConfigureAwait(false);

                result.StatusCode = response.StatusCode;

                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    result.SetData(responseContent);
                    responseContent = null;
                    result.Status = true;
                }

                if (!response.IsSuccessStatusCode)
                {
                    // SOAP devuelve faultstring en XML, no JSON: no se intenta parsear como JSON.
                    result.Ex = new Exception($"Error {response.StatusCode} en peticion SOAP: {result.Data}");
                    _logger.LogError($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Error en peticion SOAP: {response.StatusCode}");
                }

            }
            catch (TaskCanceledException ex) when (!TokenCancelacion.IsCancellationRequested)
            {
                _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion:{codigo}, Timeout en petición externa: {ex.Message}");
                result.Ex = ex;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"IdBusqueda: {IdBusqueda}, Integracion: {codigo}, Error en petición externa {ex.Message} ");
                result.Ex = ex;
            }

            return result;
        }



    }
}
