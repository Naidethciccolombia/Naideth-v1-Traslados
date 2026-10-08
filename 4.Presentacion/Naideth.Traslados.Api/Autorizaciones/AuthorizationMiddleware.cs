 using Naideth.Traslados.Dominio.Kernel.Exepciones;
using System.Security.Claims;   
using Naideth.Traslados.Dominio.Kernel.Extensiones;
using Newtonsoft.Json;
using Naideth.Traslados.Aplicacion.Autorizaciones.DTO;
using Naideth.Traslados.Aplicacion.Autorizaciones.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Naideth.Traslados.Aplicacion.UsuariosTenant.Interface;
using Naideth.Traslados.Aplicacion.Autenticacion;
using Naideth.Traslados.Aplicacion.Autenticacion.Interfaces;
using Naideth.Traslados.Aplicacion.Usuarios.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Naideth.Traslados.Api.Autorizaciones 
{ 
    public class AuthorizationMiddleware:IMiddleware
    {  
        private readonly IConfiguration _configuration;
        private readonly IAutenticacionServicio _autenticacionServicio;
        public AuthorizationMiddleware(IConfiguration configuration, IAutenticacionServicio autorizacionServicio)
        {  
            _configuration = configuration;
            _autenticacionServicio = autorizacionServicio;
        } 

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var endpoint = context.GetEndpoint();
            var requiresAuth = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() != null; // Verificar si la ruta requiere autorización


            ClaimsPrincipal principal = null;

            if (requiresAuth && string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token no proporcionado.");
                return;
            }

            if (requiresAuth)
            {
                try
                {
                    var tokenCancelacion = context.RequestAborted;

                    // Validar y obtener los claims del token
                    principal = await _autenticacionServicio.GetPrincipalFromExpiredTokenAsync(token, tokenCancelacion).ConfigureAwait(false);
                    if (principal == null)
                    {
                        throw new SecurityTokenException("Token no válido.");
                    }

                    // Extraer el userId del token
                    var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var userId = userIdClaim.ToGuidOrDefault();

                    if (userId == Guid.Empty)
                    {
                        throw new SecurityTokenException("El token no contiene un identificador de usuario válido.");
                    }

                    if (!string.IsNullOrEmpty(userIdClaim))
                    {
                        context.Items["UserId"] = Guid.Parse(userIdClaim); // Guardar el ID del usuario en HttpContext
                    } 

                }
                catch (SecurityTokenException ex)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync($"Error de token: {ex.Message}");
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"Error interno del servidor: {ex.Message}");
                }
            }

            await next(context); // Continuar con el pipeline

        }

    }
}
