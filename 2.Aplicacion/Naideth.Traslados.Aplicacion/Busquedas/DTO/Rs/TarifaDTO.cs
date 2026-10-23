using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public class TarifaDTO
    {
        public TarifaDTO()
        {
            Beneficios = new List<BeneficiosDTO>();
        }

        public string Id { get; set; } = string.Empty;
        public string IdPreReserva { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<BeneficiosDTO> Beneficios { get; set; }
        public string RangoEdad { get; set; } = string.Empty;
        public int CantidadAdultos { get; set; }
        public int CantidadNinos { get; set; }
        public int CantidadInfantes { get; set; }

        public DatosTarifaDTO? DatosTarifa { get; set; }
    }
 
}
 

