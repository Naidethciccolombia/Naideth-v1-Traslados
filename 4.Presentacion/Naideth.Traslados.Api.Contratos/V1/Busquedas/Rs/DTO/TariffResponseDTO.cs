
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs
{
    public class TariffResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }


        //[JsonPropertyName("moneda")]
        //public string Moneda { get; set; }


        [JsonPropertyName("codigoIntegracion")]
        public string codigoIntegracion { get; set; }


        [JsonPropertyName("integracion")]
        public string Integracion { get; set; }


        [JsonPropertyName("precioOferta")]
        public bool PrecioOferta { get; set; }


        [JsonPropertyName("precio")]
        public List<PrecioResponse> Precio { get; set; }

        [JsonPropertyName("alimentacion")]
        public string Alimentacion { get; set; }

        [JsonPropertyName("alimentacionCodigo")]
        public string AlimentacionCodigo { get; set; }


        [JsonPropertyName("penalidadInmediata")]
        public bool PenalidadInmediata { get; set; }


          
        [JsonPropertyName("empaquetar")]
        public bool Empaquetar { get; set; }

        [JsonPropertyName("cadenaHotelCode")]
        public string CadenaHotelCode { get; set; }

        [JsonPropertyName("cadenaHotel")]
        public string CadenaHotel { get; set; }
    }
     

}
