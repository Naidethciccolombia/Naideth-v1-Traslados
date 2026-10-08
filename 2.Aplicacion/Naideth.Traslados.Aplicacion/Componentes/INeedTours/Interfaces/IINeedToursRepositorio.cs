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
    public interface IINeedToursRepositorio
    {
        Task<ResponseDTO> BusquedaAsync(Busqueda peticion, IntegracionDTO credenciales, FiltroRequestDTO? filtros, CancellationToken TokenCancelacion);
        Task<PreReservaResponseDTO> PreReservaAsync(PreReservaDTO peticion, Busqueda busqueda, IntegracionDTO credenciales, DisponibilidadDTO disponibilidadSeleccionada, CancellationToken TokenCancelacion);
        Task<ReservaResponseDTO> ReservaAsync(ReservaDTO peticion, Busqueda busqueda, IntegracionDTO credencial, DisponibilidadDTO disponibilidadSeleccionada, CancellationToken TokenCancelacion);
        Task<CancelacionResponseDTO> PoliciesAsync(CancelacionDTO peticion, IntegracionDTO Credenciales, CancellationToken TokenCancelacion);
        Task<CancelacionResponseDTO> CancelacionAsync(CancelacionDTO peticion, IntegracionDTO Credenciales, CancellationToken TokenCancelacion);
    }
}
