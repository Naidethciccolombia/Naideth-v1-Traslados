
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs
{
  
    public sealed class BeneficiosResponse
    { 

        [Required]
        [JsonPropertyName("nombre")]
        public required string Nombre { get; set; }
        [Required]
        [JsonPropertyName("descripcion")]
        public required string Descripcion { get; set; }
        [Required]
        [JsonPropertyName("tipo")]
        public required string Tipo { get; set; }

       
    }  
 
}
