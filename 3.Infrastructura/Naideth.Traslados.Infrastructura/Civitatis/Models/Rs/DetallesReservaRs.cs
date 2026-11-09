using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class DetallesReservaRs
    {
        [JsonPropertyName("bookingId")]
        public long? BookingId { get; set; }

        [JsonPropertyName("cartId")]
        public string? CartId { get; set; }

        [JsonPropertyName("zoneStartEnd")]
        public string? ZoneStartEnd { get; set; }

        [JsonPropertyName("pickUpDate")]
        public string? PickUpDate { get; set; }

        [JsonPropertyName("creationDate")]
        public string? CreationDate { get; set; }

        [JsonPropertyName("modificationDateTime")]
        public string? ModificationDateTime { get; set; }

        [JsonPropertyName("from")]
        public OrigenDestinoReservaRs? From { get; set; }

        [JsonPropertyName("to")]
        public OrigenDestinoReservaRs? To { get; set; }

        [JsonPropertyName("vehicle")]
        public string? Vehicle { get; set; }

        [JsonPropertyName("clientName")]
        public string? ClientName { get; set; }

        [JsonPropertyName("clientLastName")]
        public string? ClientLastName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("numberOfPeople")]
        public int? NumberOfPeople { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
