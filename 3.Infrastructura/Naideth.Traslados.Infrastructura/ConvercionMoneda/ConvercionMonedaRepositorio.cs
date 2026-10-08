using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.DTO;
using Naideth.Traslados.Aplicacion.ConvercionMoneda.Interfaces;
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces; 

namespace Naideth.Traslados.Infrastructura.ConvercionMoneda
{
    internal class ConvercionMonedaRepositorio : IConvercionMonedaRepositorio
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ConvercionMonedaRepositorio(IHttpContextAccessor httpContextAccessor,IApplicationDbContext context)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor; 
        }

        public async Task<List<TasaCambioDTO>> ListTasaCambioAsync(CancellationToken TokenCancelacion)
        {

            var query = _context.TasaCambio
         .Join(_context.Moneda,
             tasa => tasa.IdMoneda,
             moneda => moneda.IdMoneda,
             (tasa, moneda) => new { TasaCambio = tasa, Moneda = moneda })
         .Where(item => item.TasaCambio.Estado && item.Moneda.Estado && item.TasaCambio.Fecha.Date == DateTime.Now.Date);

 
            var ListaItemDtos = await query
             .ToListAsync(TokenCancelacion).ConfigureAwait(false);

            return ListaItemDtos.Select(tc=> new TasaCambioDTO(
                tc.TasaCambio.IdTasaCambio,
                tc.TasaCambio.IdMoneda,
                tc.Moneda.Equivalente, 
                tc.TasaCambio.Valor,
                tc.TasaCambio.Fecha))
                .ToList();   
                   
        }


    }

}
