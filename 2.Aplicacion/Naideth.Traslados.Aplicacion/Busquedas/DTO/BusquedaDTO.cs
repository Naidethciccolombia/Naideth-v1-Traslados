
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq; 
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO
{
    public sealed record BusquedaDTO(string moneda, string referencia, List<PasajerosPeticionDTO> pasajeros, List<TrayectoDTO> trayectos, int pagina, int registrosPagina, FiltroRequestDTO? Filtros);
}
   


