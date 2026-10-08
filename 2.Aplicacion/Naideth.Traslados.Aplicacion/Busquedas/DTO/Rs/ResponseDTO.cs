using Azure;
using Naideth.Traslados.Dominio.Estados;
using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public sealed record ResponseDTO(Estado? Estado, DisponibilidadesDTO? Data); 
}
