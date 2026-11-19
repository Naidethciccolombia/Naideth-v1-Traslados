using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class ConfirmarRs
    {
        [JsonPropertyName("items")]
        public List<ItemConfirmarRs>? Items { get; set; }

        [JsonPropertyName("warnings")]
        public string? Warnings { get; set; }

        [JsonPropertyName("prices")]
        public string? Prices { get; set; }

        [JsonPropertyName("agencyPrices")]
        public string? AgencyPrices { get; set; }

        [JsonPropertyName("bookingId")]
        public string? BookingId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("httpStatus")]
        public int? HttpStatus { get; set; }

        [JsonPropertyName("errorId")]
        public int? ErrorId { get; set; }

        [JsonPropertyName("errorDescription")]
        public string? ErrorDescription { get; set; }

        [JsonPropertyName("errors")]
        public string? Errors { get; set; }

        [JsonPropertyName("itemErrors")]
        public List<ItemErrorConfirmarRs>? ItemErrors { get; set; }
    }
}
