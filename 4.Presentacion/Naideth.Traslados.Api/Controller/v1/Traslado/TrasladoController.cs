using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rq;
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using Naideth.Traslados.Api.Contratos.V1.Cancelacion.Rs;
using Naideth.Traslados.Api.Contratos.V1.Cancelaciones.Rq;
using Naideth.Traslados.Api.Contratos.V1.Emisiones.Rq;
using Naideth.Traslados.Api.Contratos.V1.Emisiones.Rs;
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rq;
using Naideth.Traslados.Api.Contratos.V1.PreReservas.Rs;
using Naideth.Traslados.Api.Contratos.V1.Reservas;
using Naideth.Traslados.Api.Contratos.V1.Reservas.Rq;
using Naideth.Traslados.Api.Controllers.v1.Traslado.Extensiones.Busquedas; 
using Naideth.Traslados.Api.Controllers.v1.Traslado.Extensiones.PreReservas;
using Naideth.Traslados.Api.Controllers.v1.Traslado.Extensiones.Reservas;
using Naideth.Traslados.Api.Controllers.v1.Extensiones.Cancelaciones.Rq;
using Naideth.Traslados.Api.Controllers.v1.Extensiones.Cancelaciones.Rs;
using Naideth.Traslados.Api.Controllers.v1.Extensiones.Emisiones.Rq;
using Naideth.Traslados.Api.Controllers.v1.Extensiones.Emisiones.Rs;
using Naideth.Traslados.Api.Controllers.v1.Extensiones.PreReservas;
using Naideth.Traslados.Api.Controllers.v1.Extensiones.Reservas;
using Naideth.Traslados.Api.Kernel;
using Naideth.Traslados.Aplicacion.Busquedas.Interfaces;
using Naideth.Traslados.Aplicacion.CacheRedis.Interfaces;
using Naideth.Traslados.Aplicacion.Cancelaciones.Interfaces;
using Naideth.Traslados.Aplicacion.Emisiones.Interfaces;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.PreReservas.Interfaces;
using Naideth.Traslados.Aplicacion.Reservas.Interfaces;
namespace Naideth.Traslados.Api.Controllers.v1.Traslado
{
    [Authorize]
    [ApiController]
    public class TrasladoController : Controller
    {
        private readonly IBusquedaServicio _busquedaServicio; 
        private readonly IPreReservaServicio _preReservaServicio; 
        private readonly IReservaServicio _reservaServicio; 
        private readonly ICancelacionServicio _cancelacionServicio; 
        private readonly IEmisionServicio _emisionServicio; 
        private readonly ICacheRedisServicio _cacheRedisServicio;

        public TrasladoController(IBusquedaServicio busquedaServicio, IPreReservaServicio preReservaServicio, IReservaServicio reservaServicio, ICancelacionServicio cancelacionServicio, IEmisionServicio emisionServicio, ICacheRedisServicio cacheRedisServicio)
        {
            _busquedaServicio = busquedaServicio; 
            _preReservaServicio = preReservaServicio;   
            _reservaServicio = reservaServicio;
            _cancelacionServicio = cancelacionServicio;
            _emisionServicio = emisionServicio;
            _cacheRedisServicio = cacheRedisServicio;
        }

