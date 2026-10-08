

using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 

    public class FiltroVariableResponse
    {

        [JsonPropertyName("penalidadInmediata")]
        public List<bool> PenalidadInmediata { get; set; }
         
    }

}
