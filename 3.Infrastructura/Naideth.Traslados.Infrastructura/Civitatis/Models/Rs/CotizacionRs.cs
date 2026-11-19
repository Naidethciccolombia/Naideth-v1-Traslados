using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class CotizacionRs
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("expires_at")]
        public string? ExpiresAt { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("passengers")]
        public int? Passengers { get; set; }

        [JsonPropertyName("price")]
        public PrecioCotizacionRs? Price { get; set; }

        [JsonPropertyName("segments")]
        public List<TramoCotizacionRs>? Segments { get; set; }
    }
}
