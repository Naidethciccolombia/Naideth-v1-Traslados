using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using Naideth.Traslados.Dominio.ConvercionMoneda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.ConvercionMoneda.Interfaces
{
    public interface IConvercionMonedaServicio
    { 
     //   Task<decimal> ConvertirMonedaAsync(decimal valor, string monedaOrigen, string monedaDestino, CancellationToken cancellationToken);
        decimal ConvertirMoneda(decimal valor, string monedaOrigen, string monedaDestino); 
    }
}
