using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Dominio.Kernel.Encriptado.DTO
{
    public sealed class VariableEncriptadaDTO
    {
        public byte[] EncryptedValue { get; set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }
}
