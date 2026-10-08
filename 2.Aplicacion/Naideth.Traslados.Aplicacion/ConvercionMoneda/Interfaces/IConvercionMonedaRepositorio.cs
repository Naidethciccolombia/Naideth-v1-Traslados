

using Naideth.Traslados.Aplicacion.ConvercionMoneda.DTO;

namespace Naideth.Traslados.Aplicacion.ConvercionMoneda.Interfaces
{
    public interface IConvercionMonedaRepositorio
    {
        Task<List<TasaCambioDTO>> ListTasaCambioAsync(CancellationToken TokenCancelacion);


    }
}
