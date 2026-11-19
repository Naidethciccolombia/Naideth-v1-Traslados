using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rq
{
    public class DetallesTransferReservarRq
    {
        [JsonPropertyName("flight")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VueloReservarRq? Flight { get; set; }

        [JsonPropertyName("location")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UbicacionReservarRq? Location { get; set; }

        [JsonPropertyName("comments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Comments { get; set; }
    }
}
