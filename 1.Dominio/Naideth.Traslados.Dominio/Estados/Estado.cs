using Naideth.Traslados.Dominio.Kernel.Exepciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Naideth.Traslados.Dominio.Estados
{ 
    public enum Estado
    {
        OK,
        ERROR,
        PENDIENTE
    }

}
