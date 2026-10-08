
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Naideth.Traslados.Api.Contratos.V1.Reservas.Rs
{ 
    public class TotalReservaResponse
    {

        [JsonPropertyName("moneda")]
        public string Moneda { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; }

        [JsonPropertyName("total")]
        public string Total { get; set; }
         

    }
}
