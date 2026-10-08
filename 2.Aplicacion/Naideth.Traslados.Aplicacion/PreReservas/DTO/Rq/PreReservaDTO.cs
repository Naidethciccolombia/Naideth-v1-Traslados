
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq; 
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq
{
    public sealed record PreReservaDTO(Guid IdBusqueda, Guid IdTarifa, string Moneda); 
}

 