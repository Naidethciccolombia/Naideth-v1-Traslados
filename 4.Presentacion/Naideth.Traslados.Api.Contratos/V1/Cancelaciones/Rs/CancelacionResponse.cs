using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Cancelacion.Rs
{
    public sealed class CancelacionResponse
    {
        [Required]
        [JsonPropertyName("source")]
        public required string Source { get; set; }
        [Required]
        [JsonPropertyName("localizador")]
        public required string Localizador { get; set; }
        [Required]
        [JsonPropertyName("estado")]
        public required string Estado { get; set; }
          
        [Required]
        [JsonPropertyName("mensaje")]
        public required string Mensaje { get; set; }


    }
      
}
