using Naideth.Central.Dominio.Roles;
using Naideth.Traslados.Aplicacion.Roles.DTO;

namespace Naideth.Traslados.Aplicacion.UsuariosRoles.DTO
{
    public sealed record QueryUsuarioRolesDTO(Guid idUsuario, List<QueryRolDTO> ListaRoles);
 
}
