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
using System.Threading;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.Componentes.Civitatis.Interfaces
{
    public interface ICivitatisServicio
    {
        Task<BusquedaResponseDTO> BusquedaAsync(Busqueda peticion, List<IntegracionDTO> listaCredenciales, FiltroRequestDTO? filtros, CancellationToken TokenCancelacion);

        Task<PreReservaResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, CacheDTO cachedResult, bool noFormateo, List<IntegracionDTO> listaCredenciales, CancellationToken TokenCancelacion);

        Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, DisponibilidadDTO cachedResult, bool noFormateo, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion);

        Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, List<IntegracionDTO> listaCredenciales, CancellationToken tokenCancelacion);
    }
}
