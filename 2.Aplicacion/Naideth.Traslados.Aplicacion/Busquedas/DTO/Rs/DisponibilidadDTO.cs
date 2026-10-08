using Azure;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public sealed record DisponibilidadDTO(string Source, string SourceName, string Id, List<TpaDTO>? Auxiliares, string Plan, string Duracion, string Tipo, DateTime FechaInicio, DateTime FechaSalida, string Imagen, PrecioDTO? Precio, List<TarifaDTO> Tarifas)
    {
        /// <summary>Localizador de la reserva (TransactionIdentifier de DestServicesBookV2). Identifica toda la reserva.</summary>
        public string? LocalizadorReserva { get; init; }

        /// <summary>Estado de la reserva (ResResponseType de DestServicesBookV2).</summary>
        public string? EstadoReserva { get; init; }

        /// <summary>EchoToken de DestServicesBookV2; requerido por DestServicesCommitV2.</summary>
        public string? EchoTokenReserva { get; init; }
    }
  
}
 

