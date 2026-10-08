using Naideth.Traslados.Aplicacion.AppLoggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.AppLoggers.Interfaces
{ 
  public interface IAppLoggerServicio
    {
        void Respuesta(string referencia, Guid id, string evento, double tiempo, int disponibilidades);
        void RespuestaCache(string referencia, Guid id, string evento, double tiempo, int disponibilidades);
        void Fin(string referencia, Guid id, string evento, double tiempo);
        void FinIntegracion(string referencia, Guid id, string evento, string integrador, double tiempo);

        void CredencialesNoEncontradas(string referencia, Guid id, string evento, string integrador);
        void TiempoRepositorio(string referencia, Guid id, string evento, string integrador, double tiempo);
        void NoData(string referencia, Guid id, string evento, string integrador);
        void ErrorLog(string referencia, Guid id, string evento, string integrador, string mensaje);
        void ConfirmacionLog(string referencia, Guid id, string evento, string integrador, string mensaje);
        void FinPeticionGds(string referencia, Guid id, string evento, string integrador,double tiempo, string mensaje);
        void FinProcesamientoGds(string referencia, Guid id, string evento, string integrador,double tiempo, int cantidad);
        void FinPeticionAlternoGds(string referencia, Guid id, Guid idAlterno, string evento, string integrador,  string mensaje);
        void FinPeticionAlternoExtrasGds(string referencia, string id, string evento, string integrador,  string mensaje);
         void FinAlternos(string referencia, string id, string evento, double tiempo);

    }
}  