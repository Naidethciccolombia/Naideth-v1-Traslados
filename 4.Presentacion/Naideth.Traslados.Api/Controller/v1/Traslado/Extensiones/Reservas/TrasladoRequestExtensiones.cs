 
using Naideth.Traslados.Api.Contratos.V1.Reservas.Rq;
using Naideth.Traslados.Api.Contratos.V1.Reservas.Rq.DTO;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rq;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Controllers.v1.Extensiones.Reservas
{
    internal static class TrasladoRequestExtensiones
    {
        internal static ReservaDTO ToCrearReservaDTO(this ReservaRequest peticion)
        { 
            return new ReservaDTO(peticion.IdPreReserva,
                peticion.Pasajeros.ToPasajeroReservaDTO(),
                peticion.Email, 
                peticion.Telefono,
                peticion.NombreContacto,
                peticion.Observaciones
                );                  
        }

        internal static List<PasajeroReservaDTO> ToPasajeroReservaDTO(this List<PasajeroReservaRequest> peticion)
        {
            return peticion.Select(p => new PasajeroReservaDTO(p.Tipo, p.IdSexo, p.FechaNacimiento, p.Nombres, p.Apellidos, p.Documento, p.IdDocument, p.Nacionalidad, p.Upgrade)).ToList();
        } 


    }
}
