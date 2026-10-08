using Newtonsoft.Json; 

namespace Naideth.Traslados.Dominio.Kernel.Extensiones
{
    public static class ExtensionesGlobales
    {
        public static string ToConvertObjectJson(this object objeto)
        {
            return JsonConvert.SerializeObject(objeto);
        }

        public static T  ToConvertJsonObject<T>(this string objeto)
        {
            return JsonConvert.DeserializeObject<T>(objeto);
        }
        public static bool ToBool(this bool? value, bool defaultValue = false)
        {
            return value ?? defaultValue;
        }
        public static string ToNullString(this string value, string defaultValue = "")
        {
            return value ?? defaultValue;
        }
        public static Guid ToGuidOrDefault(this string value, Guid defaultValue = default(Guid))
        {
            if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var guid))
            {
                return defaultValue;
            }

            return guid;
        }
        public static Guid ToGuid(this Guid? guid)
        {  
            // Verifica si el valor nullable Guid tiene un valor
            if (guid.HasValue)
            {
                return guid.Value; // Devuelve el valor Guid
            }
            else
            {
                // En este caso, puedes elegir devolver Guid.Empty o lanzar una excepción,
                // dependiendo de cómo quieras manejar la ausencia de valor.
                // Aquí devuelvo Guid.Empty como valor predeterminado.
                return Guid.Empty;
            }

        }
   
    
    }
}
