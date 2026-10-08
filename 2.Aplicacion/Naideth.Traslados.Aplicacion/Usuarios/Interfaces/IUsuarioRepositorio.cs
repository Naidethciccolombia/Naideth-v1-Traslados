 
using Naideth.Central.Dominio.Usuarios; 

namespace Naideth.Traslados.Aplicacion.Usuarios.Interfaces
{
    public interface IUsuarioRepositorio
    {   
        Task<Usuario> GetUsuarioAsync(string Usuario, CancellationToken TokenCancelacion);
        Task AddRefreshTokenAsync(Guid id, string referToken, CancellationToken TokenCancelacion);
         Task<bool> ValidateRefreshTokenAsync(Guid id, string referToken, CancellationToken TokenCancelacion);
               
    }
}
