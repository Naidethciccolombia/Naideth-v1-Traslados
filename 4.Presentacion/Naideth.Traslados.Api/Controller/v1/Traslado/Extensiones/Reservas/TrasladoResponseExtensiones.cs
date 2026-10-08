

using Microsoft.Extensions.Hosting;
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs;
using Naideth.Traslados.Api.Contratos.V1.Reservas;
using Naideth.Traslados.Api.Contratos.V1.Reservas.Rs;

using Naideth.Traslados.Aplicacion.Reservas.DTO.Rs;
using System.Globalization;

namespace Naideth.Traslados.Api.Controllers.v1.Traslado.Extensiones.Reservas
{
    internal static class TrasladoResponseExtensiones
    {

        internal static TrasladoResponseReserva ToReservaDTO(this ReservaResponseDTO peticion)
        {

            return new TrasladoResponseReserva
            { 
                Localizador=peticion.Localizador,
                Precio = peticion.Precio?.ToPrecioReservaResponseDTO()

            };

        }
          

        internal static PrecioReservaResponse ToPrecioReservaResponseDTO(this PrecioReservaDTO peticion)
        {
            return
                new PrecioReservaResponse
                {
                    Fuente=peticion.Fuente,
                    Total= peticion.Total.ToTotalResponseDTO(),
                    PtcTotal = peticion.PtcTotal.ToPtcTotalResponseDTO()
                };
        }
           

        internal static TotalReservaResponse ToTotalResponseDTO(this TotalReservaDTO hLst)
        {
            return new TotalReservaResponse
            {
                Valor = hLst.Valor.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                Total = hLst.Total.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                Moneda = hLst.Moneda


            };
        }
        internal static List<PTCTotalResevaResponse> ToPtcTotalResponseDTO(this List<PTCTotalResevaDTO> hLst)
        {
            return hLst.Select(t => new PTCTotalResevaResponse
            {
                Total = t.Total.ToTotalResponseDTO(),
                Cantidad = t.Cantidad,
                Ptc = t.Ptc
            }).ToList();
        }
    }
}

