using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public class DatosTarifaDTO
    {
        public string? Localizador { get; set; }

        public string? CodigoProducto { get; set; }

        public string? CodigoAuxiliar { get; set; }

        public string? TipoVehiculo { get; set; }

        public string? ModeloVehiculo { get; set; }

        public string? ImagenVehiculo { get; set; }

        public bool? Flexible { get; set; }

        public string? DuracionViaje { get; set; }

        public string? HoraInicio { get; set; }

        public string? EsperaMaxima { get; set; }

        public int? CantidadPasajeros { get; set; }

        public int? MaletasMaximas { get; set; }

        public List<PoliticaCancelacionDTO>? PoliticasCancelacion { get; set; }

        public string? ConceptoReserva { get; set; }
    }
}
