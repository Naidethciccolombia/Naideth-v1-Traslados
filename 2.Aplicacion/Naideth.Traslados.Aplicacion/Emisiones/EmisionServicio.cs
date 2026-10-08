using Naideth.Traslados.Aplicacion.Emisiones.DTO;
using Naideth.Traslados.Aplicacion.Emisiones.DTO.Rs;
using Naideth.Traslados.Aplicacion.Emisiones.Interfaces;
using Naideth.Traslados.Dominio.Estados;

namespace Naideth.Traslados.Aplicacion.Emisiones
{
    public sealed class EmisionServicio : IEmisionServicio
    {
        public Task<EmisionResponseDTO> EmisionAsync(EmisionDTO peticion, CancellationToken TokenCancelacion)
        {
            return Task.FromResult(new EmisionResponseDTO(
               Estado.ERROR,
               peticion.Source,
               peticion.Localizador,
               null,
               "Emisión no disponible para la integración actual."));
        }
    }
}
