using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class ServicioCotizacionRs
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("meet_and_greet")]
        public bool? MeetAndGreet { get; set; }
    }
}
