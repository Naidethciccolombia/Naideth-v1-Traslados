using Naideth.Traslados.Aplicacion.Cancelaciones.DTO; 
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs; 

namespace Naideth.Traslados.Aplicacion.Cancelaciones.Interfaces
{
    public interface ICancelacionServicio
    {
         Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, CancellationToken TokenCancelacion);
       
    }
}
