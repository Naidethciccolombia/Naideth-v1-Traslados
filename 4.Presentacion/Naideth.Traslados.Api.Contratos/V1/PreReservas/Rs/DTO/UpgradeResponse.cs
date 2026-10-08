
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs
{
   
    public sealed class UpgradeResponse
    {
        [Required]
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [Required]
        [JsonPropertyName("nombre")]
        public required string Nombre { get; set; }
        [Required]
        [JsonPropertyName("rangoEdad")]
        public required string RangoEdad { get; set; }
        [Required]
        [JsonPropertyName("valorUnitario")]
        public required string ValorUnitario { get; set; }
        [Required]
        [JsonPropertyName("precioUnitario")]
        public required string PrecioUnitario { get; set; }
        [Required]
        [JsonPropertyName("paxUpgrade")]
        public required List<PaxUpgradeResponse> PaxUpgrade { get; set; }

    }
     
}
