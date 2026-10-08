
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO
{
    public sealed record CacheDTO(List<DisponibilidadDTO> disponibilidades, Guid idBusqueda, bool esParcial, List<IntegradorStatusDTO>? integradoresStatus);
    
}
