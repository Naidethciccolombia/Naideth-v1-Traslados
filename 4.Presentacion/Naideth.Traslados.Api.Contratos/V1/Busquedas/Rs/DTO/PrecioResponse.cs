
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 
    public class PrecioResponse
    {  

        [JsonPropertyName("total")]
        public TotalResponse Total { get; set; } 

        [JsonPropertyName("ptcTotal")]
        public List<PTCTotalResponse> PtcTotal { get; set; } 
    }




}
