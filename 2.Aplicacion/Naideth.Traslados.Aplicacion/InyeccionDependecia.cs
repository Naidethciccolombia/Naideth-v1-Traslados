using Microsoft.Extensions.DependencyInjection;
using Naideth.Traslados.Aplicacion.AppLoggers;
using Naideth.Traslados.Aplicacion.AppLoggers.Interfaces;
using Naideth.Traslados.Aplicacion.Autenticacion;
using Naideth.Traslados.Aplicacion.Autenticacion.Interfaces;
using Naideth.Traslados.Aplicacion.Autorizaciones;
using Naideth.Traslados.Aplicacion.Autorizaciones.Interfaces;
using Naideth.Traslados.Aplicacion.Busquedas;
using Naideth.Traslados.Aplicacion.Busquedas.Interfaces;
using Naideth.Traslados.Aplicacion.CacheRedis;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Cancelaciones;
using Naideth.Traslados.Aplicacion.Cancelaciones.Interfaces;
using Naideth.Traslados.Aplicacion.Ciudades;
using Naideth.Traslados.Aplicacion.Ciudades.Interfaces;
using Naideth.Traslados.Aplicacion.ConvercionMoneda;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.Interfaces;
using Naideth.Traslados.Aplicacion.Datetime;
using Naideth.Traslados.Aplicacion.Datetime.interfaces;
using Naideth.Traslados.Aplicacion.Emisiones;
using Naideth.Traslados.Aplicacion.Emisiones.Interfaces;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Traslados.Aplicacion.PreReserva;
using Naideth.Traslados.Aplicacion.PreReservas.Interfaces;
using Naideth.Traslados.Aplicacion.Reservas;
using Naideth.Traslados.Aplicacion.Reservas.Interfaces;
using Naideth.Traslados.Infrastructura.HostedServicio;
using Naideth.Central.Dominio.Servicios;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours;
namespace Naideth.Traslados.Aplicacion
{
    public static class InyeccionDependecia
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection servicios)
        {
            // ✅ INFRAESTRUCTURA (Singleton - sin estado)
            servicios.AddSingleton<IDateTimeProvider, DatetimeProvider>();
            servicios.AddSingleton<ICacheRedisServicio, CacheRedisServicio>(); 

            // ✅ NEGOCIO (Scoped - estado por petición)
            servicios.AddScoped<ITenanizacionServicio, TenanizacionServicio>(); 
            servicios.AddScoped<ICiudadServicio, CiudadServicio>();    
            servicios.AddScoped<IAutenticacionServicio, AutenticacionServicio>(); 
            //servicios.AddScoped<IConvercionMonedaServicio, ConvercionMonedaServicio>(); 
            servicios.AddScoped<MetodosComunes>();
            servicios.AddScoped<IAppLoggerServicio, AppLoogerServicio>(); 
            servicios.AddScoped<IAppLoggerServicio, AppLoogerServicio>(); 
            servicios.AddScoped<IConvercionMonedaServicio, ConvercionMonedaServicio>(); 
            servicios.AddScoped<IBusquedaServicio, BusquedaServicio>();
            servicios.AddScoped<IPreReservaServicio, PreReservaServicio>();
            servicios.AddScoped<IReservaServicio, ReservaServicio>();
            servicios.AddScoped<ICancelacionServicio, CancelacionServicio>();
            servicios.AddScoped<IEmisionServicio, EmisionServicio>();
            servicios.AddScoped<IINeedToursServicio, INeedToursServicio>();

            // ✅ INTEGRADORES (Scoped)
            //      servicios.AddScoped<ICopaServicio, CopaServicio>();

            // ✅ BACKGROUND (Singleton por naturaleza)
            servicios.AddHostedService<HostedServicio>();

            return servicios;
        }
    }
}
