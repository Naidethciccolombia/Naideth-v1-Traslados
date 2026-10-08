using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rq;
using Naideth.Traslados.Api.Contratos.V1.Busquedas.Rs;
using Naideth.Traslados.Aplicacion.Busquedas.DTO;
using Naideth.Traslados.Aplicacion.Busquedas.DTO.Rq;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Naideth.Traslados.Api.Controllers.v1.Traslado.Extensiones.Busquedas
{
    internal static class TrasladoRequestExtensiones
    {
        internal static BusquedaDTO ToCrearBusquedaDTO(this BusquedaRequest peticion)
        {
            if (peticion.Trayectos is null || peticion.Trayectos.Count == 0)
                throw new Exception("Debe indicar al menos un trayecto.");

            foreach (var trayecto in peticion.Trayectos)
            {
                if (trayecto.Fecha < DateTime.Today)
                    throw new Exception(
                        $"La fecha del trayecto {trayecto.Numero} ({trayecto.Fecha:yyyy-MM-dd}) no puede ser menor a la fecha actual.");

                if (!TimeSpan.TryParse(trayecto.Hora, out _))
                    throw new Exception(
                        $"La hora del trayecto {trayecto.Numero} ({trayecto.Hora}) no es válida.");
            }

            var trayectos = peticion.Trayectos
                .OrderBy(t => t.Numero)
                .Select(t => new TrayectoDTO(
                    t.Numero,
                    t.Fecha,
                    t.Hora,
                    new UbicacionDTO(t.IataOrigen.Origen, t.IataOrigen.Tipo),
                    new UbicacionDTO(t.IataDestino.Origen, t.IataDestino.Tipo)))
                .ToList();

            return new BusquedaDTO(
                peticion.Moneda,
                peticion.Referencia,
                peticion.Pasajeros.ToPasajerosPeticionDTO(),
                trayectos,
                peticion.Pagina ?? 1,
                peticion.RegistrosPagina ?? 5,
                peticion.Filtros?.ToFiltroResponseDTO());
                  
        }
         
           
        internal static List<PasajerosPeticionDTO> ToPasajerosPeticionDTO(this List<PasajerosPeticionRequest> peticion)
        {
            return peticion.Where(val=> val.Cantidad>0).Select(item => new PasajerosPeticionDTO(
                item.Tipo,
                item.Cantidad,
                item.Edades?? new List<int>(),
                item.Upgrade
                )).ToList();
        }
         
   


        public static string NormalizarNombreAereo(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            // Quitar acentos
            var normalized = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = Char.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            // Reglas típicas PNR
            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .ToUpperInvariant()
                     .Replace("Ñ", "N")
                     .Replace("'", "")
                     .Replace("-", " ")
                     .Trim();
        }
          
   
        internal static FiltroOrdenamientoRequestDTO ToOrdenamientoDTO(this FiltroOrdenamientoRequest peticion)
        {  
            return new FiltroOrdenamientoRequestDTO(peticion.Columna, peticion.Ascendente);
        }

        internal static FiltroRequestDTO ToFiltroResponseDTO(this FiltroRequest? fil)
        {
            if (fil == null)
            {
                return new FiltroRequestDTO(
                    null, null, 0, null, null);
            }

            var culture = CultureInfo.InvariantCulture;// new CultureInfo("es-CO");

            // 🔸 Procesar rangos con validación
            var precio = ParseFiltroRango(
                fil.Precio,
                culture,
                "Precio",
                minPermitido: 0m
            );
              
            // 🔸 Limpiar listas (remover valores vacíos/null)
            var fuentes = fil.Fuentes?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            var nombres = fil.Nombres?
             .Where(s => !string.IsNullOrWhiteSpace(s))
             .ToList();


            return new FiltroRequestDTO(
                precio,
                fuentes,
                fil.MarkUps,
                fil.Ordenamiento,
                nombres
            );

        }
        private static FiltroPrecioRequestDTO? ParseFiltroRango(
      FiltroPrecioRequest? filtro,
      CultureInfo culture,
      string nombreCampo,
      decimal? minPermitido = null,
      decimal? maxPermitido = null)
        {
            if (filtro == null)
                return null;

            decimal? minimo = null;
            decimal? maximo = null;

            // 🔸 Parsear valor mínimo con el nuevo método flexible
            if (!string.IsNullOrWhiteSpace(filtro.minimo))
            {
                var parsedMin = ParseToDecimal(filtro.minimo);

                if (!parsedMin.HasValue)
                {
                    throw new ArgumentException(
                        $"{nombreCampo}: El valor mínimo '{filtro.minimo}' no es un número válido. " +
                        $"Formatos aceptados: colombiano (12.769.987) o internacional (12,769.987)"
                    );
                }

                // Validar contra límites permitidos
                if (minPermitido.HasValue && parsedMin.Value < minPermitido.Value)
                {
                    throw new ArgumentException(
                        $"{nombreCampo}: El valor mínimo {parsedMin.Value:N2} no puede ser menor que {minPermitido.Value:N2}"
                    );
                }

                if (maxPermitido.HasValue && parsedMin.Value > maxPermitido.Value)
                {
                    throw new ArgumentException(
                        $"{nombreCampo}: El valor mínimo {parsedMin.Value:N2} no puede ser mayor que {maxPermitido.Value:N2}"
                    );
                }

                minimo = parsedMin.Value;
            }

            // 🔸 Parsear valor máximo con el nuevo método flexible
            if (!string.IsNullOrWhiteSpace(filtro.maximo))
            {
                var parsedMax = ParseToDecimal(filtro.maximo);

                if (!parsedMax.HasValue)
                {
                    throw new ArgumentException(
                        $"{nombreCampo}: El valor máximo '{filtro.maximo}' no es un número válido. " +
                        $"Formatos aceptados: colombiano (12.769.987) o internacional (12,769.987)"
                    );
                }

                // Validar contra límites permitidos
                if (minPermitido.HasValue && parsedMax.Value < minPermitido.Value)
                {
                    throw new ArgumentException(
                        $"{nombreCampo}: El valor máximo {parsedMax.Value:N2} no puede ser menor que {minPermitido.Value:N2}"
                    );
                }

                if (maxPermitido.HasValue && parsedMax.Value > maxPermitido.Value)
                {
                    throw new ArgumentException(
                        $"{nombreCampo}: El valor máximo {parsedMax.Value:N2} no puede ser mayor que {maxPermitido.Value:N2}"
                    );
                }

                maximo = parsedMax.Value;
            }

            // 🔸 Validar coherencia del rango
            if (minimo.HasValue && maximo.HasValue && minimo.Value > maximo.Value)
            {
                throw new ArgumentException(
                    $"{nombreCampo}: El valor mínimo ({minimo.Value:N2}) no puede ser mayor " +
                    $"que el máximo ({maximo.Value:N2})"
                );
            }

            if (!minimo.HasValue && !maximo.HasValue)
                return null;

            return new FiltroPrecioRequestDTO(minimo, maximo);
        }


        public static decimal? ParseToDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            valor = valor.Trim();


            // 🔸 PASO 2: Parsear número limpio (sin sufijos)
            return ParseNumeroLimpio(valor);
        }

        private static decimal? ParseNumeroLimpio(string valor)
        {
            // Eliminar espacios
            valor = valor.Trim();

            // 🔸 Intentar parsear directamente con cultura invariante
            if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
                return result;

            // 🔸 Intentar con cultura colombiana
            if (decimal.TryParse(valor, NumberStyles.Number, new CultureInfo("es-CO"), out result))
                return result;

            // 🔸 Normalizar manualmente (quitar separadores de miles, estandarizar decimal)
            var normalizado = NormalizarNumero(valor);
            if (!string.IsNullOrEmpty(normalizado))
            {
                if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                    return result;
            }

            return null;
        }

        private static string NormalizarNumero(string valor)
        {
            // Eliminar caracteres no numéricos (excepto punto y coma)
            var limpio = new string(valor.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            if (string.IsNullOrEmpty(limpio))
                return null;

            // Contar separadores
            var puntos = limpio.Count(c => c == '.');
            var comas = limpio.Count(c => c == ',');

            // 🔸 Caso 1: "12.769.987" o "12.769.987,50" (formato colombiano)
            if (puntos >= 1 && comas <= 1)
            {
                // Si tiene coma decimal
                if (comas == 1)
                {
                    var partes = limpio.Split(',');
                    var parteEntera = partes[0].Replace(".", ""); // Quitar puntos de miles
                    var parteDecimal = partes[1];
                    return $"{parteEntera}.{parteDecimal}"; // Formato invariante
                }
                else // Sin decimales
                {
                    return limpio.Replace(".", ""); // Quitar puntos de miles
                }
            }

            // 🔸 Caso 2: "12,769,987" o "12,769,987.50" (formato internacional)
            if (comas >= 1 && puntos <= 1)
            {
                // Si tiene punto decimal
                if (puntos == 1)
                {
                    var partes = limpio.Split('.');
                    var parteEntera = partes[0].Replace(",", ""); // Quitar comas de miles
                    var parteDecimal = partes[1];
                    return $"{parteEntera}.{parteDecimal}"; // Formato invariante
                }
                else // Sin decimales
                {
                    return limpio.Replace(",", ""); // Quitar comas de miles
                }
            }

            // 🔸 Caso 3: Sin separadores "12769987" o "12769987.50"
            if (puntos == 0 && comas == 0)
            {
                return limpio; // Ya está en formato invariante
            }

            // 🔸 Caso 4: "12.769,987" (mezcla: punto miles, coma decimal)
            if (puntos >= 1 && comas == 1 && limpio.IndexOf('.') < limpio.IndexOf(','))
            {
                var partes = limpio.Split(',');
                var parteEntera = partes[0].Replace(".", "");
                var parteDecimal = partes[1];
                return $"{parteEntera}.{parteDecimal}";
            }

            return limpio;
        }


        internal static FiltroVariableRequestDTO ToVariableDTO(this FiltroVariableRequest? peticion)
        {
            return new FiltroVariableRequestDTO(peticion?.PenalidadInmediata);
        }


    }
}
