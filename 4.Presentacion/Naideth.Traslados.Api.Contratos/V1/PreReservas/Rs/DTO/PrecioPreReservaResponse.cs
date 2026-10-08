
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs
{ 
    public class PrecioPreReservaResponse
    {  

        [JsonPropertyName("total")]
        public TotalPreReservaResponse Total { get; set; } 

        [JsonPropertyName("ptcTotal")]
        public List<PTCTotalPreReservaResponse> PtcTotal { get; set; } 
    }




}
