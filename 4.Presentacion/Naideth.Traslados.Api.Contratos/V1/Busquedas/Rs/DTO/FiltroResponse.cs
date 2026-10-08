 
using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{
    public class FiltroResponse
    {
        [JsonPropertyName("precios")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public required FiltroPrecioResponse Precios { get; set; }

        [JsonPropertyName("fuentes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public required List<FiltroGeneralResponse> Fuentes { get; set; }

        [JsonPropertyName("nombres")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public required List<FiltroGeneralResponse> Nombres { get; set; }
         

        [JsonPropertyName("ordenamientos")]

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        public required List<FiltroGeneralResponse> Ordenamientos { get; set; }
    }
}
