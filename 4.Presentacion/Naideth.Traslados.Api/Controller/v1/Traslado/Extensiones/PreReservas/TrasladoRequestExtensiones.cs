
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;

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

            return new PreReservaSeleccionadaDTO
            {
                IdPreReserva = peticion.IdPreReserva
            };


        }

    }
}
