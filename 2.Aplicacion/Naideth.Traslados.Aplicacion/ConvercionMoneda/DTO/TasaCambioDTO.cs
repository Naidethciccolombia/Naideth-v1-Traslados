using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.ConvercionMoneda.DTO
{ 
    public sealed record TasaCambioDTO(Guid IdTasaCambio, Guid IdMoneda, string Moneda, decimal Valor, DateTime Fecha);

}
