
using Naideth.Traslados.Api.Contratos.V1.Reservas.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Reservas
{
    public class TrasladoResponseReserva
    {
        [Required]
        [JsonPropertyName("localizador")]
        public string Localizador { get; set; }

        [Required]
        [JsonPropertyName("precio")]
        public required PrecioReservaResponse Precio { get; set; }

    }
}