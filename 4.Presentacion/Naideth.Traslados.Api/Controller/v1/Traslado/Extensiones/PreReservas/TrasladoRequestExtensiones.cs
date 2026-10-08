
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Controllers.v1.Extensiones.PreReservas
{
    internal static class TrasladoRequestExtensiones
    {
        internal static PreReservaDTO ToCrearPreReservaDTO(this PreReservaRequest peticion)
        { 
            return new PreReservaDTO(
                peticion.IdBusqueda,
                peticion.IdTarifa,
                peticion.Moneda);
                  
        }

        internal static PreReservaSeleccionadaDTO ToCrearPreReservaSeleccionadaDTO(this PreReservaSeleccionadaRequest peticion)
        {

            return new PreReservaSeleccionadaDTO(
                peticion.IdPreReserva, 
                peticion.Upgrades?.ToUpgradePreReservaDTO()?? new List<pasajerosUpgradePreReservaDTO>()
                );


        }
        internal static List<pasajerosUpgradePreReservaDTO> ToUpgradePreReservaDTO(this List<pasajerosUpgradePreReservaRequest> lst)
        {
            return lst.Select(x => new pasajerosUpgradePreReservaDTO(x.IdPlan, x.Pax, x.Upgrade)).ToList();

        }

    }
}
