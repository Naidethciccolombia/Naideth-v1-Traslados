  
using Naideth.Traslados.Api.Contratos.V1.Emisiones.Rq;
using Naideth.Traslados.Aplicacion.Emisiones.DTO;

namespace Naideth.Traslados.Api.Controllers.v1.Extensiones.Emisiones.Rq
{
    internal static class EmisionRequestExtensiones
    { 
        internal static EmisionDTO ToCrearEmisionDTO(this EmisionRequest peticion)
        {
            return new EmisionDTO(
                peticion.Localizador,
                peticion.Source

                );
        }


    }
}
