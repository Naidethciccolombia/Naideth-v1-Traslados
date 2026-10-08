using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.ConexionesExternas.DTO
{
    public class ResponseTravelKitStatusDTO: IDisposable
    {
        public bool Status { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Exception Ex { get; set; }
        public dynamic Data { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
         
        public void SetData(dynamic Document)
        {
            Data = Document;
        }

        public void Dispose()
        {
            Data?.Dispose(); // Libera memoria cuando ya no se use
        }
    }
}
