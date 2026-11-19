using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rq
{
    public class CotizarRq
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("from")]
        public UbicacionCotizarRq From { get; set; } = new UbicacionCotizarRq();

        [JsonPropertyName("to")]
        public UbicacionCotizarRq To { get; set; } = new UbicacionCotizarRq();

        [JsonPropertyName("outbound_at")]
        public string? OutboundAt { get; set; }

        [JsonPropertyName("inbound_at")]
        public string? InboundAt { get; set; }

        [JsonPropertyName("passengers")]
        public int Passengers { get; set; }
    }
}
