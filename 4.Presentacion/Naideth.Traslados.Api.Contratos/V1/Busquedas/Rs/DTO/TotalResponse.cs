
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 
    public class TotalResponse
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
