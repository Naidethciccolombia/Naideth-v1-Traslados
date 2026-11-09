namespace Naideth.Traslados.Aplicacion.Busquedas.DTO.Rs
{
    public class DatosExtraDTO
    {
        public string Id { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal? Importe { get; set; }

        public string? Moneda { get; set; }

        public int? MaximoPermitido { get; set; }

        public bool Obligatorio { get; set; }

        public bool Garantizado { get; set; }

        public string? Especificaciones { get; set; }
    }
}
