 
using Naideth.Traslados.Aplicacion.ConexionesExternas.DTO;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.ConexionesExternas.Interfaces
{
    public interface IConexionesExternasRepositorio
    {
        Task<ResponseTravelKitStatusDTO> PeticionExternaTravelKitAsync(Guid id, string referencia, string codigo, StringContent data, Dictionary<string, string> headers, List<string> parameters, string Url, string verb, CancellationToken TokenCancelacion, int TimeOut, bool Logs);
        Task<ResponseTravelKitStatusDTO> PeticionExternaTravelKitGetAsync(Guid id, string referencia, string codigo,   Dictionary<string, string> headers, List<string> parameters, string Url, string verb, CancellationToken TokenCancelacion, int TimeOut, bool Logs);
        Task<ResponseTravelKitStatusDTO> PeticionExternaSoapAsync(Guid id, string referencia, string codigo, string dataXml, Dictionary<string, string> headers, string Url, CancellationToken TokenCancelacion, int TimeOut, bool Logs);

    }
}
