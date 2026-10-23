using System;
using System.Collections.Generic;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public class DisponibilidadDTO
    {
        public DisponibilidadDTO()
        {
            Auxiliares = new List<TpaDTO>();
            Tarifas = new List<TarifaDTO>();
        }

        public string Source { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public List<TpaDTO> Auxiliares { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Duracion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaSalida { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraSalida { get; set; }
        public string Imagen { get; set; } = string.Empty;
        public PrecioDTO? Precio { get; set; }
        public List<TarifaDTO> Tarifas { get; set; }

        public string? LocalizadorReserva { get; set; }

        public string? EstadoReserva { get; set; }

        public string? EchoTokenReserva { get; set; }
    }
  
}
 

