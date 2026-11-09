using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO;
using Naideth.Traslados.Aplicacion.Cancelaciones.DTO.Rs;
using Naideth.Traslados.Aplicacion.Comunes.DTO.Rq;
using Naideth.Traslados.Aplicacion.PreReservas.DTO.Rq;
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
    public interface ICivitatisRepositorio
    {
        Task<ResponseDTO> BusquedaAsync(Busqueda peticion, IntegracionDTO credencial, FiltroRequestDTO? filtros, CancellationToken TokenCancelacion);

        Task<ResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, string quoteId, CancellationToken TokenCancelacion);

        Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, DisponibilidadDTO disponibilidadSeleccionada, CancellationToken TokenCancelacion);

        Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, IntegracionDTO credencial, CancellationToken TokenCancelacion);
    }
}
