
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Reservas.Rs
{ 
    public class PrecioReservaResponse
    {
         

        [JsonPropertyName("fuente")]
        public string Fuente { get; set; }

        [JsonPropertyName("total")]
        public TotalReservaResponse Total { get; set; } 

        [JsonPropertyName("ptcTotal")]
        public List<PTCTotalResevaResponse> PtcTotal { get; set; } 
    }




}