        [HttpPost(ApiEndpoints.V1.Traslados.BusquedaAsync, Name = "V1.Traslados.BusquedaAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BusquedaResponse))]
        public async Task<IActionResult> BusquedaAsync([FromBody] BusquedaRequest peticion, CancellationToken TokenCancelacion)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (peticion.Pagina <= 0) peticion.Pagina = 1;
            if (peticion.RegistrosPagina <= 0 || peticion.RegistrosPagina > 100) peticion.RegistrosPagina = 10;

            var busqueda = await _busquedaServicio.BusquedaAsync(peticion.ToCrearBusquedaDTO(), TokenCancelacion).ConfigureAwait(false);                  
            return Ok(busqueda.ToBusquedaResponseDTO());
        }

        [HttpPost(ApiEndpoints.V1.Traslados.TrasladoIncluidaAsync, Name = "V1.Traslados.TrasladoIncluidaAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TrasladoPreReservaRsDTO))]
        public async Task<IActionResult> TrasladoIncluidaAsync([FromBody] BusquedaRequest peticion, CancellationToken TokenCancelacion)
        {
            if (!ModelState.IsValid)
                return BadRequest();             

            var busqueda = await _busquedaServicio.TrasladoIncluidaAsync(peticion.ToCrearBusquedaDTO(), TokenCancelacion).ConfigureAwait(false);
            return Ok(busqueda.ToPreReservaResponseDTO());
        }


        [HttpPost(ApiEndpoints.V1.Traslados.PreReservaAsync, Name = "V1.Traslados.PreReservaAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PreReservaResponse))]
        public async Task<IActionResult> PreReservaAsync([FromBody] PreReservaRequest peticion, CancellationToken TokenCancelacion)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var preReserva = await _preReservaServicio.PreReservaAsync(peticion.ToCrearPreReservaDTO(), TokenCancelacion).ConfigureAwait(false);

            return Ok(preReserva.ToPreReservaResponseDTO());
        }

        [HttpPost(ApiEndpoints.V1.Traslados.PreReservaSeleccionadaAsync, Name = "V1.Traslados.PreReservaSeleccionadaAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PreReservaResponse))]
        public async Task<IActionResult> PreReservaSeleccionadaAsync([FromBody] PreReservaSeleccionadaRequest peticion, CancellationToken TokenCancelacion)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var Vuelos = await _preReservaServicio.PreReservaSeleccionadaAsync(peticion.ToCrearPreReservaSeleccionadaDTO(), TokenCancelacion).ConfigureAwait(false);

            return Ok(Vuelos.ToPreReservaResponseDTO());
        }


        [HttpPost(ApiEndpoints.V1.Traslados.ReservaAsync, Name = "V1.Traslados.ReservaAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TrasladoResponseReserva))]
        public async Task<IActionResult> ReservaAsync([FromBody] ReservaRequest peticion, CancellationToken TokenCancelacion)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = await _reservaServicio.ReservaAsync(peticion.ToCrearReservaDTO(), TokenCancelacion).ConfigureAwait(false);         
                return Ok(book.ToReservaDTO());
        }


        [HttpPost(ApiEndpoints.V1.Traslados.CancelacionAsync, Name = "V1.Traslados.CancelacionAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CancelacionResponse))]
        public async Task<IActionResult> CancelacionReservaAsync([FromBody] CancelacionRequest peticion, CancellationToken TokenCancelacion)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var cancelacion = await _cancelacionServicio.CancelacionAsync(peticion.ToCrearCancelacionDTO(), TokenCancelacion).ConfigureAwait(false);
            return Ok(cancelacion.ToCancelacionDTO());
        }



        /*[HttpPost(ApiEndpoints.V1.Traslados.EmisionAsync, Name = "V1.Traslados.EmisionAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmisionResponse))]
        public async Task<IActionResult> EmisionReservaAsync([FromBody] EmisionRequest peticion, CancellationToken TokenCancelacion)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var emision = await _emisionServicio.EmisionAsync(peticion.ToCrearEmisionDTO(), TokenCancelacion).ConfigureAwait(false);
            return Ok(emision.ToEmisionDTO());
        }*/

        [HttpPost(ApiEndpoints.V1.Traslados.BorrarCacheRedisAsync, Name = "V1.Traslados.BorrarCacheRedisAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> BorrarCacheRedisAsync(string key, CancellationToken TokenCancelacion)
        {
            await _cacheRedisServicio.RemoveAsync(key, TokenCancelacion).ConfigureAwait(false);

            return Ok();
        }
    }
}
