using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Naideth.Traslados.Aplicacion.Busquedas.Interfaces;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.Interfaces;
using Newtonsoft.Json.Linq;
namespace Naideth.Traslados.Infrastructura.HostedServicio
{
    internal class HostedServicio : BackgroundService
    { 
        private readonly ILogger<HostedServicio> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICacheRedisServicio _cacheServicio;

        public HostedServicio( IServiceScopeFactory scopeFactory, ICacheRedisServicio cacheServicio, ILogger<HostedServicio> logger)
        {  
            _logger = logger;
            _scopeFactory = scopeFactory;
            _cacheServicio = cacheServicio;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var comunRepositorio = scope.ServiceProvider.GetRequiredService<IComunRepositorio>();
                        //var tavelKitRepositorio = scope.ServiceProvider.GetRequiredService<ITravelKitRepositorio>();


                        await comunRepositorio.CredencialesForCacheAsync(stoppingToken).ConfigureAwait(false);
                        await comunRepositorio.StartListaTRM(stoppingToken).ConfigureAwait(false);



                        const string cacheKey = "CREDENCIAL_TRASLADO";

                        // leer cache global
                        var credencial = await _cacheServicio.GetAsync<List<IntegracionDTO>>(cacheKey, stoppingToken).ConfigureAwait(false);

                          
                        // Esperar 24 horas antes de volver a ejecutar
                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    }



                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al actualizar la caché. Datos estaticos");
                }
            }
        }
    }

}
