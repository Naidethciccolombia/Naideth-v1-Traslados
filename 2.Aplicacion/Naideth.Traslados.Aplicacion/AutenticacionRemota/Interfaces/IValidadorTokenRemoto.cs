using Naideth.Traslados.Aplicacion.AutenticacionRemota.DTO;

namespace Naideth.Traslados.Aplicacion.AutenticacionRemota.Interfaces;

public interface IValidadorTokenRemoto
{
    Task<ValidarTokenRemotoResponseDTO> ValidarTokenAsync(string tokenUsuario, CancellationToken cancellationToken);
}
