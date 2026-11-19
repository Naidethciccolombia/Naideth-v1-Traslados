using System.Text.Json;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class ExtraCotizacionRs
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("price")]
        public ImporteCotizacionRs? Price { get; set; }

        [JsonPropertyName("max_allowed")]
        public int? MaxAllowed { get; set; }

        [JsonPropertyName("is_mandatory")]
        public bool? IsMandatory { get; set; }

        [JsonPropertyName("is_guaranteed")]
        public bool? IsGuaranteed { get; set; }

        [JsonPropertyName("specs")]
        public JsonElement? Specs { get; set; }
    }
}
