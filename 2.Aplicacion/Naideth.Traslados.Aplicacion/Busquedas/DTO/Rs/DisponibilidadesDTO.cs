using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
public sealed record DisponibilidadesDTO( string? Source,string? SourceName , List<DisponibilidadDTO>? Disponibilidades);

}
