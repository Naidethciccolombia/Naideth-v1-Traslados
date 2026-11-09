using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class PoliticaCancelacionCotizacionRs
    {
        [JsonPropertyName("free_cancellation_hours")]
        public int? FreeCancellationHours { get; set; }
    }
}
