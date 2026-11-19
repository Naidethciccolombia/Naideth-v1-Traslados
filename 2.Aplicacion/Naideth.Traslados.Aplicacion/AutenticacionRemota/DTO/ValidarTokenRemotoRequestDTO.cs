namespace Naideth.Traslados.Aplicacion.AutenticacionRemota.DTO;

public sealed record ValidarTokenRemotoRequestDTO
{
    public required string Token { get; init; }
}
