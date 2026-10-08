using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Naideth.Central.Dominio.Usuarios.Excepciones; 
using Naideth.Central.Dominio.Kernel.Extensiones;
using Naideth.Central.Dominio.Autenticacion; 
using Newtonsoft.Json; 
using Naideth.Central.Aplicacion.Usuarios.DTO; 
using Naideth.Central.Dominio.Usuarios; 
using Naideth.Traslados.Aplicacion.Autenticacion.DTO;
using Naideth.Traslados.Aplicacion.Datetime.interfaces;
using Naideth.Traslados.Aplicacion.Usuarios.Interfaces;
using Naideth.Traslados.Aplicacion.UsuariosRoles.Interfaces;
using Naideth.Traslados.Aplicacion.UsuariosTenant.Interface;
using Naideth.Traslados.Aplicacion.Autenticacion.Interfaces;

namespace Naideth.Traslados.Aplicacion.Autenticacion
{
    public sealed  class AutenticacionServicio: IAutenticacionServicio
    {

        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IUsuarioRolesRepositorio _usuarioRolesRepositorio;
        private readonly IUsuarioTenantRepositorio _usuarioTenantRepositorio;
        private readonly IConfiguration _configuration;
        private readonly IDateTimeProvider _dateTimeProvider;
         
        public AutenticacionServicio(IUsuarioRepositorio usuarioRepositorio, IConfiguration configuration, IDateTimeProvider dateTimeProvider, IUsuarioRolesRepositorio usuarioRolesRepositorio,  IUsuarioTenantRepositorio usuarioTenantRepositorio)
         {
            _usuarioRepositorio = usuarioRepositorio;
            _configuration = configuration;
            _dateTimeProvider = dateTimeProvider;
            _usuarioRolesRepositorio = usuarioRolesRepositorio; 
            _usuarioTenantRepositorio= usuarioTenantRepositorio;

          }

        public async Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string token, CancellationToken tokenCancelacion)
        {

            tokenCancelacion.ThrowIfCancellationRequested();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]?.ToString())),
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token no válido");

            tokenCancelacion.ThrowIfCancellationRequested();

            return await Task.FromResult(principal);

        }


    }

}
