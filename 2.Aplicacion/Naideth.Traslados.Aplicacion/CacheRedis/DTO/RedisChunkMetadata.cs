using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.CacheRedis.DTO
{ 
    public class RedisChunkMetadata
    {
        public int TotalChunks { get; set; }

        public string TypeName { get; set; }

        public long TotalSize { get; set; }

        public bool IsChunked { get; set; }
    }
}
