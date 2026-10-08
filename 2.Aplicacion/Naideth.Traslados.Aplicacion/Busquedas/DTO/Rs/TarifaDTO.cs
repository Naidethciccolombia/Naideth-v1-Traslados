using Azure;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public sealed record TarifaDTO(string Id, string IdPreReserva, string Category, string PdfCondicciones,string PdfDescripcion, string Nombre, List<BeneficiosDTO> Beneficios, string RangoEdad, int CantidadAdultos, int CantidadNinos, int CantidadInfantes, List<UpgradesDTO> Upgrades)
    {
        /// <summary>DetailBookingCode del detalle seleccionado; obligatorio en DestServicesBookV2.</summary>
        public string? Localizador { get; init; }

        /// <summary>ProductBookingCode del producto (AvailResponseV2Product); obligatorio en DestServicesBookV2.</summary>
        public string? CodigoProducto { get; init; }

        /// <summary>EchoToken|ProductCode|ConceptCode del concepto; opcional en DestServicesBookV2.</summary>
        public string? CodigoAuxiliar { get; init; }

        /// <summary>Tipo de vehículo (ProductName del proveedor).</summary>
        public string? TipoVehiculo { get; init; }

        /// <summary>Modelo del vehículo (Title de Contents, o SmallContent si no hay Title).</summary>
        public string? ModeloVehiculo { get; init; }

        /// <summary>Imagen del vehículo (ProductImage del proveedor).</summary>
        public string? ImagenVehiculo { get; init; }

        /// <summary>Indica si la reserva es flexible. Null cuando el proveedor no lo informa.</summary>
        public bool? Flexible { get; init; }

        /// <summary>Duración del viaje. Null cuando el proveedor no lo informa.</summary>
        public string? DuracionViaje { get; init; }

        /// <summary>Hora de inicio del recorrido (PickUpTime del proveedor).</summary>
        public string? HoraInicio { get; init; }

        /// <summary>Espera máxima del transporte. Null cuando el proveedor no lo informa.</summary>
        public string? EsperaMaxima { get; init; }

        /// <summary>Cantidad de pasajeros solicitada en la búsqueda.</summary>
        public int? CantidadPasajeros { get; init; }

        /// <summary>Cantidad máxima de maletas. Null cuando el proveedor no lo informa.</summary>
        public int? MaletasMaximas { get; init; }

        /// <summary>Penalidades de cancelación del vehículo (CancelPenalties de DestServicesBookV2).</summary>
        public List<PoliticaCancelacionDTO>? PoliticasCancelacion { get; init; }

        /// <summary>ConceptBookingCode devuelto por DestServicesBookV2; requerido por DestServicesCommitV2.</summary>
        public string? ConceptoReserva { get; init; }
    }
 
}
 

