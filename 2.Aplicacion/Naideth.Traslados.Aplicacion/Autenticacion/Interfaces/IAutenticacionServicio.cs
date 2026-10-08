using Naideth.Central.Dominio.Autenticacion;
using Naideth.Traslados.Aplicacion.Autenticacion.DTO;
using System.Security.Claims;

namespace Naideth.Traslados.Aplicacion.Autenticacion.Interfaces
{
    public interface IAutenticacionServicio
    { 
        Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string token, CancellationToken TokenCancelacion);    
    }
}
