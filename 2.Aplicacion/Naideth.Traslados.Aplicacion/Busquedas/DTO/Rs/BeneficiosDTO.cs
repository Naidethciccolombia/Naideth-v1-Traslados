using Azure;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{ 
    public sealed record BeneficiosDTO(string Nombre, string Descripcion, string Tipo);
 
}
 

