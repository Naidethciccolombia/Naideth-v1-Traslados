using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class PrecioReservarRs
    {
        [JsonPropertyName("total")]
        public ImporteReservarRs? Total { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
