using System;
using Naideth.Traslados.Dominio.Trayectos;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO
{
    public sealed record TrayectoDTO(int numero, DateTime fecha, string hora, UbicacionDTO iataOrigen, UbicacionDTO iataDestino)
    {
        public Trayecto ToDominio() => Trayecto.Crear(numero, fecha, hora, iataOrigen.ToDominio(), iataDestino.ToDominio());
    }
}
