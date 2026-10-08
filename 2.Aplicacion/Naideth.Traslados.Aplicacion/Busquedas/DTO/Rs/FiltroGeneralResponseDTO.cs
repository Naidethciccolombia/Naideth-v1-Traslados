using Naideth.Central.Dominio.Amenidades;
using Naideth.Central.Dominio.Hoteles;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
   public sealed record FiltroGeneralResponseDTO(string codigo, string nombre, int sel);
     
}
