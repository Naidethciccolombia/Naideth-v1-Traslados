using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{
    public sealed class UbicacionRequest
    {
        [JsonPropertyName("origen")]
        public required string Origen { get; set; }

        [JsonPropertyName("tipo")]
        public required string Tipo { get; set; }
    }
}
