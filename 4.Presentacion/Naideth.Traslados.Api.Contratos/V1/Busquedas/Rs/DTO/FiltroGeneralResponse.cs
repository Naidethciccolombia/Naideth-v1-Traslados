
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 
    public class FiltroGeneralResponse
    {

        [JsonPropertyName("codigo")]
        public string Codigo { get; set; }

        [JsonPropertyName("sel")]
        public int Sel { get; set; }


        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }
    } 
}
