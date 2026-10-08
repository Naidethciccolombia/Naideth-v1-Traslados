using Microsoft.Extensions.Logging;
using Naideth.Central.Dominio.Integradores;
using Naideth.Central.Dominio.Proveedores; 
using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces; 
using NPOI.POIFS.Crypt.Dsig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.AppLoggers
{ 
 public class AppLoogerServicio : IAppLoggerServicio
    {
        private readonly ILogger<AppLoogerServicio> _logger;
        private static readonly TimeZoneInfo ColombiaTimeZone =TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

        public AppLoogerServicio(ILogger<AppLoogerServicio> logger)
        {
            _logger = logger; 
        }

        public static DateTime AhoraColombia()
        {
            //return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ColombiaTimeZone);
            return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }
        public void Fin(string referencia, Guid id, string evento, double tiempo)
        {
            AppLooger.Fin(_logger, referencia, id, evento, tiempo, AhoraColombia());
        }
        public void FinIntegracion(string referencia, Guid id, string evento, string integrador, double tiempo)
        {
            AppLooger.FinIntegracion(_logger, referencia, id, evento, integrador, tiempo, AhoraColombia());
        }
        public void CredencialesNoEncontradas(string referencia, Guid id, string evento, string integrador)
        {

            AppLooger.CredencialesNoEncontradas(_logger, referencia, id, evento, integrador, AhoraColombia());
        }
        public void NoData(string referencia, Guid id, string evento, string integrador)
        {

            AppLooger.NoData(_logger, referencia, id, evento, integrador, AhoraColombia());
        }
         
        public void TiempoRepositorio(string referencia, Guid id, string evento, string integrador, double tiempo)
        {
            AppLooger.TiempoRepositorio(_logger, referencia, id, evento, integrador, tiempo, AhoraColombia());
        }

        public void ConfirmacionLog(string referencia, Guid id, string evento, string integrador,  string mensaje)
        {
            AppLooger.ConfirmacionLog(_logger, referencia, id, evento, integrador,   mensaje, AhoraColombia());
        }


        public void FinPeticionGds(string referencia, Guid id, string evento, string integrador, double tiempo,  string mensaje)
        {
            AppLooger.FinPeticionGds(_logger, referencia, id, evento, integrador, tiempo, mensaje, AhoraColombia());
        }
        public void FinProcesamientoGds(string referencia, Guid id, string evento, string integrador, double tiempo,  int cantidad)
        {
            AppLooger.FinProcesamientoGds(_logger, referencia, id, evento, integrador, tiempo, cantidad, AhoraColombia());
        }
        public void ErrorLog(string referencia, Guid id, string evento, string integrador,  string mensaje)
        {
            AppLooger.ErrorLog(_logger, referencia, id, evento, integrador,  mensaje, AhoraColombia());
        }
        public void FinPeticionAlternoGds(string referencia, Guid id, Guid idAlterno, string evento, string integrador,   string mensaje)
        {
            AppLooger.FinPeticionAlternoGds(_logger, referencia, id, idAlterno, evento, integrador, mensaje, AhoraColombia());
        }
        public void FinAlternos(string referencia, string id, string evento, double tiempo)
        {
            AppLooger.FinAlternos(_logger, referencia, id, evento, tiempo, AhoraColombia());
        }

        public void FinPeticionAlternoExtrasGds(string referencia, string id, string evento, string integrador, string mensaje)
        {
            AppLooger.FinPeticionAlternosExtrasGds(_logger, referencia, id, evento, integrador, mensaje, AhoraColombia());
        }

        public void Respuesta(string referencia, Guid id, string evento, double tiempo, int disponibilidades)
        {
            AppLooger.Respuesta(_logger, referencia, id, evento, tiempo, disponibilidades, AhoraColombia());
        }

        public void RespuestaCache(string referencia, Guid id, string evento, double tiempo, int disponibilidades)
        {
            AppLooger.RespuestaCache(_logger, referencia, id, evento, tiempo, disponibilidades, AhoraColombia());
        }
    }
}
