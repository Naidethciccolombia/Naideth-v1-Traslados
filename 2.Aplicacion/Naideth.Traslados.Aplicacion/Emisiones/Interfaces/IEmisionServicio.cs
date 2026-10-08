using Naideth.Traslados.Aplicacion.Emisiones.DTO; 
using Naideth.Traslados.Aplicacion.Emisiones.DTO.Rs; 

namespace Naideth.Traslados.Aplicacion.Emisiones.Interfaces
{
    public interface IEmisionServicio   
    {
         Task<EmisionResponseDTO> EmisionAsync(EmisionDTO peticion, CancellationToken TokenCancelacion);
       
    }
}
