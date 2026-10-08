

using Naideth.Traslados.Api.Contratos.V1.Emisiones.Rs;
using Naideth.Traslados.Aplicacion.Emisiones.DTO.Rs;

namespace Naideth.Traslados.Api.Controllers.v1.Extensiones.Emisiones.Rs
{
    internal static class EmisionResponseExtensiones
    {

        internal static EmisionResponse ToEmisionDTO(this EmisionResponseDTO? hLst)
        {
            return new EmisionResponse
            {
                Estado = hLst.Estado.ToString()
             ,Mensaje = hLst.Mensaje
             ,Source = hLst.Source,
                Voucher= hLst.Vouchers,
                Localizador = hLst.Localizador
            };
        }


    }

}
