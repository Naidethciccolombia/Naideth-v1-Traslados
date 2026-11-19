using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rs
{
    public class TramoCotizacionRs
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("from")]
        public UbicacionCotizarRs? From { get; set; }

        [JsonPropertyName("to")]
        public UbicacionCotizarRs? To { get; set; }

        [JsonPropertyName("pickup_at")]
        public string? PickupAt { get; set; }

        [JsonPropertyName("service")]
        public ServicioCotizacionRs? Service { get; set; }

        [JsonPropertyName("vehicle")]
        public VehiculoCotizacionRs? Vehicle { get; set; }

        [JsonPropertyName("price")]
        public PrecioCotizacionRs? Price { get; set; }

        [JsonPropertyName("booking_requirements")]
        public List<string>? BookingRequirements { get; set; }

        [JsonPropertyName("cancellation_policy")]
        public PoliticaCancelacionCotizacionRs? CancellationPolicy { get; set; }

        [JsonPropertyName("wait_surcharge")]
        public RecargoEsperaCotizacionRs? WaitSurcharge { get; set; }

        [JsonPropertyName("available_addons")]
        public List<ExtraCotizacionRs>? AvailableAddons { get; set; }
    }
}
