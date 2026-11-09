using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class ItemConfirmarRs
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("voucher")]
        public string? Voucher { get; set; }

        [JsonPropertyName("hasDeferredVoucher")]
        public bool? HasDeferredVoucher { get; set; }
    }
}
