using Naideth.Traslados.Dominio.Kernel.Exepciones;
using System;

namespace Naideth.Traslados.Dominio.Trayectos
{
    public sealed class Trayecto
    {
        public int Numero { get; private set; }
        public DateTime Fecha { get; private set; }
        public string Hora { get; private set; }
        public Ubicacion Origen { get; private set; }
        public Ubicacion Destino { get; private set; }

        private Trayecto(int numero, DateTime fecha, string hora, Ubicacion origen, Ubicacion destino)
        {
            Numero = numero;
            Fecha = fecha.Date;
            Hora = hora;
            Origen = origen;
            Destino = destino;
        }

        public static Trayecto Crear(int numero, DateTime fecha, string hora, Ubicacion origen, Ubicacion destino)
        {
            if (numero <= 0)
                throw new CampoInvalidoExcepcion("trayecto.numero");

            if (string.IsNullOrWhiteSpace(hora))
                throw new CampoInvalidoExcepcion("trayecto.hora");

            if (origen is null)
                throw new CampoInvalidoExcepcion("trayecto.origen");

            if (destino is null)
                throw new CampoInvalidoExcepcion("trayecto.destino");

            return new Trayecto(numero, fecha, hora, origen, destino);
        }

        public DateTime FechaHora => Fecha.Date.Add(ObtenerHora());

        public TimeSpan ObtenerHora()
        {
            if (TimeSpan.TryParse(Hora, out var hora))
                return hora;

            throw new CampoInvalidoExcepcion("trayecto.hora");
        }
    }
}
