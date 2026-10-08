
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rq
{
    public sealed class PreReservaRequest
    {
        [Required]
        [JsonPropertyName("idBusqueda")]
        public required Guid IdBusqueda { get; set; }
        [Required]
        [JsonPropertyName("idTarifa")]
        public required Guid IdTarifa { get; set; }
        
        [Required]
        [JsonPropertyName("moneda")]
        public required string Moneda { get; set; }
          

    }
}
