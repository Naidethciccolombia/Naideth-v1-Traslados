using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs.DTO
{
    public sealed class IntegradorStatusResponse
    {
        [JsonPropertyName("nombre")]
        public required string Nombre { get; set; }

        [JsonPropertyName("estado")]
        public required string Estado { get; set; }

        [JsonPropertyName("tiempoRespuesta")]
        public double? TiempoRespuesta { get; set; }

        [JsonPropertyName("cantidadDisponibilidades")]
        public required int CantidadDisponibilidades { get; set; }

        [JsonPropertyName("fechaInicio")]
        public required DateTime FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")]
        public DateTime? FechaFin { get; set; }

        [JsonPropertyName("mensajeError")]
        public string? MensajeError { get; set; }
    }
}
