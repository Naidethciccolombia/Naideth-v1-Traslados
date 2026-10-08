
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.PreReservas.Interfaces
{
    public interface IPreReservaServicio
    {
        Task<TrasladoPreReservaRsDTO> PreReservaAsync(PreReservaDTO peticion, CancellationToken TokenCancelacion);
        // Task<TrasladoIncluidaRsDTO> TrasladoIncluidaAsync(PreReservaDTO peticion, CancellationToken TokenCancelacion);
        Task<TrasladoPreReservaRsDTO> PreReservaSeleccionadaAsync(PreReservaSeleccionadaDTO peticion, CancellationToken TokenCancelacion);


    }
}
