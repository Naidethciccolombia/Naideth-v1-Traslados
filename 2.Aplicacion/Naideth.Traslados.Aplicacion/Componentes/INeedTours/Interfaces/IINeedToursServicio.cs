using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Reservas.DTO.Rs;
using Naideth.Traslados.Dominio.Busquedas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Naideth.Traslados.Aplicacion.Componentes.INeedTours.Interfaces
{
    public interface IINeedToursServicio
    {
        Task<BusquedaResponseDTO> BusquedaAsync(Busqueda peticion, List<IntegracionDTO> listacredenciales, FiltroRequestDTO? Filtros, CancellationToken TokenCancelacion);
        Task<PreReservaResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, CacheDTO cachedResult, List<IntegracionDTO> listacredenciales, CancellationToken TokenCancelacion);
        Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, DisponibilidadDTO cachedResult, List<IntegracionDTO> listaCredenciales, CancellationToken TokenCancelacion);
        Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, List<IntegracionDTO> listaCredenciales, CancellationToken TokenCancelacion);
    }
}
