using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rq
{
    public class VueloReservarRq
    {
        [JsonPropertyName("flight_number")]
        public string FlightNumber { get; set; } = string.Empty;

        [JsonPropertyName("airline")]
        public string Airline { get; set; } = string.Empty;

        [JsonPropertyName("origin_city")]
        public string OriginCity { get; set; } = string.Empty;
    }
}
