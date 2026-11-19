namespace Naideth.Traslados.Aplicacion.AutenticacionRemota.DTO;

public sealed record ValidarTokenRemotoResponseDTO
{
    public required bool EsValido { get; init; }
    public Guid IdUsuario { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Tenants { get; init; } = Array.Empty<string>();
    public string Mensaje { get; init; } = string.Empty;
}
