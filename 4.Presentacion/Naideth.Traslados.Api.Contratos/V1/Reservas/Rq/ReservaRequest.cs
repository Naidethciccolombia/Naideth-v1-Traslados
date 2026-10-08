
using Naideth.Traslados.Api.Contratos.V1.Reservas.Rq.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Naideth.Traslados.Api.Contratos.V1.Reservas.Rq
{
    public sealed class ReservaRequest
    {
        [Required]
        [JsonPropertyName("idPreReserva")]
        public required Guid IdPreReserva { get; set; } 
         
        [JsonPropertyName("observaciones")]
        public string? Observaciones { get; set; }
         
        [Required]
        [JsonPropertyName("email")]
        public required string Email { get; set; }

        [JsonPropertyName("telefono")]
        public string? Telefono { get; set; }

        [Required]
        [JsonPropertyName("nombreContacto")]
        public string? NombreContacto { get; set; }

        [Required]
        [JsonPropertyName("pasajeros")]
        public required List<PasajeroReservaRequest> Pasajeros { get; set; } 

    } 
    
}
