
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{  
    public class PTCTotalResponse
    {

        [JsonPropertyName("ptc")]
        public string Ptc { get; set; }

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("total")]
        public TotalResponse Total { get; set; }
    }
      
}
