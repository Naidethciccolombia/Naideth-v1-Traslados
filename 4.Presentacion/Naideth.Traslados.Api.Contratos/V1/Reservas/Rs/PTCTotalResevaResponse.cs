
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Reservas.Rs
{  
    public class PTCTotalResevaResponse
    {

        [JsonPropertyName("ptc")]
        public string Ptc { get; set; }

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("total")]
        public TotalReservaResponse Total { get; set; }
    }
      
}
