
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq; 
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.Reservas.DTO.Rq
{
    public sealed record ReservaDTO(Guid IdPreReserva, List<PasajeroReservaDTO> Pasajeros,string Email, string? Telefono , string? NombreContacto , string? Observaciones); 
}
 