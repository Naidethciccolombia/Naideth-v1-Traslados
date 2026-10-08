

using System.Text.Json.Serialization;
namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq
{
    public sealed record FiltroPrecioRequestDTO(decimal? minimo, decimal? maximo);


}
