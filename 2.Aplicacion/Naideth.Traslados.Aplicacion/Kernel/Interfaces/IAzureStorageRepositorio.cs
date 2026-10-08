using Microsoft.WindowsAzure.Storage.Table;

namespace Naideth.Vuelos.Aplicacion.Kernel.Interfaces
{
    public interface IAzureStorageRepositorio
    {
         Task<List<TableEntity>> ObtenerTablaAsync(string nombre, CancellationToken TokenCancelacion);
        Task<List<TableEntity>> ObtenerTablaColumnasAsync(string tabla, string particion, List<string> listaDestinos,  List<string> columnas, CancellationToken TokenCancelacion);
         
    }
}
