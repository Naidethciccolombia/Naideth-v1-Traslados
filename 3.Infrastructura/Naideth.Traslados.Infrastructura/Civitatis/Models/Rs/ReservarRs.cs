using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class ReservarRs
    {
        [JsonPropertyName("quote_id")]
        public string? QuoteId { get; set; }

        [JsonPropertyName("cart_id")]
        public string? CartId { get; set; }

        [JsonPropertyName("price")]
        public PrecioReservarRs? Price { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
