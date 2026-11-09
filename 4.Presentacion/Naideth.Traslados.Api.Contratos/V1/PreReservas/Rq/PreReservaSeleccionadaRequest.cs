
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rq
{
    public sealed class PreReservaSeleccionadaRequest
    {
        [Required]
        [JsonPropertyName("idPreReserva")]
        public required Guid IdPreReserva { get; set; }

    } 
}
