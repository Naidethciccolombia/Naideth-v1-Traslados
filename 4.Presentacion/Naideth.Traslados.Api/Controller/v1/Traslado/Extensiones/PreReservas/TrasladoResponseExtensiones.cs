

using Microsoft.Extensions.Hosting;
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs.DTO;
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

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
        internal static List<PaxUpgradeResponse> ToPaxUpgradeResponseDTO(this List<PaxUpgradeDTO> peticion)
        {
            var lista = new List<PaxUpgradeResponse>();

            foreach (var pax in peticion)
            {
                foreach (var edad in pax.edades)
                {

                    lista.Add(new PaxUpgradeResponse
                    {
                        Tipo = pax.tipo,
                        Edades = edad


                    });
                }

            } 

            return lista;
        }
        internal static List<UpgradeResponse> ToUpgradesResponseDTO(this List<UpgradesDTO> peticion)
        {
            return peticion.Select(p => new UpgradeResponse
            { Id = p.id,
                Nombre = p.nombre,
                RangoEdad = p.RangoEdad,
                ValorUnitario = p.ValorUnitario.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                PrecioUnitario = p.PrecioUnitario.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
                PaxUpgrade=p.PaxUpdate.ToPaxUpgradeResponseDTO()??new List<PaxUpgradeResponse>()
            }).ToList();
        } 
        internal static List<TarifaResponse> ToTarifaResponseDTO(this List<TarifaDTO> peticion)
        {
            return peticion.Select(p => new TarifaResponse
            {
                Category = p.Category,
                PdfDescripcion=p.PdfDescripcion,
                Id = p.Id,
                RangoEdad=p.RangoEdad,
                CantidadAdultos = p.CantidadAdultos,
                CantidadNinos = p.CantidadNinos,
                CantidadInfantes = p.CantidadInfantes,
                Nombre = p.Nombre,
                PdfCondicciones = p.PdfCondicciones,
                Beneficios = p.Beneficios?.ToBeneficiosResponseDTO() ?? new List<BeneficiosResponse>(),
                Upgrades = p.Upgrades?.ToUpgradesResponseDTO() ?? new List<UpgradeResponse>(), 

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

