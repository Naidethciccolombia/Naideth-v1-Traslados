
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq; 
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq
{ 
    public sealed record pasajerosUpgradePreReservaDTO(string IdPlan, int Pax, List<string> Upgrade);
}

 