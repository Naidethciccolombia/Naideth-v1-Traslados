using Naideth.Central.Dominio.Amenidades;
using Naideth.Central.Dominio.Hoteles;
using Naideth.Traslados.Dominio.Estados;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{ 
   public sealed record BusquedaResponseDTO(Estado? Estado, DisponibilidadesDTO? Data);
     
}
