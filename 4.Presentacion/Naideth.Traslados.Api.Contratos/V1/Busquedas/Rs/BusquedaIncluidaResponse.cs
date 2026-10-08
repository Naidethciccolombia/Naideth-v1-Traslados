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
    public sealed class BusquedaIncluidaResponse
    {
        [Required]
        [JsonPropertyName("idBusqueda")]
        public required Guid IdBusqueda { get; set; }    

        [Required]
        [JsonPropertyName("disponibilidad")]
        public required DisponibilidadResponse Disponibilidad { get; set; }


    }
}
