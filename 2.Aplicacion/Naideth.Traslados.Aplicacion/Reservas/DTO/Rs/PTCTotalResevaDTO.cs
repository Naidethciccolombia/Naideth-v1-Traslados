using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using Naideth.Traslados.Dominio.Estados;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.Reservas.DTO.Rs
{
    public sealed record PTCTotalResevaDTO(string Ptc, int Cantidad, TotalReservaDTO Total);


}
