using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rq
{
    public class AddonReservarRq
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
