using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public sealed record PaxRs(int Adultos, int Nino, List<string> EdadNinos, List<string> EdadAdultos);
    
}
