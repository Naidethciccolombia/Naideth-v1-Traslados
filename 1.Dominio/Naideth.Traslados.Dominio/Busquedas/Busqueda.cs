using Naideth.Traslados.Dominio.Estados;
using Naideth.Traslados.Dominio.Kernel.Exepciones;
using Naideth.Traslados.Dominio.Trayectos;
using System.Collections.Generic;

namespace Naideth.Traslados.Dominio.Busquedas
{ 
    public sealed class Busqueda
    {
        public Guid IdBusqueda { get; set; } = Guid.NewGuid();
        public List<Trayecto> Trayectos { get; set; } = new List<Trayecto>();
        public string Referencia { get; set; }
        public string PaisDestino { get; set; }
        public bool Incluida { get; set; }
        public List<Pasajero> Pasajeros { get; set; } = new List<Pasajero>();
        public string Moneda { get; set; }
        public decimal MarkUps { get; set; }

        public DateTime Inicio => Trayectos.OrderBy(t => t.Numero).FirstOrDefault()?.FechaHora ?? default;
        public DateTime Fin => Trayectos.OrderBy(t => t.Numero).LastOrDefault()?.FechaHora ?? default;
        public bool AgregarRegreso => Trayectos.Count > 1;

        private Busqueda(Guid idBusqueda, List<Trayecto> trayectos, string paisDestino, string moneda, string referencia, bool incluida, decimal markUps)
        {
            IdBusqueda = idBusqueda;
            Trayectos = trayectos;
            Moneda = moneda;
            MarkUps = markUps;
            Referencia = referencia;
            PaisDestino = paisDestino;
            Incluida = incluida;
        }
        public static Busqueda Crear(Guid idBusqueda, List<Trayecto> trayectos, string paisDestino, string moneda, string referencia, bool incluida, decimal markUps)
        {
            if (moneda == string.Empty)
                throw new CampoInvalidoExcepcion("moneda");

            if (trayectos is null || trayectos.Count == 0)
                throw new CampoInvalidoExcepcion("trayectos");

            return new Busqueda(idBusqueda, trayectos, paisDestino, moneda, referencia, incluida, markUps);

        }
         
        public void ListPasajeros(List<Pasajero> pasajeros)
        {

            if (pasajeros.Count() == 0)
                throw new CampoInvalidoExcepcion("pasajeros");

            if (pasajeros.Count(x => x.Tipo == "ADT") < 1)
                throw new CampoInvalidoExcepcion("Adultos");


            Pasajeros = pasajeros ?? throw new ArgumentNullException(nameof(Pasajero)); 
        }


    }

}
