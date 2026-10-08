using System.Collections.Generic;

namespace Naideth.Traslados.Aplicacion.Comunes.DTO.Rs
{

    public sealed record TotalDTO(string Moneda, decimal Valor, decimal Impuesto, decimal Total);

}
