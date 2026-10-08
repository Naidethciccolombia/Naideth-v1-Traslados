
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Naideth.Traslados.Aplicacion.Busquedas.Interfaces;
using Naideth.Traslados.Aplicacion.Cancelaciones.Interfaces;
using Naideth.Traslados.Aplicacion.Ciudades.Interfaces;
using Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.ConexionesExternas.Interfaces;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.Interfaces;
using Naideth.Traslados.Aplicacion.PreReservas.Interfaces;
using Naideth.Traslados.Aplicacion.Reservas.Interfaces;
using Naideth.Traslados.Aplicacion.Usuarios.Interfaces;
using Naideth.Traslados.Aplicacion.UsuariosRoles.Interfaces;
using Naideth.Traslados.Aplicacion.UsuariosTenant.Interface;
using Naideth.Traslados.Infrastructura.AccesoDatos;
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces; 
using Naideth.Traslados.Infrastructura.Ciudades;
using Naideth.Traslados.Infrastructura.Comunes;
using Naideth.Traslados.Infrastructura.ConexionesExternas;
using Naideth.Traslados.Infrastructura.ConvercionMoneda; 
using Naideth.Traslados.Infrastructura.INeedTours;
using Naideth.Traslados.Infrastructura.Usuarios;
using Naideth.Traslados.Infrastructura.UsuariosRoles;
using Naideth.Traslados.Infrastructura.UsuariosTenant;
using Naideth.Vuelos.Aplicacion.Kernel.Interfaces;
using System.Net;
using System.Net.Http.Headers;

namespace Naideth.Traslados.Infrastructura
{
    public static class InyeccionDependecia
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection servicios, IConfiguration configuration)
        {
             
            servicios.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DbConexion"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            servicios.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
              

            servicios.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>(); 
            servicios.AddScoped<IUsuarioRolesRepositorio, UsuarioRolesRepositorio>(); 
            servicios.AddScoped<IUsuarioTenantRepositorio, UsuarioTenantRepositorio>(); 
            servicios.AddScoped<ICiudadRepositorio, CiudadRepositorio>();

              servicios.AddScoped<IINeedToursRepositorio, INeedToursRepositorio>();

                  servicios.AddScoped<IConvercionMonedaRepositorio, ConvercionMonedaRepositorio>(); 
                  servicios.AddScoped<IComunRepositorio, ComunRepositorio>();  
             

            servicios.AddHttpClient<IConexionesExternasRepositorio, ConexionesExternasRepositorio>()
                    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        PooledConnectionLifetime = TimeSpan.FromMinutes(2),  // Mantiene conexiones hasta 5 min
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1), // Cierra conexiones inactivas tras 1 min
                        MaxConnectionsPerServer = 50, // Límite de conexiones simultáneas
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30), // Mantiene la conexión activa
                        KeepAlivePingDelay = TimeSpan.FromSeconds(15), // Intervalo antes del primer ping
                    });

            return servicios;
        }
    }
}
