using Microsoft.Extensions.Logging; 

namespace Naideth.Traslados.Aplicacion.AppLoggers
{ 
    public static partial class AppLooger
    {
        
        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | TiempoMs={Tiempo}| Fin General")]
        public static partial void Fin(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            double tiempo,
            DateTime hora);


        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | TiempoMs={Tiempo}| Respuesta | Disponibilidades:{Disponibilidades}")]
        public static partial void Respuesta(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            double tiempo,
            int disponibilidades,
            DateTime hora);

        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | TiempoMs={Tiempo}| Respuesta desde Cache | Disponibilidades:{Disponibilidades}")]
        public static partial void RespuestaCache(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            double tiempo,
            int disponibilidades,
            DateTime hora);

        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | TiempoMs={Tiempo} | Fin Integracion")]
        public static partial void FinIntegracion(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            string integrador,
            double tiempo,
            DateTime hora);


        [LoggerMessage(
            EventId = 1002,
            Level = LogLevel.Warning,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | Credenciales No Encontradas")]
        public static partial void CredencialesNoEncontradas(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            string integrador,
            DateTime hora);

        [LoggerMessage(
            EventId = 1002,
            Level = LogLevel.Warning,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | No hay Datos")]
        public static partial void NoData(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            string integrador,
            DateTime hora);


        [LoggerMessage(
            EventId = 1003,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | Tiempo Repositorio General | TiempoMs={Tiempo} ")]
        public static partial void TiempoRepositorio(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            string integrador,
            double tiempo,
            DateTime hora);

        [LoggerMessage(
            EventId = 1004,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | Confirmacion de Log:{mensaje}")]
        public static partial void ConfirmacionLog(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            string integrador, 
            string mensaje,
            DateTime hora);

        [LoggerMessage(
            EventId = 1006,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | TiempoMs={Tiempo} | Envio peticion a Gds:{mensaje}")]
        public static partial void FinPeticionGds(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            string integrador,
            double tiempo,
            string mensaje,
            DateTime hora);

        [LoggerMessage(
            EventId = 1006,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | Procesamiento | TiempoMs={Tiempo} | Cantidad={Cantidad}")]
        public static partial void FinProcesamientoGds(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            string integrador,
            double tiempo,
            int cantidad,
            DateTime hora);

        [LoggerMessage(
            EventId = 1007,
            Level = LogLevel.Warning,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | Error de Log:{mensaje}")]
        public static partial void ErrorLog(
            ILogger logger,
            string referencia,
            Guid id,
            string evento,
            string integrador, 
            string mensaje,
            DateTime hora);



        [LoggerMessage(
            EventId = 1009,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | IdAlterno={IdAlterno} | Evento={Evento} | Integrador={Integrador}  | Envio peticion a Gds:{mensaje}")]
        public static partial void FinPeticionAlternoGds(
            ILogger logger,
            string referencia,
            Guid id,
            Guid idAlterno,
            string evento,
            string integrador, 
            string mensaje,
            DateTime hora);


        [LoggerMessage(
            EventId = 1011,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | TiempoMs={Tiempo} | Fin General")]
        public static partial void FinAlternos(
            ILogger logger,
            string referencia,
            string id,
            string evento,
            double tiempo,
            DateTime hora);



        [LoggerMessage(
            EventId = 1013,
            Level = LogLevel.Information,
            Message = "Timestamp:{hora} | Referencia={Referencia} | Id={Id} | Evento={Evento} | Integrador={Integrador} | {mensaje}")]
        public static partial void FinPeticionAlternosExtrasGds(
            ILogger logger,
            string referencia,
            string id,
            string evento,
            string integrador,
            string mensaje,
            DateTime hora);
    }

}
