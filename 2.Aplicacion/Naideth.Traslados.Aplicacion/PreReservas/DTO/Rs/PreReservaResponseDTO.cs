using Naideth.Central.Dominio.Amenidades;
using Naideth.Central.Dominio.Hoteles;
using Naideth.Traslados.Dominio.Estados;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;

namespace Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs
{ 
   public sealed record PreReservaResponseDTO(Estado? Estado, DisponibilidadesDTO? Data);
     
}
