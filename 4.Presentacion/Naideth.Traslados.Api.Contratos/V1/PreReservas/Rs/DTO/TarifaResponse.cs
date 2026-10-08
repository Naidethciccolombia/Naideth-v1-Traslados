
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs
{ 
    public sealed class TarifaResponse
    { 

        [Required]
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [Required]
        [JsonPropertyName("category")]
        public required string Category { get; set; } 
        [JsonPropertyName("pdfCondicciones")]
        public required string PdfCondicciones { get; set; } 
        [JsonPropertyName("pdfDescripcion")]
        public required string PdfDescripcion { get; set; }
        [Required]
        [JsonPropertyName("nombre")]
        public required string Nombre { get; set; } 
        [Required]
        [JsonPropertyName("rangoEdad")]
        public required string RangoEdad { get; set; } 
        [Required]
        [JsonPropertyName("beneficios")]
        public required List<BeneficiosResponse> Beneficios { get; set; }
        [Required]
        [JsonPropertyName("upgrades")]
        public required List<UpgradeResponse> Upgrades { get; set; }
        [Required]
        [JsonPropertyName("cantidadAdultos")]
        public required int CantidadAdultos { get; set; }
        [Required]
        [JsonPropertyName("cantidadNinos")]
        public required int CantidadNinos { get; set; }
        [Required]
        [JsonPropertyName("cantidadInfantes")]
        public required int CantidadInfantes { get; set; }


    }

}
