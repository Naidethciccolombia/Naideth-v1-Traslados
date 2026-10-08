  
using Naideth.Traslados.Api.Contratos.V1.Cancelaciones.Rq;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO;

namespace Naideth.Traslados.Api.Controllers.v1.Extensiones.Cancelaciones.Rq
{
    internal static class CancelacionRequestExtensiones
    { 
        internal static CancelacionDTO ToCrearCancelacionDTO(this CancelacionRequest peticion)
        {
            return new CancelacionDTO(
                peticion.Localizador,
                peticion.Source

                );
        }


    }
}
