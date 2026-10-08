
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.CacheRedis.DTO;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using NPOI.SS.Formula.Functions;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Diagnostics; 

namespace Naideth.Traslados.Aplicacion.CacheRedis
{
    public sealed class CacheRedisServicio : ICacheRedisServicio
    { 
        private readonly IDatabase _redis;
        private readonly IDatabase _redisReserva;
        private readonly List<string> _cacheKeys = new List<string>();
        private readonly ILogger<CacheRedisServicio> _logger;
        private readonly ConcurrentDictionary<string, long> _cacheSizes = new();
        private long _totalCacheSize = 0;
        private readonly IConnectionMultiplexer _redisMultiplexer;
        private readonly object _lock = new();


        public CacheRedisServicio(
            ILogger<CacheRedisServicio> logger,
            IConnectionMultiplexer redisMultiplexer,
            IConfiguration config)
        {
            _redisMultiplexer = redisMultiplexer; 
             
            var defaultDb = config.GetValue<int>("Redis:DefaultDb");
            var reservaDb = config.GetValue<int>("Redis:ReservaDb");

            _redis = redisMultiplexer.GetDatabase(defaultDb);
            _redisReserva = redisMultiplexer.GetDatabase(reservaDb);

            _logger = logger;
        }

        public async Task SetReservaAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
        {

            await SetAsync(key, value, expiration, cancellationToken).ConfigureAwait(false);
             
        }

        public async Task SetAsync<T>(
          string key,
          T value,
          TimeSpan expiration, CancellationToken cancellationToken)
        {
            var data = ExtensionesMessagePack.Serialize(value);

            // 100KB límite
            const int maxChunkSize = 100 * 1024;
           

            // OBJETO GRANDE
            var chunks = data
                .Chunk(maxChunkSize)
                .ToList();

            var meta = new RedisChunkMetadata
            {
                TotalChunks = chunks.Count,
                TotalSize = data.Length,
                TypeName = typeof(T).FullName!,
                IsChunked = true
            };

            // guardar metadata
            await _redis.StringSetAsync(
                $"{key}:meta",
                ExtensionesMessagePack.Serialize(meta),
                expiration);

            // guardar chunks
            var batch = _redis.CreateBatch();

            for (int i = 0; i < chunks.Count; i++)
            {
                batch.StringSetAsync(
                    $"{key}:chunk:{i}",
                    chunks[i].ToArray(),
                    expiration);
            }

            batch.Execute();
        }
       
        public async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            try
            {
                // buscar metadata chunked
                var metaValue = await _redis.StringGetAsync($"{key}:meta");

                if (metaValue.IsNullOrEmpty)
                    return default;

                var meta = ExtensionesMessagePack
                    .Deserialize<RedisChunkMetadata>(metaValue);

                var ms = new MemoryStream();

                for (int i = 0; i < meta.TotalChunks; i++)
                {
                    try
                    {
                        var chunk = await _redis.StringGetAsync(
                            $"{key}:chunk:{i}");

                        if (chunk.IsNullOrEmpty)
                            return default;

                        var bytes = (byte[])chunk;

                        await ms.WriteAsync(bytes);
                    }
                    catch (Exception e)
                    {

                    }
                }

       
                try
                {
                    return ExtensionesMessagePack.Deserialize<T>(ms.ToArray());
                }
                catch
                {
                    return default;
                }

            }
            catch
            {
                return default;
            }
        }

