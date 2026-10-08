
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq; 
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq
{ 
    public sealed record PreReservaSeleccionadaDTO(Guid? IdPreReserva, List<pasajerosUpgradePreReservaDTO>? Upgrades); 
}

 