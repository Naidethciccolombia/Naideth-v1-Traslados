using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Naideth.Traslados.Api.Contratos.V1.Cancelaciones.Rq
{
    public sealed class CancelacionRequest
    {
        [Required]
        [JsonPropertyName("source")]
        public required string Source { get; set; }
        [Required]
        [JsonPropertyName("localizador")]
        public required string Localizador { get; set; }

    }
  
}
