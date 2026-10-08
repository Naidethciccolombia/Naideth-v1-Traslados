using Naideth.Central.Dominio.CiudadesIntegraciones;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.DTO;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
//using Naideth.Traslados.Aplicacion.ConvercionMoneda.DTO;

namespace Naideth.Traslados.Aplicacion.Comunes.Interfaces
{
    public interface IComunRepositorio
    {
        Task<List<IntegracionDTO>> CredencialesAsync(CancellationToken TokenCancelacion);
      Task<List<IntegracionDTO>> CredencialesCancelacionAsync(CancellationToken TokenCancelacion);
        Task<List<TasaCambioDTO>> ListaTRMAsync(CancellationToken TokenCancelacion);
        Task<bool> StartListaTRM(CancellationToken TokenCancelacion);
        Task<bool> CredencialesForCacheAsync(CancellationToken TokenCancelacion);
         

    }
}
