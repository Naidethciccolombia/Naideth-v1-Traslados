using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class EquipajeCotizacionRs
    {
        [JsonPropertyName("large_bags")]
        public int? LargeBags { get; set; }

        [JsonPropertyName("small_bags")]
        public int? SmallBags { get; set; }
    }
}
