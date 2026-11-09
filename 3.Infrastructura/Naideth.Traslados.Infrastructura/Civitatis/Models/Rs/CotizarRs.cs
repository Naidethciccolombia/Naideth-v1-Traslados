using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class CotizarRs
    {
        [JsonPropertyName("quotes")]
        public List<CotizacionRs>? Quotes { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
