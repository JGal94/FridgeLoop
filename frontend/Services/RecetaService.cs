using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Frontend_Proyecto_Fridgeloop.Helpers;
using Newtonsoft.Json;

namespace Frontend_Proyecto_Fridgeloop.Services
{
    public class RecetaService : HttpServiceBase
    {
        // =========================
        // DTOs base (normalizados)
        // =========================
        public class ErrorDto
        {
            public int ErrorCode { get; set; }
            public string Message { get; set; } = "";
        }

        public class ResBase
        {
            public bool resultado { get; set; }
            public string? mensaje { get; set; }
            public List<ErrorDto>? listaDeErrores { get; set; }
        }

        public class IngredienteDto
        {
            public string? Nombre { get; set; }
            public string? Unidad { get; set; }
            public decimal? Cantidad { get; set; }
        }

        public class RecetaDto
        {
            public int RecipeID { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public int PreparationTime { get; set; }
            public string? Difficulty { get; set; }
            public int Calories { get; set; }
            public string? Style { get; set; }
            public List<IngredienteDto>? Ingredients { get; set; }
        }

        public class ResRecetasIA : ResBase
        {
            public List<RecetaDto>? recetas { get; set; }
        }

        public class ResInsertarReceta
        {
            public bool Exito { get; set; }
            public string Mensaje { get; set; } = "";
        }

        public static string FirstError(ResBase? r, string fallback = "Ocurrió un error.")
            => r?.listaDeErrores?.FirstOrDefault()?.Message ?? r?.mensaje ?? fallback;

        // =========================================================
        // RAW DTOs para deserializar exactamente lo que envía la IA
        // (Ej.: Ingredients con { "nombre": "...", "Cantidad": "500 g" })
        // =========================================================
        private class IngredienteRaw
        {
            public string? nombre { get; set; }
            public string? Cantidad { get; set; }    // llega como string (ej. "500 g", "2 pz")
        }

        private class RecetaRaw
        {
            public int RecipeID { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public int PreparationTime { get; set; }
            public string? Difficulty { get; set; }
            public int Calories { get; set; }
            public string? Style { get; set; }
            public List<IngredienteRaw>? Ingredients { get; set; }
        }

        private class ResRecetasIA_Raw : ResBase
        {
            public List<RecetaRaw>? recetas { get; set; }
        }

        // =========================
        // Helpers internos
        // =========================

        /// <summary>
        /// Aplica el Authorization: Bearer {token} al HttpClient si existe.
        /// </summary>
        private void ApplyBearerIfAny()
        {
            try
            {
                var token = Helpers.Sesion.Token; // JWT guardado tras login
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var current = Http.DefaultRequestHeaders.Authorization;
                    if (current == null || current.Scheme != "Bearer" || current.Parameter != token)
                    {
                        Http.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                    }
                }
                else
                {
                    Http.DefaultRequestHeaders.Authorization = null;
                }
            }
            catch
            {
                // No romper si falla; el backend responderá 401 si es necesario.
            }
        }

        /// <summary>
        /// Post que devuelve el payload como texto. Lanza HttpRequestException si el status no es 2xx.
        /// </summary>
        private async Task<string> PostTextAsync(string url, HttpContent content, CancellationToken ct)
        {
            ApplyBearerIfAny();
            using var resp = await Http.PostAsync(url, content, ct);
            var payload = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Status {(int)resp.StatusCode}: {payload}");
            }
            return payload;
        }

        /// <summary>
        /// Intenta extraer un número de un texto ("500 g" -> 500).
        /// </summary>
        private static decimal? TryParseCantidad(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            // Toma solo los dígitos/., y cambia coma por punto por si viene "1,5"
            var filtered = new string(raw.Select(c => char.IsDigit(c) || c == '.' || c == ',' ? c : ' ').ToArray())
                .Replace(',', '.')
                .Trim();

            // Toma la primera "palabra" que parezca número
            var part = filtered.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                               .FirstOrDefault();
            if (decimal.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                return val;

            return null;
        }

        private static string? InferUnidad(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var lower = raw.ToLowerInvariant();
            if (lower.Contains("kg")) return "kg";
            if (lower.Contains("g")) return "g";
            if (lower.Contains("ml")) return "ml";
            if (lower.Contains("l ")) return "l";
            if (lower.Contains("pz") || lower.Contains("pieza") || lower.Contains("unid") || lower.Contains("unidad"))
                return "pz";
            return null; // deja null si no se puede inferir
        }

        private static RecetaDto Map(RecetaRaw r)
        {
            var ingredientes = (r.Ingredients ?? new List<IngredienteRaw>())
                .Select(i => new IngredienteDto
                {
                    Nombre = i.nombre,
                    Cantidad = TryParseCantidad(i.Cantidad),
                    Unidad = InferUnidad(i.Cantidad)
                })
                .ToList();

            return new RecetaDto
            {
                RecipeID = r.RecipeID,
                Name = r.Name,
                Description = r.Description,
                PreparationTime = r.PreparationTime,
                Difficulty = r.Difficulty,
                Calories = r.Calories,
                Style = r.Style,
                Ingredients = ingredientes
            };
        }

        // =========================
        // ENDPOINTS
        // =========================

        /// <summary>
        /// IA: POST /api/receta/ia
        /// El backend toma el idUsuario del JWT. Enviamos un body mínimo por compatibilidad.
        /// </summary>
        public async Task<ResRecetasIA?> ObtenerRecetasIAAsync(CancellationToken ct = default)
        {
            try
            {
                var body = new
                {
                    idUsuario = Sesion.Id > 0 ? Sesion.Id : 0
                };

                // Timeout por llamada
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linked.CancelAfter(TimeSpan.FromSeconds(75));

                var json = await PostTextAsync("api/receta/ia", J(body), linked.Token);

                // Deserializa a RAW y mapea a DTO normalizado
                var raw = JsonConvert.DeserializeObject<ResRecetasIA_Raw>(json);
                if (raw == null)
                    return null;

                return new ResRecetasIA
                {
                    resultado = raw.resultado,
                    mensaje = raw.mensaje,
                    listaDeErrores = raw.listaDeErrores,
                    recetas = raw.recetas?.Select(Map).ToList()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RecetaService] ERROR ia: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Preparar receta: POST /api/receta/preparar
        /// </summary>
        public async Task<ResInsertarReceta?> PrepararRecetaAsync(RecetaDto receta, CancellationToken ct = default)
        {
            var ingredientes = (receta.Ingredients ?? new List<IngredienteDto>())
                .Select(i => new
                {
                    nombre = i.Nombre ?? "",
                    unidad = i.Unidad ?? "",
                    cantidad = i.Cantidad ?? 0m
                }).ToList();

            var body = new
            {
                // El UserID se toma del token en el backend
                receta = new
                {
                    RecipeID = receta.RecipeID,
                    Name = receta.Name ?? "",
                    Description = receta.Description ?? "",
                    PreparationTime = receta.PreparationTime,
                    Difficulty = receta.Difficulty ?? "",
                    Calories = receta.Calories,
                    Style = receta.Style ?? ""
                },
                // Algunas implementaciones esperan "Ingredientes" y otras "Ingredients"
                Ingredientes = ingredientes,
                Ingredients = ingredientes
            };

            return await SendAsync<ResInsertarReceta>(
                () =>
                {
                    ApplyBearerIfAny();
                    return Http.PostAsync("api/receta/preparar", J(body), ct);
                },
                ct
            );
        }
    }
}
