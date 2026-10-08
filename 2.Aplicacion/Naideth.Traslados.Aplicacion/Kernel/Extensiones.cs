 
using NPOI.SS.Formula.Functions;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Naideth.Traslados.Aplicacion.Kernel
{
    public static class Extensiones
    {
        // Cache thread-safe de XmlSerializer
        private static readonly ConcurrentDictionary<Type, XmlSerializer> _xmlSerializerCache =
            new ConcurrentDictionary<Type, XmlSerializer>();



        // XmlWriterSettings reutilizable (sin declaración XML, sin indent para menor tamaño; cambia Indent=true si lo necesitas)
        private static readonly XmlWriterSettings _xmlWriterSettingsCompact = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = false,
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.None
        };

        // Conversores seguros para decimales
        private sealed class SafeDecimalConverter : JsonConverter<decimal>
        {
            public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.Number:
                            // Intentar leer como decimal; si falla por tamaño, leer como double y convertir con control
                            if (reader.TryGetDecimal(out var d))
                                return d;
                            if (reader.TryGetDouble(out var dbl))
                            {
                                try { return Convert.ToDecimal(dbl); }
                                catch { return 0m; }
                            }
                            return 0m;
                        case JsonTokenType.String:
                            var s = reader.GetString();
                            if (string.IsNullOrWhiteSpace(s)) return 0m;
                            // Intentar con Invariant y estilos amplios
                            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultInv))
                                return resultInv;
                            // Intentar con cultura actual por si viene con comas
                            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out var resultCurr))
                                return resultCurr;
                            // Intentar parsear como double y convertir
                            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dd))
                            {
                                try { return Convert.ToDecimal(dd); } catch { return 0m; }
                            }
                            return 0m;
                        default:
                            return 0m;
                    }
                }
                catch
                {
                    return 0m;
                }
            }

            public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private sealed class SafeNullableDecimalConverter : JsonConverter<decimal?>
        {
            public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return null;
                var dec = new SafeDecimalConverter().Read(ref reader, typeof(decimal), options);
                return dec;
            }

            public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
            {
                if (value.HasValue) writer.WriteNumberValue(value.Value);
                else writer.WriteNullValue();
            }
        }

        public static string FormatearHoras(this string input)
        {
            return input.Replace("p. m.", "PM").Replace("a. m.", "AM")
                        .Replace(" p.�m.", " PM").Replace(" a.�m.", " AM")
                        .Replace(" p. m.", " PM").Replace(" a. m.", " AM");
        }

        public static string ToXml(this string json, string rootName = "Root")
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON vacío o nulo.");

            var xmlDoc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(json, rootName);
            return xmlDoc?.OuterXml
                ?? throw new Exception("No se pudo convertir JSON a XML.");
        }

        public static string ToXml(this object o, XmlSerializerNamespaces ns = null)
        {

            if (o == null) return string.Empty;
            try
            {
                var serializer = GetCachedSerializer(o.GetType());
                var sb = new StringBuilder(4096);
                using (var sw = new StringWriter(sb))
                using (var xw = XmlWriter.Create(sw, _xmlWriterSettingsCompact))
                {
                    if (ns != null)
                        serializer.Serialize(xw, o, ns);
                    else
                        serializer.Serialize(xw, o);
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e);
                return string.Empty;
            }

        }

        private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializerCache = new();
  
      
        private static XmlSerializer GetCachedSerializer(Type t)
        {
            return _xmlSerializerCache.GetOrAdd(t, key => new XmlSerializer(key));
        }





        public static string SerializeObjectTextCamel(this object? value, JsonSerializerOptions? options = null)
        {
            options ??= new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Permite caracteres especiales sin codificación Unicode
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new SafeDecimalConverter(), new SafeNullableDecimalConverter() }
            };

            return System.Text.Json.JsonSerializer.Serialize(value, options);
        }

      
        public static T? ConvertirObjeto<T>(this object? valor)
        {
            if (valor == null)
                return default;

            // 1️⃣ Ya es el tipo correcto
            if (valor is T t)
                return t;

            // 2️⃣ Viene como JsonElement
            if (valor is JsonElement je)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new SafeDecimalConverter(), new SafeNullableDecimalConverter() },
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };
                    return je.Deserialize<T>(options);
                }
                catch
                {
                    return default;
                }
            }

            // 3️⃣ Viene como string JSON
            if (valor is string json)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new SafeDecimalConverter(), new SafeNullableDecimalConverter() },
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };
                    return JsonSerializer.Deserialize<T>(json, options);
                }
                catch
                {
                    return default;
                }
            }

            // 4️⃣ Cualquier otro objeto → JSON → T
            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new SafeDecimalConverter(), new SafeNullableDecimalConverter() },
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
                var jsons = JsonSerializer.Serialize(valor, options);
                return JsonSerializer.Deserialize<T>(jsons, options);
            }
            catch
            {
                return default;
            }
        }
        public static T? DeSerializarObjeto<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new SafeDecimalConverter(), new SafeNullableDecimalConverter() },
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (JsonException)
            {
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }


        public static T? DeSerializarObjetoTest<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new SafeDecimalConverter(),
                new SafeNullableDecimalConverter()
            },
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error JSON: {e.Message}");
                return default;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error general: {e.Message}");
                return default;
            }
        }

        public static async Task<T?> DeSerializarObjetoAsync<T>(
       this string json,
       CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            //try
            //{
            //    var options = new JsonSerializerOptions
            //    {
            //        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new SafeDecimalConverter(), new SafeNullableDecimalConverter() },
            //        PropertyNameCaseInsensitive = true,
            //        NumberHandling = JsonNumberHandling.AllowReadingFromString
            //    };

            //    return await JsonSerializer.DeserializeAsync<T>(
            //        new MemoryStream(Encoding.UTF8.GetBytes(json)),
            //        options,
            //        cancellationToken
            //    );
            //}
            //catch(Exception e) 
            //{ 
            //    return default;
            //}

            try
            {
                // Prueba 1: Deserialización básica
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

              //  var result = JsonSerializer.Deserialize<T>(json, options); 
                 
               // var serialized = JsonSerializer.Serialize(result, options);
                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FALLÓ: {ex.Message}");
                if (ex is JsonException jex)
                {
                    Console.WriteLine($"Path: {jex.Path}");
                    Console.WriteLine($"LineNumber: {jex.LineNumber}");
                }
            }
            return default;
        }

        public static string SerializeObjectText(this object? value, JsonSerializerOptions? options = null)
        {
            options ??= new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Permite caracteres especiales sin codificación Unicode
                Converters = { new SafeDecimalConverter(), new SafeNullableDecimalConverter() }
            };

            return System.Text.Json.JsonSerializer.Serialize(value, options);
        }

        public static DateTime ParseAndCorrectFecha(string inputDate)
        {
            if (string.IsNullOrWhiteSpace(inputDate))
            { 
                return DateTime.Now;
            }

            DateTime validDate;

            try
            {
                // Normalizar AM/PM en español a formato inglés
                inputDate = inputDate.Trim().FormatearHoras();

                // Intentar conversión directa
                if (DateTime.TryParse(inputDate, new CultureInfo("es-ES"), DateTimeStyles.None, out validDate))
                {
                    return validDate;
                }

                // Formatos permitidos
                string[] formatos = {
                "dd/MM/yyyy hh:mm:ss tt",  // Español (día/mes/año)
                "MM/dd/yyyy hh:mm:ss tt",  // Inglés (mes/día/año)
                "yyyy-MM-dd HH:mm:ss",     // Formato ISO
                "dd/MM/yyyy", "MM/dd/yyyy"
            };

                if (DateTime.TryParseExact(inputDate, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out validDate))
                {
                    return validDate;
                }
                 
                return DateTime.Now;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al corregir la fecha: {ex.Message}");
                return DateTime.Now;
            }
        }

        public static byte[] Compress<T>(T data)
        {
            using var memoryStream = new MemoryStream();
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
            using var writer = new StreamWriter(gzipStream, Encoding.UTF8);

            var json = JsonSerializer.Serialize(data);
            writer.Write(json);
            writer.Flush();
            gzipStream.Flush();

            return memoryStream.ToArray();
        }

        public static T Decompress<T>(byte[] compressedData)
        {
            using var memoryStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream, Encoding.UTF8);

            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(json)!;
        }


        public static string GetSafeString(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
                ? property.GetString() ?? string.Empty
                : string.Empty;
        }
        public static int GetSafeInt(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
                ? property.GetInt32()
                : 0;
        }
        public static decimal GetSafeDecimal(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                try
                {
                    switch (property.ValueKind)
                    {
                        case JsonValueKind.Number:
                            if (property.TryGetDecimal(out var d)) return d;
                            if (property.TryGetDouble(out var dbl)) { try { return Convert.ToDecimal(dbl); } catch { return 0m; } }
                            break;
                        case JsonValueKind.String:
                            var s = property.GetString();
                            if (string.IsNullOrWhiteSpace(s)) return 0m;
                            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultInv)) return resultInv;
                            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out var resultCurr)) return resultCurr;
                            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dd)) { try { return Convert.ToDecimal(dd); } catch { return 0m; } }
                            break;
                    }
                }
                catch { }
            }
            return 0m;
        }   
        public static JsonElement GetJsonPropertyOrEmpty(this JsonElement root, string key)
        {
            return root.TryGetProperty(key, out var property)
                ? property
                : new JsonElement(); // Retorna un JsonElement vacío si no es un array
        }

        public static bool GetSafeBool(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.True
                ? true
                : property.ValueKind == JsonValueKind.False ? false : false;
        }


    }

}
