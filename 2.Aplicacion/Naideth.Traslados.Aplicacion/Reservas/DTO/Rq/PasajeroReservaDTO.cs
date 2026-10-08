
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq; 
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.Reservas.DTO.Rq
{
    public sealed record PasajeroReservaDTO(string Tipo, string IdSexo, string FechaNacimiento, string Nombres, string Apellidos, string Documento, string IdDocument, string Nacionalidad, List<string> Upgrade);

}