using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{
    public sealed class TrayectoRequest
    {
        [JsonPropertyName("numero")]
        public required int Numero { get; set; }

        [JsonPropertyName("fecha")]
        public required DateTime Fecha { get; set; }

        [JsonPropertyName("hora")]
        public required string Hora { get; set; }

        [JsonPropertyName("iataOrigen")]
        public required UbicacionRequest IataOrigen { get; set; }

        [JsonPropertyName("iataDestino")]
        public required UbicacionRequest IataDestino { get; set; }
    }
}
