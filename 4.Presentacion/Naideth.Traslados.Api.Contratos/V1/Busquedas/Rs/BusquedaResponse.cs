using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{
    public sealed class BusquedaResponse
    {
        [Required]
        [JsonPropertyName("idBusqueda")]
        public required Guid IdBusqueda { get; set; }

        [Required]
        [JsonPropertyName("tiempo")]
        public required string Tiempo { get; set; }
        [Required]
        [JsonPropertyName("EsParcial")]
        public required bool EsParcial { get; set; }

        [Required]
        [JsonPropertyName("pagina")]
        public required int Pagina { get; set; }

        [Required]
        [JsonPropertyName("registrosPagina")]
        public required int RegistrosPagina { get; set; }

        [Required]
        [JsonPropertyName("totalItems")]
        public required int TotalItems { get; set; } 


        [Required]
        [JsonPropertyName("filtros")]
        public required FiltroResponse Filtros { get; set; }

        [Required]
        [JsonPropertyName("disponibilidades")]

        public required List<DisponibilidadResponse> Disponibilidades { get; set; }
        [JsonPropertyName("integradoresStatus")]
        public List<IntegradorStatusResponse> IntegradoresStatus { get; set; }


    }
}
