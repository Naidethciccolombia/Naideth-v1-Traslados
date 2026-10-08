
using Naideth.Traslados.Dominio.Kernel.Exepciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Naideth.Traslados.Dominio.Trayectos
{ 
    public sealed class Pasajero
    {   
        public int Cantidad { get; set; }  
        public string Tipo { get; set; }
        public List<int> Edades { get; set; } 
        public List<int>? Upgrade { get; set; } 

        private Pasajero(int cantidad, string tipo, List<int> edades, List<int>? upgrade)
        { 
             Cantidad = cantidad;
             Tipo = tipo;
             Edades = edades;
            Upgrade = upgrade;

        } 
        public static Pasajero Crear(int cantidad, string tipo, List<int> edades, List<int>? upgrade)
        {   
            if(edades.Count < 0 )
                throw new CampoInvalidoExcepcion("Edades No disponibles");

            if (edades.Count <= 0 && tipo == "CHD")
                throw new CampoInvalidoExcepcion("Edades No disponibles");

            return new Pasajero(cantidad, tipo, edades, upgrade);

        }
          
    }

}
