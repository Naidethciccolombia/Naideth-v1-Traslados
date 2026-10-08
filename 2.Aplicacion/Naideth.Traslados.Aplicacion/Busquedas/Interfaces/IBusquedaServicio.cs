
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.Busquedas.Interfaces
{
    public interface IBusquedaServicio
    {
        Task<TrasladoRsDTO> BusquedaAsync(BusquedaDTO peticion, CancellationToken TokenCancelacion);
        Task<TrasladoPreReservaRsDTO> TrasladoIncluidaAsync(BusquedaDTO peticion, CancellationToken TokenCancelacion);
       
    }
}
