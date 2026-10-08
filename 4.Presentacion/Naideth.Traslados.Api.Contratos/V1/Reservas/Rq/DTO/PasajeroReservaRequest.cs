using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Naideth.Traslados.Api.Contratos.V1.Reservas.Rq.DTO
{
    public sealed class PasajeroReservaRequest
    {  
        [Required]
        [JsonPropertyName("tipo")]
        public required string Tipo { get; set; }

        [Required]
        [JsonPropertyName("idSexo")]
        public required string IdSexo { get; set; }

        [Required]
        [JsonPropertyName("fechaNacimiento")]
        public required string FechaNacimiento { get; set; }

        [Required]
        [JsonPropertyName("nombres")]
        public required string Nombres { get; set; }

        [Required]
        [JsonPropertyName("apellidos")]
        public required string Apellidos { get; set; } 

        [Required]
        [JsonPropertyName("documento")]
        public required string Documento { get; set; }
          
        [Required]
        [JsonPropertyName("idDocument")]
        public required string IdDocument { get; set; }
        [Required]
        [JsonPropertyName("nacionalidad")]
        public required string Nacionalidad { get; set; }

        [JsonPropertyName("upgrade")]
        public List<string>? Upgrade { get; set; }
    }

}