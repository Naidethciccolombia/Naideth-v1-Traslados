using Microsoft.AspNetCore.Authorization;
using Naideth.Traslados.Aplicacion.AutenticacionRemota.Interfaces; 
using System.Net.Http.Headers;
using System.Text.Json;

namespace Naideth.Traslados.Api.Autorizaciones;

public sealed class AuthorizationMiddleware(
    IConfiguration configuration,
    IValidadorTokenRemoto validadorTokenRemoto,
    IHttpClientFactory httpClientFactory,
    ILogger<AuthorizationMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;
        var requiereAutorizacion = endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null;

        if (allowAnonymous || !requiereAutorizacion)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var tokenUsuario = ExtraerBearerUsuario(context.Request.Headers.Authorization);
        if (string.IsNullOrWhiteSpace(tokenUsuario))
        {
            await Responder401Async(context, "token usuario inválido: no se recibió bearer.").ConfigureAwait(false);
            return;
        }

        var resultado = await validadorTokenRemoto
            .ValidarTokenAsync(tokenUsuario, context.RequestAborted)
            .ConfigureAwait(false);

        if (!resultado.EsValido)
        {
            await Responder401Async(context, resultado.Mensaje).ConfigureAwait(false);
            return;
        }

        context.Items["UserId"] = resultado.IdUsuario;
        context.Items["Roles"] = resultado.Roles;
        context.Items["Tenants"] = resultado.Tenants;

        await next(context).ConfigureAwait(false);
    }

    private static string ExtraerBearerUsuario(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return authorizationHeader["Bearer ".Length..].Trim();
    }

    private async Task<(string Token, string Error)> ObtenerTokenTecnicoAsync(CancellationToken cancellationToken)
    {
        var tokenEndpoint = configuration["Autenticacion:IntrospeccionEndpoint"];  
         
        if (string.IsNullOrWhiteSpace(tokenEndpoint)  )
        {
            return (string.Empty, "endpoint mal configurado: faltan datos de client credentials.");
        }

        var cliente = httpClientFactory.CreateClient("TokenTecnicoClient");
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = "client_credentials"
            })
        };

        try
        {
            using var response = await cliente.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return (string.Empty, "403 por policy: token técnico sin permisos suficientes.");
                }

                return (string.Empty, "token técnico sin scope o credenciales inválidas.");
            }

            using var doc = JsonDocument.Parse(content);
            if (!doc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
            {
                return (string.Empty, "token técnico sin scope: respuesta sin access_token.");
            }

            var accessToken = accessTokenElement.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return (string.Empty, "token técnico sin scope: access_token vacío.");
            }

            return (accessToken, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo token técnico por client credentials.");
            return (string.Empty, "endpoint mal configurado o no disponible para token técnico.");
        }
    }

    private static Task Responder401Async(HttpContext context, string causa)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = new AuthenticationHeaderValue("Bearer").ToString();
        return context.Response.WriteAsync($"401: {causa}");
    }
}
