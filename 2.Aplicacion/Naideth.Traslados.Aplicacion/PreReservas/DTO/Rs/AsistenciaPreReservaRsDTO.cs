using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs
{
    public class TrasladoPreReservaRsDTO
    {
        
        public Guid IdPreReserva { get; set; } 
        public DisponibilidadDTO Disponibilidad { get; set; }   
    }
}
