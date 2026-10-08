using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public class TrasladoIncluidaRsDTO
    { 
        public Guid IdBusqueda { get; set; } 
        public DisponibilidadDTO Disponibilidad { get; set; }       
    }
}
