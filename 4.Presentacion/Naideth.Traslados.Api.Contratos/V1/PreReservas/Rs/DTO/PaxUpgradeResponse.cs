
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs
{
   
    public sealed class PaxUpgradeResponse
    {
        [Required]
        [JsonPropertyName("tipo")]
        public required string Tipo { get; set; }
        [Required]
        [JsonPropertyName("edades")]
        public required int Edades { get; set; } 

    }
     
} 
