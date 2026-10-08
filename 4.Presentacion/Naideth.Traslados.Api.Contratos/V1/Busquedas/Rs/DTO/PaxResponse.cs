
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{
    public sealed class PaxResponse
    {   

        [JsonPropertyName("adultos")]
        public int Adultos { get; set; }

        [JsonPropertyName("nino")]
        public int Nino { get; set; }

        [JsonPropertyName("edadNinos")]
        public List<string> EdadNinos { get; set; }

        [JsonPropertyName("edadAdultos")]
        public List<string> EdadAdultos { get; set; }
    }


}
