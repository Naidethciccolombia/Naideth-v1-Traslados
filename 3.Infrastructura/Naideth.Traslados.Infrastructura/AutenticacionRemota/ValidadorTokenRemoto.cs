using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Naideth.Traslados.Aplicacion.AutenticacionRemota.DTO;
using Naideth.Traslados.Aplicacion.AutenticacionRemota.Interfaces;

namespace Naideth.Traslados.Infrastructura.AutenticacionRemota;

public sealed class ValidadorTokenRemoto(HttpClient httpClient, IConfiguration configuration, ILogger<ValidadorTokenRemoto> logger) : IValidadorTokenRemoto
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ValidarTokenRemotoResponseDTO> ValidarTokenAsync(string tokenUsuario, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tokenUsuario))
        {
            return CrearInvalido("Token de usuario inválido: vacío.");
        }

        var endpoint = configuration["Autenticacion:IntrospeccionEndpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            logger.LogError("Configuración faltante: Autenticacion:IntrospeccionEndpoint");
            return CrearInvalido("Endpoint mal configurado: falta Autenticacion:IntrospeccionEndpoint.");
        }

        var maxIntentos = Math.Max(1, configuration.GetValue("Autenticacion:HttpClient:RetryCount", 3));
        var delayBaseMs = Math.Max(50, configuration.GetValue("Autenticacion:HttpClient:RetryDelayMs", 300));

        for (var intento = 1; intento <= maxIntentos; intento++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUsuario);

                var payload = new ValidarTokenRemotoRequestDTO { Token = tokenUsuario };
                var body = JsonSerializer.Serialize(payload, JsonOptions);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return MapearOk(content);
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var causa = InferirCausa401(content);
                    return CrearInvalido(causa);
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return CrearInvalido("403 por policy: acceso denegado por autorización remota.");
                }

                if ((int)response.StatusCode >= 500)
                {
                    if (intento < maxIntentos)
                    {
                        await Task.Delay(delayBaseMs * intento, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    var mensajeServidor = ExtraerMensaje(content);
                    return CrearInvalido($"Error remoto {(int)response.StatusCode}: {mensajeServidor}");
                }

                return CrearInvalido($"Respuesta no esperada {(int)response.StatusCode}: {ExtraerMensaje(content)}");
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && intento < maxIntentos)
            {
                await Task.Delay(delayBaseMs * intento, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex) when (intento < maxIntentos)
            {
                logger.LogWarning(ex, "Fallo HTTP en introspección, reintentando. Intento {Intento}", intento);
                await Task.Delay(delayBaseMs * intento, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado en validación remota de token.");
                return CrearInvalido("Error interno al validar token contra API Central.");
            }
        }

        return CrearInvalido("No fue posible validar token remoto tras reintentos.");
    }

    private static ValidarTokenRemotoResponseDTO MapearOk(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return CrearInvalido("Respuesta 200 sin contenido desde API Central.");
        }

        var response = JsonSerializer.Deserialize<ValidarTokenRemotoResponseDTO>(content, JsonOptions);
        if (response is null)
        {
            return CrearInvalido("Respuesta 200 con payload inválido desde API Central.");
        }

        return response with
        {
            Roles = response.Roles ?? Array.Empty<string>(),
            Tenants = response.Tenants ?? Array.Empty<string>(),
            Mensaje = string.IsNullOrWhiteSpace(response.Mensaje)
                ? (response.EsValido ? "Token válido." : "Token de usuario inválido.")
                : response.Mensaje
        };
    }

    private static string InferirCausa401(string content)
    {
        var mensaje = ExtraerMensaje(content).ToLowerInvariant();

        if (mensaje.Contains("scope") || mensaje.Contains("client") || mensaje.Contains("technical") || mensaje.Contains("tecnico"))
        {
            return "Token técnico sin scope: API Central rechazó credenciales del cliente.";
        }

        if (mensaje.Contains("token") || mensaje.Contains("expir") || mensaje.Contains("firma") || mensaje.Contains("signature"))
        {
            return "Token de usuario inválido: expirado, malformado o con firma no válida.";
        }

        return "401 no autorizado: verificar token de usuario o token técnico.";
    }

    private static string ExtraerMensaje(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "Sin detalle de error.";
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (TryGet(root, "mensaje", out var mensaje) ||
                TryGet(root, "message", out mensaje) ||
                TryGet(root, "detail", out mensaje) ||
                TryGet(root, "title", out mensaje))
            {
                return mensaje;
            }
        }
        catch
        {
            // Ignorado: se devuelve contenido crudo resumido.
        }

        return content.Length > 250 ? content[..250] : content;
    }

    private static bool TryGet(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var prop))
        {
            return false;
        }

        value = prop.ValueKind == JsonValueKind.String ? prop.GetString() ?? string.Empty : prop.ToString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static ValidarTokenRemotoResponseDTO CrearInvalido(string mensaje) => new()
    {
        EsValido = false,
        IdUsuario = Guid.Empty,
        Roles = Array.Empty<string>(),
        Tenants = Array.Empty<string>(),
        Mensaje = mensaje
    };
}
