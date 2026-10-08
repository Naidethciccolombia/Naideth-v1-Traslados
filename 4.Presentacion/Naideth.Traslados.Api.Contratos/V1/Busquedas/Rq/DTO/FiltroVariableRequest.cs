

using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 

    public class FiltroVariableRequest
    {

        [JsonPropertyName("penalidadInmediata")]
        public bool PenalidadInmediata { get; set; }
    }

}
