using Naideth.Central.Dominio.Amenidades;
using Naideth.Central.Dominio.Hoteles;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public sealed record FiltroResponseDTO(FiltroPrecioResponseDTO precios,  List<FiltroGeneralResponseDTO> fuentes,  List<FiltroGeneralResponseDTO> nombres,  List<FiltroGeneralResponseDTO> ordenamientos);

     
}
