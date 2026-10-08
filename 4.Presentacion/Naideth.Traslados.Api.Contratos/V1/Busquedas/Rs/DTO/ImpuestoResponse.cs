
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{ 
    public class ImpuestoResponse
    {

        [JsonPropertyName("codigo")]
        public string Codigo { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("total")]
        public string Total { get; set; } 
         
    }




}
