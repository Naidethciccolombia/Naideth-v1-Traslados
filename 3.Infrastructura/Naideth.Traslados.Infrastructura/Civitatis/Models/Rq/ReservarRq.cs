using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rq
{
    public class ReservarRq
    {
        [JsonPropertyName("quote_id")]
        public string QuoteId { get; set; } = string.Empty;

        [JsonPropertyName("holder")]
        public HolderReservarRq Holder { get; set; } = new HolderReservarRq();

        [JsonPropertyName("outbound_details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DetallesTransferReservarRq? OutboundDetails { get; set; }

        [JsonPropertyName("inbound_details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DetallesTransferReservarRq? InboundDetails { get; set; }

        [JsonPropertyName("addons")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AddonReservarRq>? Addons { get; set; }
    }
}
