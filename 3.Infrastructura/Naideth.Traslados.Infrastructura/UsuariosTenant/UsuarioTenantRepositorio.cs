using Microsoft.EntityFrameworkCore; 
using Naideth.Traslados.Aplicacion.UsuariosTenant.DTO;
using Naideth.Traslados.Aplicacion.UsuariosTenant.Interface;
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces;

namespace Naideth.Traslados.Infrastructura.UsuariosTenant
{
    internal class UsuarioTenantRepositorio:IUsuarioTenantRepositorio
    {

        private readonly IApplicationDbContext _context; 

        public UsuarioTenantRepositorio(IApplicationDbContext context)
        {
            _context = context; 
        }
         

        public async Task<QueryUsuarioTenantDTO> GetTenants(Guid idUsuario, CancellationToken TokenCancelacion)
        {  
            var tenants = await _context.UsuarioTenant
            .Where(rp => rp.IdUsuario == idUsuario)
            .Select(rp => rp.IdTenant).ToListAsync(TokenCancelacion)
            .ConfigureAwait(false);

            var tenantTercero = await _context.Tercero.FirstOrDefaultAsync(rp => rp.IdUsuario == idUsuario,TokenCancelacion).ConfigureAwait(false);

            if (tenantTercero != null)
            {
                tenants.Add(tenantTercero.IdTenant);
            }


            return new QueryUsuarioTenantDTO(idUsuario, tenants);
        }
         

    }
}
