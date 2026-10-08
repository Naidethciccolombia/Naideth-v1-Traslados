namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public sealed record PoliticaCancelacionDTO(string? Inicio, string? Fin, int? Porcentaje, int? Noches, decimal? Importe, string? Moneda, string? Descripcion, bool Reembolsable);
}
