 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs
{
    public sealed class PreReservaResponse
    {
        [Required]
        [JsonPropertyName("idPreReserva")]
        public required Guid IdPreReserva { get; set; }   

        [Required]
        [JsonPropertyName("disponibilidad")]
        public required DisponibilidadPreReservaResponse Disponibilidad { get; set; } 


    }
}
