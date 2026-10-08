using Naideth.Traslados.Aplicacion.UsuariosRoles.DTO;
using Naideth.Central.Dominio.UsuariosRoles; 

namespace Naideth.Traslados.Aplicacion.UsuariosRoles.Interfaces
{
    public interface IUsuarioRolesRepositorio
    { 
        Task<QueryUsuarioRolesDTO> GetRoles(Guid idUsuario, CancellationToken TokenCancelacion);
     

    }
}
