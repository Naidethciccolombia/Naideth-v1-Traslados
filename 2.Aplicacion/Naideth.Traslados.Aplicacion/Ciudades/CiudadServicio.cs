  
using Naideth.Central.Dominio.Ciudades;
using Naideth.Traslados.Aplicacion.Ciudades.Interfaces; 

namespace Naideth.Traslados.Aplicacion.Ciudades
{
    public sealed class CiudadServicio : ICiudadServicio
    {
        private readonly ICiudadRepositorio _CiudadRepositorio; 
        public CiudadServicio(ICiudadRepositorio CiudadRepositorio)
        {
            _CiudadRepositorio = CiudadRepositorio;
        }

        public async Task<string> GetCiudadesPorIatasAsync(string id, CancellationToken cancellationToken)
        {  
            return await _CiudadRepositorio.GetCiudadesPorIatasAsync(id, cancellationToken).ConfigureAwait(false);
        }

        //public async Task<((Ciudad ciudad, List<string> aeropuertos) origen, (Ciudad ciudad, List<string> aeropuertos) destino)> GetTrayectoPorIatasAsync(TrayectoDTO trayecto, CancellationToken cancellationToken)
        //{

        //    return await _CiudadRepositorio.GetTrayectoPorIatasAsync(trayecto, cancellationToken).ConfigureAwait(false);
        //}
    }
}