        private static string ExtractInbound(string message)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, @"inbound=([^,]+)");
            return match.Success ? match.Groups[1].Value : "N/A";
        }
        public async ValueTask<T?> GetReservaAsync<T>(string key, CancellationToken cancellationToken)
        {
            const int maxRetries = 3;
            var retryDelays = new[] { 500, 2000, 3000 }; // Backoff: 500ms, 2s, 3s - MÁS TIEMPO para valores grandes

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // ⚠️ TIMEOUT EXPLÍCITO de 25 segundos para valores grandes
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(25));

                    var value = await _redisReserva.StringGetAsync(key).ConfigureAwait(false);

                    if (value.IsNullOrEmpty)
                        return default;

                    return ExtensionesMessagePack.Deserialize<T>(value);
                }
                catch (RedisTimeoutException ex) when (attempt < maxRetries)
                {
                    var delay = retryDelays[attempt];
                    _logger.LogWarning(ex, 
                        "Timeout obteniendo cache de reserva para key: {Key}. Intento {Attempt}/{MaxRetries}. Reintentando en {Delay}ms...", 
                        key, attempt + 1, maxRetries, delay);

                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (RedisTimeoutException ex)
                {
                    _logger.LogError(ex, "Timeout definitivo obteniendo cache de reserva para key: {Key} después de {MaxRetries} intentos", key, maxRetries);
                    return default;
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex, "Operación de reserva cancelada para key: {Key} en intento {Attempt}", key, attempt + 1);
                    return default;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error obteniendo cache de reserva para key: {Key}", key);
                    return default;
                }
            }

            return default;
        }

        public async Task AppendAsync(string key, List<DisponibilidadDTO> nuevas, TimeSpan expiration)
        {
            if (nuevas == null || nuevas.Count == 0)
                return;

            var cache = await GetAsync<CacheDTO>(key, CancellationToken.None).ConfigureAwait(false);

            if (cache == null || nuevas.Count == 0)
                return;

            cache.disponibilidades.AddRange(nuevas);

            await SetAsync(key, cache, expiration, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task RemoveAsync(
    string key,
    CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                 
                // buscar metadata chunked
                var metaKey = $"{key}:meta";

                var metaValue = await _redis.StringGetAsync(metaKey);

                if (metaValue.IsNullOrEmpty)
                    return;

                var meta = ExtensionesMessagePack
                    .Deserialize<RedisChunkMetadata>(metaValue);

                var keys = new List<RedisKey>{metaKey};

                for (int i = 0; i < meta.TotalChunks; i++)
                { 
                    keys.Add($"{key}:chunk:{i}");
                }

                await _redis.KeyDeleteAsync(keys.ToArray());
            }
            catch
            {

            }
        }
        public async Task UpdateEstadoAsync(
            string key,
            bool esParcial,
            TimeSpan expiration)
        {
            try
            {
                var cache = await GetAsync<CacheDTO>(key, CancellationToken.None).ConfigureAwait(false);

                if (cache == null)
                    return;

                await SetAsync(
                    key,
                    new CacheDTO(cache.disponibilidades, cache.idBusqueda, esParcial, cache.integradoresStatus),
                    expiration,
                    CancellationToken.None
                ).ConfigureAwait(false);
            }
            finally
            {
            }
        }

        public async Task UpdateIntegradoresStatusAsync(
            string key,
            List<IntegradorStatusDTO> integradoresStatus,
            TimeSpan expiration)
        {
            try
            {
                var cache = await GetAsync<CacheDTO>(key, CancellationToken.None).ConfigureAwait(false);

                if (cache == null)
                    return;

                await SetAsync(
                    key,
                    new CacheDTO(cache.disponibilidades, cache.idBusqueda, cache.esParcial, integradoresStatus),
                    expiration,
                    CancellationToken.None
                ).ConfigureAwait(false);
            }
            finally
            {
            }
        }


        public async Task<bool> TryAcquireLockReservaAsync(string key, TimeSpan expiration)
        {
            return await _redisReserva.StringSetAsync(key, "1", expiration, When.NotExists).ConfigureAwait(false);
        }

        public async Task ReleaseLockReservaAsync(string key)
        {
            await _redisReserva.KeyDeleteAsync(key).ConfigureAwait(false);
        }

        public async Task<int> IncrementReservaAsync(string key, TimeSpan expiration)
        {
            var count = await _redisReserva.StringIncrementAsync(key).ConfigureAwait(false);

            if (count == 1)
            {
                await _redisReserva.KeyExpireAsync(key, expiration).ConfigureAwait(false);
            }

            return (int)count;
        }

        public async Task UpsertAsync<T>(string key, T value, TimeSpan expiration, CancellationToken TokenCancelacion)
        {
            // Optimización: StringSetAsync con XX (solo si existe) o NX (solo si no existe) 
            // es más eficiente que Get → Remove → Set
            // Simplemente sobrescribimos el valor directamente
            await SetAsync(key, value, expiration, TokenCancelacion).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifica la salud de la conexión Redis mediante un PING
        /// </summary>
        public async Task<(bool IsHealthy, TimeSpan Latency, string Status)> CheckHealthAsync()
        {
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await _redis.PingAsync().ConfigureAwait(false);
                sw.Stop();

                var isConnected = _redisMultiplexer.IsConnected;
                var endpoints = _redisMultiplexer.GetEndPoints();
                var status = $"Connected: {isConnected}, Endpoints: {string.Join(", ", endpoints.Select(e => e.ToString()))}";

                if (sw.Elapsed.TotalMilliseconds > 1000)
                {
                    _logger.LogWarning("Redis PING lento: {Latency}ms - {Status}", sw.Elapsed.TotalMilliseconds, status);
                }

                return (isConnected, sw.Elapsed, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando salud de Redis");
                return (false, TimeSpan.Zero, $"Error: {ex.Message}");
            }
        }
         
    }
}
