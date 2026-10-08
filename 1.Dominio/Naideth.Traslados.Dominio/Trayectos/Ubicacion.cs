using Naideth.Traslados.Dominio.Kernel.Exepciones;

namespace Naideth.Traslados.Dominio.Trayectos
{
    public sealed class Ubicacion
    {
        public const string TipoIATA = "IATA";
        public const string TipoHotel = "HOTEL";

        public string Codigo { get; private set; }
        public string Tipo { get; private set; }

        private Ubicacion(string codigo, string tipo)
        {
            Codigo = codigo;
            Tipo = tipo;
        }

        public static Ubicacion Crear(string codigo, string tipo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new CampoInvalidoExcepcion("ubicacion.codigo");

            if (tipo != TipoIATA && tipo != TipoHotel)
                throw new CampoInvalidoExcepcion("ubicacion.tipo");

            return new Ubicacion(codigo.Trim(), tipo);
        }

        public bool EsHotel => Tipo == TipoHotel;

        public bool EsIATA => Tipo == TipoIATA;
    }
}
