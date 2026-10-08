using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO   
{
    public sealed record PasajerosPeticionDTO(string tipo, int cantidad, List<int>? edad,int? upgrade);
} 