 
namespace Naideth.Traslados.Aplicacion.Datetime.interfaces
{ 
     public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
        DateTime GetFechaHora();
    }
} 
