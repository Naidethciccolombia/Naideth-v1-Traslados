

using Microsoft.Extensions.Hosting;
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Controllers.v1.Traslado.Extensiones.Busquedas
{
    internal static class TrasladoResponseExtensiones
    {

        internal static BusquedaResponse ToBusquedaResponseDTO(this TrasladoRsDTO peticion)
        {

            return new BusquedaResponse
            {
                Filtros = peticion.Filtros?.ToFiltroResponseDTO(),
                EsParcial = peticion.EsParcial,
                IdBusqueda = peticion.IdBusqueda,
                Pagina = peticion.Pagina,
                RegistrosPagina = peticion.RegistrosPagina,
                Disponibilidades = peticion.Disponibilidades.ToDisponibilidadResponseDTO(),
                Tiempo = peticion.Tiempo,
                TotalItems = peticion.TotalItems,
                IntegradoresStatus = peticion.IntegradoresStatus?.ToIntegradorStatusResponseDTO()

            };

        }
      
        internal static List<IntegradorStatusResponse> ToIntegradorStatusResponseDTO(this List<IntegradorStatusDTO> hLst)
        {
            return hLst?.Select(t => new IntegradorStatusResponse
            {
                Nombre = t.Nombre,
                Estado = t.Estado,
                TiempoRespuesta = t.TiempoRespuesta,
                CantidadDisponibilidades = t.CantidadDisponibilidades,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin,
                MensajeError = t.MensajeError
            }).ToList();
        }

        internal static FiltroResponse ToFiltroResponseDTO(this FiltroResponseDTO peticion)
        {
            return
            new FiltroResponse
            {
                Precios = peticion.precios?.ToPrecioResponseDTO() ?? new FiltroPrecioResponse(),
                Fuentes = peticion.fuentes?.ToGeneralResponseDTO() ?? new List<FiltroGeneralResponse>(),
                Ordenamientos = peticion.ordenamientos?.ToGeneralResponseDTO() ?? new List<FiltroGeneralResponse>(),
                Nombres = peticion.nombres?.ToGeneralResponseDTO() ?? new List<FiltroGeneralResponse>()
            };


        }

        internal static FiltroPrecioResponse ToPrecioResponseDTO(this FiltroPrecioResponseDTO fil)
        {
            return new FiltroPrecioResponse
            {
                minimo = (Math.Floor(fil.minimo) - 1).ToString("N0").Replace(",", "").Replace(".", ""),
                maximo = (Math.Ceiling(fil.maximo) + 1).ToString("N0").Replace(",", "").Replace(".", ""),
            };
        }

        internal static List<FiltroGeneralResponse> ToGeneralResponseDTO(this List<FiltroGeneralResponseDTO> hLst)
        {
            return hLst?.Select(general => new FiltroGeneralResponse
            {
                Codigo = general.codigo,
                Nombre = general.nombre,
                Sel = general.sel
            }).ToList() ?? new List<FiltroGeneralResponse>();
        }

        internal static List<DisponibilidadResponse> ToDisponibilidadResponseDTO(this List<DisponibilidadDTO> peticion)
        {
            return peticion.Select(p =>
                new DisponibilidadResponse
                {
                    Id = p.Id,
                    Plan = p.Plan, 
                    Duracion = p.Duracion,
                    Tipo = p.Tipo,
                    FechaInicio = p.FechaInicio.ToString("yyyy-MM-dd"),
                    FechaSalida = p.FechaSalida.ToString("yyyy-MM-dd"),
                    Imagen = p.Imagen,
                    Precio = p.Precio?.ToPrecioResponseDTO() ?? new PrecioResponse(),
                    Fuente = p.Source,
                    // Tarifas = p.Tarifas?.ToTarifaResponseDTO() ?? new List<TarifaResponse>()

                }).ToList();
        }

        internal static PrecioResponse ToPrecioResponseDTO(this PrecioDTO peticion)
        {
            return new PrecioResponse
            {
                Total = peticion.Total.ToTotalResponseDTO(),
                PtcTotal = peticion.PtcTotal.ToPtcTotalResponseDTO(),
            };
        }
    

        internal static TotalResponse ToTotalResponseDTO(this TotalDTO hLst)
        {
            return new TotalResponse
            {
                Valor = hLst.Valor.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                Impuesto = hLst.Impuesto.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                Total = hLst.Total.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                Moneda = hLst.Moneda


            };
        }
        internal static List<PTCTotalResponse> ToPtcTotalResponseDTO(this List<PTCTotalDTO> hLst)
        {
            return hLst.Select(t => new PTCTotalResponse
            {
                Total = t.Total.ToTotalResponseDTO(),
                Cantidad = t.Cantidad,
                Ptc = t.Ptc
            }).ToList();
        }
    }
}

