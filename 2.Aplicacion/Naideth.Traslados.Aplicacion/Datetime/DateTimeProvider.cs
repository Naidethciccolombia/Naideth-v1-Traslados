using Naideth.Traslados.Aplicacion.Datetime.interfaces; 

namespace Naideth.Traslados.Aplicacion.Datetime
{
    public sealed class DatetimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
        public DateTime GetFechaHora()
        {
            //TimeZoneInfo utcMinusFiveZone = TimeZoneInfo.CreateCustomTimeZone("UTC-05:00", new TimeSpan(-5, 0, 0), "UTC-05:00", "UTC-05:00");
            //DateTime utcMinusFiveTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, utcMinusFiveZone);
            return Now;
        }

    }
}
