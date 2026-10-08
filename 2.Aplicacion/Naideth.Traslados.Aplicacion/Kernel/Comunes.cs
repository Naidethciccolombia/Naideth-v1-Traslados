using Microsoft.Extensions.Logging;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rs;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.Interfaces;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rs;
using Naideth.Traslados.Dominio.Kernel.Exepciones;
using Naideth.Traslados.Dominio.Kernel.Extensiones;
using Naideth.Central.Dominio.Monedas;
using Naideth.Central.Dominio.RangoEdades;
using Naideth.Central.Dominio.TipoMaletas;  
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.Kernel
{
    public sealed class MetodosComunes
    {

        private readonly IConvercionMonedaServicio _convercionMonedaServicio;
        private readonly ILogger<MetodosComunes> _logger;
        public MetodosComunes(IConvercionMonedaServicio convercionMonedaServicio, ILogger<MetodosComunes> logger)
        {
            _convercionMonedaServicio = convercionMonedaServicio;
            _logger = logger;
        }

       



        public async Task<List<DisponibilidadDTO>> FormateadoTariff(
           List<DisponibilidadDTO>? disponibilidades, 
            string monedaDestino,
            decimal markUps,
            CancellationToken tokenCancelacion)
        { 

            if (disponibilidades == null)
                return null;


            var lista = new List<DisponibilidadDTO>();
            
            foreach(var disp in disponibilidades)
            {
                var dispConvertida = await ConvertirDisponibilidad(disp, monedaDestino, markUps, tokenCancelacion).ConfigureAwait(false);
                lista.Add(dispConvertida);
            } 

            return lista;
        }

        public async Task<ReservaResponseDTO> FormateadoTariffReserva(
           ReservaResponseDTO disponibilidad, 
            string monedaDestino,
            decimal markUps,
            CancellationToken tokenCancelacion)
        {
            var precio = disponibilidad.Precio;

            var origenMoneda = disponibilidad.Precio.Total.Moneda;

            var PtcTotal=new List<PTCTotalResevaDTO>();

            foreach (var ptc in disponibilidad.Precio.PtcTotal)
            {
                var valor = _convercionMonedaServicio.ConvertirMoneda(ptc.Total.Valor, origenMoneda, monedaDestino);
                var impuesto = _convercionMonedaServicio.ConvertirMoneda(ptc.Total.Impuesto, origenMoneda, monedaDestino);
                var total = AplicarMarkUp(valor, markUps);

                PtcTotal.Add(new PTCTotalResevaDTO(ptc.Ptc, ptc.Cantidad, new TotalReservaDTO(monedaDestino, valor, impuesto, total+impuesto)));
            }

            var sumValor = PtcTotal.Sum(x => x.Total.Valor);
            var sumImpuesto = PtcTotal.Sum(x => x.Total.Impuesto);
            var sumTotal = PtcTotal.Sum(x => x.Total.Total);

              
            var precios = new PrecioReservaDTO(precio.Fuente, new TotalReservaDTO(monedaDestino, sumValor, sumImpuesto, sumTotal+ sumImpuesto), PtcTotal);


            return new ReservaResponseDTO(disponibilidad.Estado, disponibilidad.Localizador, precios);
             
        }

        private async Task<DisponibilidadDTO> ConvertirDisponibilidad(
       DisponibilidadDTO? disponibilidad,
      string monedaDestino, 
      decimal markUps,
      CancellationToken tokenCancelacion)
        {
            if (disponibilidad is null)
                throw new ExepcionConvitiendoTrm();

            var origenMoneda = disponibilidad.Precio.Total.Moneda;

            // ============================================
            // 1. CONVERSIÓN GENERAL TOTAL
            // ============================================
            var baseTotal = _convercionMonedaServicio.ConvertirMoneda(disponibilidad.Precio.Total.Valor, origenMoneda, monedaDestino);
            var impTotal = _convercionMonedaServicio.ConvertirMoneda(disponibilidad.Precio.Total.Impuesto, origenMoneda, monedaDestino);
            var total = AplicarMarkUp(baseTotal, markUps);
            // Total general convertido

            // ============================================
            // 2. CONVERSIÓN DE PTC  
            // ============================================
             

            var ptcListaConvertida = disponibilidad.Precio.PtcTotal.Select(item =>
            {
              //  var monedaOrigPtc = item.Total.Moneda;

                var baseConvertida = _convercionMonedaServicio.ConvertirMoneda(item.Total.Valor, origenMoneda, monedaDestino);
                var impConvertida = _convercionMonedaServicio.ConvertirMoneda(item.Total.Impuesto, origenMoneda, monedaDestino);
                var totalConvertido = AplicarMarkUp(baseConvertida, markUps);

                return new PTCTotalDTO(
                    item.Ptc switch
                    {
                        "PFA" or "ADT" => "ADT",
                        "CNN" or "CHD" => "CHD",
                        "INF" => "INF",
                        _ => item.Ptc // fallback por seguridad
                    },
                    item.Cantidad,
                    new TotalDTO(
                        monedaDestino,
                        baseConvertida,
                        impConvertida,
                        totalConvertido+ impConvertida
                    )
                );
            });
              

            var totalFinal = new TotalDTO(
                monedaDestino,
                baseTotal, 
                impTotal,
                total+ impTotal
            );

            var tarifas = new List<TarifaDTO>();
            foreach (var pl in disponibilidad.Tarifas)
            {
                var upgrades = pl.Upgrades.Select(item =>
                {
                    var baseConvertida = _convercionMonedaServicio.ConvertirMoneda(item.ValorUnitario, origenMoneda, monedaDestino);
                    var totalConvertido = AplicarMarkUp(baseConvertida, markUps);
                    return new UpgradesDTO(item.id, item.nombre, item.RangoEdad, baseConvertida, totalConvertido, item.PaxUpdate);
                }).ToList();

                tarifas.Add(new TarifaDTO(pl.Id, pl.IdPreReserva, pl.Category, pl.PdfCondicciones, pl.PdfDescripcion, pl.Nombre, pl.Beneficios, pl.RangoEdad, pl.CantidadAdultos, pl.CantidadNinos, pl.CantidadInfantes, upgrades));
            }


            return new DisponibilidadDTO(disponibilidad.Source,
                disponibilidad.SourceName,
                disponibilidad.Id,
                disponibilidad.Auxiliares,
                disponibilidad.Plan,
                disponibilidad.Duracion,
                disponibilidad.Tipo,
                disponibilidad.FechaInicio,
                disponibilidad.FechaSalida,
                disponibilidad.Imagen,
                new PrecioDTO(totalFinal, ptcListaConvertida.ToList()),
                tarifas
            );

        }


        public decimal AplicarMarkUp(decimal valor, decimal markUps)
        {
            return ((valor / ((100 - markUps)/100)));
        }


    }
}
