using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class VehiculoCotizacionRs
    {
        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("eco_label")]
        public string? EcoLabel { get; set; }

        [JsonPropertyName("models")]
        public List<string>? Models { get; set; }

        [JsonPropertyName("images")]
        public List<string>? Images { get; set; }

        [JsonPropertyName("is_adapted")]
        public bool? IsAdapted { get; set; }

        [JsonPropertyName("pax_capacity")]
        public int? PaxCapacity { get; set; }

        [JsonPropertyName("baggage_capacity")]
        public EquipajeCotizacionRs? BaggageCapacity { get; set; }
    }
}
