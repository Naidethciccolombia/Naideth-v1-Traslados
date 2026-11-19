using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class PrecioCotizacionRs
    {
        [JsonPropertyName("total")]
        public ImporteCotizacionRs? Total { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
