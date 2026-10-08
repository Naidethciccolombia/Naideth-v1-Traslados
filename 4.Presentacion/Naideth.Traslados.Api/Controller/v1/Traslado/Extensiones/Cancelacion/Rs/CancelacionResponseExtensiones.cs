

using Naideth.Traslados.Api.Contratos.V1.Cancelacion.Rs;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs;

namespace Naideth.Traslados.Api.Controllers.v1.Extensiones.Cancelaciones.Rs
{
    internal static class CancelacionResponseExtensiones
    {

        internal static CancelacionResponse ToCancelacionDTO(this CancelacionResponseDTO? hLst)
        {
            return new CancelacionResponse
            {
                Estado = hLst.Estado.ToString()
             ,Mensaje = hLst.Mensaje
             ,Source = hLst.Source,
                Localizador = hLst.Localizador
            };
        }


    }

}
