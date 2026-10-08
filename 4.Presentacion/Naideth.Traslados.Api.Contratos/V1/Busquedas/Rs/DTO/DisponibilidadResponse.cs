
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{

    public sealed class DisponibilidadResponse
    { 

        [Required]
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [Required]
        [JsonPropertyName("plan")]
        public required string Plan { get; set; }
        [Required]
        [JsonPropertyName("duracion")]
        public required string Duracion { get; set; }
        [Required]
        [JsonPropertyName("tipo")]
        public required string Tipo { get; set; }
        [Required]
        [JsonPropertyName("fechaInicio")]
        public required string FechaInicio { get; set; }
        [Required]
        [JsonPropertyName("fechaSalida")]
        public required string FechaSalida { get; set; }
        [Required]
        [JsonPropertyName("imagen")]
        public required string Imagen { get; set; }
        [Required]
        [JsonPropertyName("precio")]
        public required PrecioResponse Precio { get; set; }
        [Required]
        [JsonPropertyName("fuente")]
        public required string Fuente { get; set; }
        //[Required]
        //[JsonPropertyName("tarifas")]
        //public required List<TarifaResponse> Tarifas { get; set; }


    }

}
