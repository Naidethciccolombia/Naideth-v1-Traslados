using Naideth.Central.Dominio.TipoTarifasVuelos;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Comunes.DTO.Rs
{ 
    public sealed record PrecioDTO(TotalDTO Total, List<PTCTotalDTO> PtcTotal);
}
