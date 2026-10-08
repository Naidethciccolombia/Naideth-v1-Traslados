using Microsoft.OpenApi; 
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.Kernel
{ 
   public class SwaggerFileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody != null)
            {
                foreach (var contentType in operation.RequestBody.Content.Keys.ToList())
                {
                    if (contentType == "multipart/form-data")
                    {
                        var formData = operation.RequestBody.Content[contentType];
                        formData.Schema.Properties["file"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        };
                    }
                }
            }
        }
    }
}
