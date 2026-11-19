

using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using System.Globalization;

namespace Naideth.Traslados.Api.Controllers.v1.Traslado.Extensiones.PreReservas
{
    internal static class TrasladoResponseExtensiones
    {

        internal static PreReservaResponse ToPreReservaResponseDTO(this TrasladoPreReservaRsDTO peticion)
        {

            return new PreReservaResponse
            {
                IdPreReserva = peticion.IdPreReserva,
                Disponibilidad=peticion.Disponibilidad?.ToDisponibilidadPreReservaResponseDTO()

            };

        }
          

        internal static DisponibilidadPreReservaResponse ToDisponibilidadPreReservaResponseDTO(this DisponibilidadDTO peticion)
        {
            return  
                new DisponibilidadPreReservaResponse
                {
                    Id = peticion.Id,
                    Plan = peticion.Plan, 
                    Duracion = peticion.Duracion,
                    Tipo = peticion.Tipo,
                    FechaInicio = peticion.FechaInicio.ToString("yyyy-MM-dd"),
                    FechaSalida = peticion.FechaSalida.ToString("yyyy-MM-dd"),
                    HoraInicio = peticion.HoraInicio ?? string.Empty,
                    HoraSalida = peticion.HoraSalida ?? string.Empty,
                    Fuente= peticion.Source,
                    Imagen = peticion.Imagen,
                    Precio = peticion.Precio?.ToPrecioPreReservaResponseDTO() ?? new PrecioPreReservaResponse(),
                    Tarifas = peticion.Tarifas?.ToTarifaResponseDTO() ?? new List<TarifaResponse>()

                };
        }

        internal static PrecioPreReservaResponse ToPrecioPreReservaResponseDTO(this PrecioDTO peticion)
        {
            return new PrecioPreReservaResponse
            {
                Total = peticion.Total.ToTotalResponseDTO(),
                PtcTotal = peticion.PtcTotal.ToPtcTotalResponseDTO(),
            };
        }
        internal static List<BeneficiosResponse> ToBeneficiosResponseDTO(this List<BeneficiosDTO> peticion)
        {
            return peticion.Select(p => new BeneficiosResponse
            {
                Descripcion = p.Descripcion,
                Nombre = p.Nombre,
                Tipo = p.Tipo
            }).ToList();
        }
        internal static List<TarifaResponse> ToTarifaResponseDTO(this List<TarifaDTO> peticion)
        {
            return peticion.Select(p => new TarifaResponse
            {
                Category = p.Category,
                Id = p.Id,
                RangoEdad=p.RangoEdad,
                CantidadAdultos = p.CantidadAdultos,
                CantidadNinos = p.CantidadNinos,
                CantidadInfantes = p.CantidadInfantes,
                Nombre = p.Nombre,
                Beneficios = p.Beneficios?.ToBeneficiosResponseDTO() ?? new List<BeneficiosResponse>()

            }).ToList();
        }


        internal static TotalPreReservaResponse ToTotalResponseDTO(this TotalDTO hLst)
        {
            return new TotalPreReservaResponse
            {
                Valor = hLst.Valor.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                Impuesto = hLst.Impuesto.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                Total = hLst.Total.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                Moneda = hLst.Moneda


            };
        }
        internal static List<PTCTotalPreReservaResponse> ToPtcTotalResponseDTO(this List<PTCTotalDTO> hLst)
        {
            return hLst.Select(t => new PTCTotalPreReservaResponse
            {
                Total = t.Total.ToTotalResponseDTO(),
                Cantidad = t.Cantidad,
                Ptc = t.Ptc
            }).ToList();
        }
    }
}

