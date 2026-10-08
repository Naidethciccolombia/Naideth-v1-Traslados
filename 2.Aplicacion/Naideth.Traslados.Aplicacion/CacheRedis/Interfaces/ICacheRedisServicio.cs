
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs;
using NPOI.SS.Formula.Functions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.CacheRedis.Interfaces
{
    public interface ICacheRedisServicio
    { 
        ValueTask<T?> GetAsync<T>(string key, CancellationToken TokenCancelacion); 
        ValueTask<T?> GetReservaAsync<T>(string key, CancellationToken TokenCancelacion); 
        Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken TokenCancelacion); 
        Task UpsertAsync<T>(string key, T value, TimeSpan expiration, CancellationToken TokenCancelacion); 
        Task SetReservaAsync<T>(string key, T value, TimeSpan expiration, CancellationToken TokenCancelacion); 
        Task RemoveAsync(string key, CancellationToken TokenCancelacion);
        Task AppendAsync(string key, List<DisponibilidadDTO> nuevas, TimeSpan expiration);
        Task UpdateEstadoAsync(string key, bool esParcial, TimeSpan expiration);
        Task UpdateIntegradoresStatusAsync(string key, List<IntegradorStatusDTO> integradoresStatus, TimeSpan expiration);
        Task<bool> TryAcquireLockReservaAsync(string key, TimeSpan expiration);
        Task ReleaseLockReservaAsync(string key);
        Task<int> IncrementReservaAsync(string key, TimeSpan expiration);
    }
}
