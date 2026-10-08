 
namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq
{ 
    public sealed record FiltroRequestDTO( FiltroPrecioRequestDTO? Precio, List<string>? Fuentes, decimal MarkUps, string? Ordenamiento, List<string>? Nombre); 
}
