

using System.Text.Json.Serialization;
namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 

    public class FiltroPrecioRequest
    {

        [JsonPropertyName("minimo")]
        public string minimo { get; set; }

        [JsonPropertyName("maximo")]
        public string maximo { get; set; } 
    }

}
