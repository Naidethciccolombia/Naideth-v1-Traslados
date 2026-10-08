
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs
{ 
    public class TotalPreReservaResponse
    {

        [JsonPropertyName("moneda")]
        public string Moneda { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; }

        [JsonPropertyName("impuesto")]
        public string Impuesto { get; set; }


        [JsonPropertyName("total")]
        public string Total { get; set; }

    }
}
