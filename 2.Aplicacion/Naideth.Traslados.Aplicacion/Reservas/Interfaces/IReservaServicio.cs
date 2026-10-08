
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.Reservas.Interfaces
{
    public interface IReservaServicio
    {
        Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, CancellationToken TokenCancelacion);
        // Task<TrasladoIncluidaRsDTO> TrasladoIncluidaAsync(PreReservaDTO peticion, CancellationToken TokenCancelacion);
       
    }
}
