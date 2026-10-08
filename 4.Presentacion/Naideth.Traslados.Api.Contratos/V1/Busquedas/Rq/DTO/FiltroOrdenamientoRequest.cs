

using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 

    public class FiltroOrdenamientoRequest
    {

        [JsonPropertyName("columna")]
        public string Columna { get; set; }

        [JsonPropertyName("ascendente")]
        public bool Ascendente { get; set; }
    }

}
