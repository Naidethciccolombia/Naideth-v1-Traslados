using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public class TrasladoRsDTO
    {
        public TrasladoRsDTO()
        {
            Disponibilidades = new List<DisponibilidadDTO>(); 
        }
        public Guid IdBusqueda { get; set; } 
        public List<DisponibilidadDTO> Disponibilidades { get; set; }   
        public int Pagina { get; set; } // Página actual
        public int RegistrosPagina { get; set; }    // Tamaño de página
        public bool EsParcial { get; set; }    // Tamaño de página
        public int TotalItems { get; set; }  // Total de elementos
        public int TotalPaginas
        {
            get
            {
                return (int)Math.Ceiling((double)TotalItems / RegistrosPagina);
            }
        }

        public  FiltroResponseDTO? Filtros { get; set; }
        public string Tiempo { get; set; }  // Total de elementos
        public List<IntegradorStatusDTO> IntegradoresStatus { get; set; }
    }
}
