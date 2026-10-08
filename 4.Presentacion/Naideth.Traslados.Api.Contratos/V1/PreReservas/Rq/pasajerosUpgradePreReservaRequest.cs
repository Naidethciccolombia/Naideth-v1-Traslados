
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.PreReservas.Rq
{
    public sealed class pasajerosUpgradePreReservaRequest
    {

        [JsonPropertyName("idPlan")]
        public required string IdPlan { get; set; }  

        [JsonPropertyName("pax")]
        public required int Pax { get; set; }  

        [JsonPropertyName("upgrade")]
        public List<string>? Upgrade { get; set; }  

    }
}
