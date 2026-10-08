using Microsoft.EntityFrameworkCore; 
using Naideth.Central.Dominio.MenusRoles;
using Naideth.Central.Dominio.Roles;
using Naideth.Central.Dominio.RolesPermisos;
using Naideth.Central.Dominio.Usuarios;
using Naideth.Central.Dominio.UsuariosRoles;
using Naideth.Traslados.Aplicacion.UsuariosRoles.DTO;
using Naideth.Traslados.Aplicacion.UsuariosRoles.Interfaces;
using Naideth.Traslados.Infrastructura.AccesoDatos;
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces;
using System.Data;
using Naideth.Traslados.Aplicacion.Roles.DTO;

namespace Naideth.Traslados.Infrastructura.UsuariosRoles
{
    internal class UsuarioRolesRepositorio:IUsuarioRolesRepositorio
    {

        private readonly IApplicationDbContext _context; 

        public UsuarioRolesRepositorio(IApplicationDbContext context)
        {
            _context = context; 
        }
    
        public async Task<QueryUsuarioRolesDTO> GetRoles(Guid idUsuario, CancellationToken TokenCancelacion)
        {
              
            var roles = await _context.UsuarioRoles
                       .Where(ur => ur.IdUsuario == idUsuario)
                       .Join(_context.Rol,
                             ur => ur.IdRol,
                             r => r.IdRol,
                             (ur, r) => new { ur, r })
                       .Where(joined => joined.r.Estado)
                       .Select(joined=>new QueryRolDTO(joined.ur.IdRol, joined.r.Nombre,joined.r.Estado))
                       .ToListAsync(TokenCancelacion)
                       .ConfigureAwait(false);


            return new QueryUsuarioRolesDTO(idUsuario, roles);


        }

         
         
    }
}
