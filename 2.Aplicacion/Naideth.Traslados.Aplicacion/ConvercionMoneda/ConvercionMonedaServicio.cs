
using Microsoft.Extensions.Logging; 
using Naideth.Traslados.Aplicacion.Busquedas; 
using Naideth.Traslados.Aplicacion.Busquedas.Interfaces; 
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Comunes.Interfaces;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.DTO;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.Interfaces;
using Naideth.Traslados.Dominio.Kernel.Exepciones; 

namespace Naideth.Traslados.Aplicacion.ConvercionMoneda
{
    public sealed class ConvercionMonedaServicio : IConvercionMonedaServicio
    {
        private readonly IConvercionMonedaRepositorio _convercionMonedaRepositorio;
        private readonly ICacheRedisServicio _cacheServicio; 
        private readonly IComunRepositorio _comunRepositorio;
        private Dictionary<string, decimal>? _tasasDict; 

        public ConvercionMonedaServicio(
            
            ICacheRedisServicio cacheServicio,
            IConvercionMonedaRepositorio convercionMonedaRepositorio
            , IComunRepositorio comunRepositorio)
        {
            _convercionMonedaRepositorio = convercionMonedaRepositorio;
            _cacheServicio = cacheServicio;
            _comunRepositorio = comunRepositorio;
             
             
        }


        
        public decimal ConvertirMoneda(
        decimal valor,
        string monedaOrigen,
        string monedaDestino)
        {
            monedaOrigen = monedaOrigen?.ToUpperInvariant();
            monedaDestino = monedaDestino?.ToUpperInvariant();

            // Misma moneda: no se requiere TRM. Evita depender de que la moneda esté en la
            // tabla de tasas y el redondeo de ida/vuelta por USD.
            if (string.Equals(monedaOrigen, monedaDestino, StringComparison.Ordinal))
                return valor;

            EnsureTasasLoadedAsync();

            if (_tasasDict == null || _tasasDict.Count == 0)
                throw new ExepcionConvitiendoTrm();

            decimal amountInUSD = monedaOrigen == "USD"
                ? valor
                : _tasasDict.TryGetValue(monedaOrigen, out var tasaOrigen) && tasaOrigen > 0
                    ? valor / tasaOrigen
                    : throw new ExepcionConvitiendoTrm();

            decimal resultado = monedaDestino == "USD"
                ? amountInUSD
                : _tasasDict.TryGetValue(monedaDestino, out var tasaDestino)
                    ? amountInUSD * tasaDestino
                    : throw new ExepcionConvitiendoTrm();

            return Math.Round(resultado, 2, MidpointRounding.AwayFromZero);
        }
      
        private void EnsureTasasLoadedAsync()
        {
            if (_tasasDict != null && _tasasDict.Count > 0)
                return; // Ya están en memoria, nada que hacer

            var tasas = _comunRepositorio
          .ListaTRMAsync(CancellationToken.None)
          .GetAwaiter()
          .GetResult();

            _tasasDict = tasas.ToDictionary(
                x => x.Moneda.ToUpperInvariant(),
                x => x.Valor);
        }

        private async Task<List<TasaCambioDTO>> ObtenerTasasCambio(CancellationToken cancellationToken)
        {
            var cacheKey = "TasasCambio";

            try
            {
                var cachedResult = await _cacheServicio.GetAsync<List<TasaCambioDTO>>(cacheKey, cancellationToken);
                if (cachedResult == null || !cachedResult.Any())
                {
                    cachedResult = await _convercionMonedaRepositorio.ListTasaCambioAsync(cancellationToken)
                                   ?? new List<TasaCambioDTO>();

                    if (cachedResult.Any())
                    {
                        await _cacheServicio.SetAsync(cacheKey, cachedResult, TimeSpan.FromDays(1), cancellationToken);
                    }
                }
                return cachedResult;
            }
            catch (Exception ex)
            { 
                return new List<TasaCambioDTO>();
            }
        }
    }

}
