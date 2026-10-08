

using System.Text.Json.Serialization;
namespace Naideth.Traslados.Aplicacion.Busquedas.DTO
{ 
    public sealed record FiltroOrdenamientoRequestDTO(string columna, bool ascendente);
 

}
