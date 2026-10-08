
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{
    public class FiltroRequest
    {
        [JsonPropertyName("precio")]
        public FiltroPrecioRequest? Precio { get; set; }

        [JsonPropertyName("fuentes")]
        public List<string>? Fuentes { get; set; }

        [JsonPropertyName("markUps")]
        public required decimal MarkUps { get; set; }

        [JsonPropertyName("ordenamiento")]
        public string? Ordenamiento { get; set; }


        [JsonPropertyName("nombres")]
        public List<string>? Nombres { get; set; }

    }
}
