
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rq
{
    public sealed class BusquedaRequest
    {
        [Required]
        [JsonPropertyName("moneda")]
        public required string Moneda { get; set; }
        [Required]
        [JsonPropertyName("referencia")]
        public required string Referencia { get; set; }
        [Required]
        [JsonPropertyName("pasajeros")]
        public required List<PasajerosPeticionRequest> Pasajeros { get; set; } 
        [Required]
        [JsonPropertyName("trayectos")]
        public required List<TrayectoRequest> Trayectos { get; set; }

        [JsonPropertyName("filtros")]
        public FiltroRequest? Filtros { get; set; }

        [JsonPropertyName("pagina")]
        public int? Pagina { get; set; } = 1;

        [JsonPropertyName("registrosPagina")]
        public int? RegistrosPagina { get; set; } = 5;

    } 
}
