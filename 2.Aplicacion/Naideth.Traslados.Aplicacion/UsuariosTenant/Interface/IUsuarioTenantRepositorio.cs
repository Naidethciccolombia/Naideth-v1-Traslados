
using Naideth.Traslados.Aplicacion.UsuariosTenant.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.UsuariosTenant.Interface
{
    public interface IUsuarioTenantRepositorio
    {
         Task<QueryUsuarioTenantDTO> GetTenants(Guid idUsuario, CancellationToken TokenCancelacion);

    }
}
