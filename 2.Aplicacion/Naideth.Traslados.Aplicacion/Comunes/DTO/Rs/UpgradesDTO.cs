using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Comunes.DTO.Rs
{
    public sealed record UpgradesDTO(string id, string nombre, string RangoEdad, decimal ValorUnitario,  decimal PrecioUnitario, List<PaxUpgradeDTO> PaxUpdate);  

}
