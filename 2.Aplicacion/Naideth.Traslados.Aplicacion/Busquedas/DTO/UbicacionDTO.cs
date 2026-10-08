using Naideth.Traslados.Dominio.Trayectos;

namespace Naideth.Traslados.Aplicacion.Busquedas.DTO
{
    public sealed record UbicacionDTO(string origen, string tipo)
    {
        public Ubicacion ToDominio() => Ubicacion.Crear(origen, tipo);
    }
}
