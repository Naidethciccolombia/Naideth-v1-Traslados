

using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 

    public class PasajerosPeticionRequest
    {

        [JsonPropertyName("tipo")]
        public required string Tipo { get; set; }

        [JsonPropertyName("cantidad")]
        public required int Cantidad { get; set; }

        [JsonPropertyName("edades")]
        public required List<int>?  Edades { get; set; }

        [JsonPropertyName("upgrade")]
        public int? Upgrade { get; set; }

    }

}
 