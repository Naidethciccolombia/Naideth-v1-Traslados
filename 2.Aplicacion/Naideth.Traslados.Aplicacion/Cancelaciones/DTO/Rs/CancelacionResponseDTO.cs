 
using Naideth.Traslados.Dominio.Estados; 

namespace Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs
{
    public sealed record CancelacionResponseDTO(Estado Estado, string? Source, string? Localizador,string? Mensaje);
   
}
