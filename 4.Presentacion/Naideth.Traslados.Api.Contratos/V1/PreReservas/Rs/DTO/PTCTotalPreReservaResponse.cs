
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs
{  
    public class PTCTotalPreReservaResponse
    {

        [JsonPropertyName("ptc")]
        public string Ptc { get; set; }

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("total")]
        public TotalPreReservaResponse Total { get; set; }
    }
      
}
