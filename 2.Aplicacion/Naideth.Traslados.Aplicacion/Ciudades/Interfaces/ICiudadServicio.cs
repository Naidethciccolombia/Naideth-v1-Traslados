  
using Naideth.Central.Dominio.Ciudades; 

namespace Naideth.Traslados.Aplicacion.Ciudades.Interfaces
{
    public interface ICiudadServicio
    {
        Task<string> GetCiudadesPorIatasAsync(string id, CancellationToken cancellationToken);
       // Task<((Ciudad ciudad, List<string> aeropuertos) origen, (Ciudad ciudad, List<string> aeropuertos) destino)> GetTrayectoPorIatasAsync(TrayectoDTO trayecto, CancellationToken cancellationToken);

    }
}
