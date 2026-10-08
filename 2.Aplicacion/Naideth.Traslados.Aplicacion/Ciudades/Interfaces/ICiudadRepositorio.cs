 
using Naideth.Central.Dominio.Ciudades; 

namespace Naideth.Traslados.Aplicacion.Ciudades.Interfaces
{
    public interface ICiudadRepositorio
    {    
        Task<string> GetCiudadesPorIatasAsync(string id, CancellationToken cancellationToken);
        //Task<((Ciudad ciudad, List<string> aeropuertos), (Ciudad ciudad, List<string> aeropuertos))> GetTrayectoPorIatasAsync(TrayectoDTO trayecto, CancellationToken cancellationToken);
    }
}
