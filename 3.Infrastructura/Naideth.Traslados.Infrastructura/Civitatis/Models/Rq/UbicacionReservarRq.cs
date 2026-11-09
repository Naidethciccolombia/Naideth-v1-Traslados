using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rq
{
    public class UbicacionReservarRq
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
