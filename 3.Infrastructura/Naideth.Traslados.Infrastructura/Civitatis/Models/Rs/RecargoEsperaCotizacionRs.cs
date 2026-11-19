using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class RecargoEsperaCotizacionRs
    {
        [JsonPropertyName("price")]
        public ImporteCotizacionRs? Price { get; set; }

        [JsonPropertyName("minutes")]
        public int? Minutes { get; set; }
    }
}
