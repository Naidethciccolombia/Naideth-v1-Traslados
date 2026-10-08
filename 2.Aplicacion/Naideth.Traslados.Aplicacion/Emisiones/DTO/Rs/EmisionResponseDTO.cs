 
using Naideth.Traslados.Dominio.Estados; 

namespace Naideth.Traslados.Aplicacion.Emisiones.DTO.Rs
{
    public sealed record EmisionResponseDTO(Estado Estado, string? Source, string? Localizador,List<string>? Vouchers, string? Mensaje);
   
}
