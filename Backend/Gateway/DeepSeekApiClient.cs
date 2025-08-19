using Gateway.Request;
using Gateway.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity; // ← Para acceder a Productos y Receta

namespace Gateway
{
    public sealed class DeepSeekApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "sk-768e8b2116f148328d53458675ab0f1a";
        private const string ApiBaseUrl = "https://api.deepseek.com/v1"; // Reemplazar con URL real

        //public DeepSeekApiClient()
        //{ }
        public DeepSeekApiClient()
        {
           // _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<DeepSeekResponse> SendChatRequestAsync(DeepSeekRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string endpoint = $"{ApiBaseUrl}/chat/completions";
            string jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                using (var response = await _httpClient.PostAsync(endpoint, content))
                {
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DeepSeekResponse>(jsonResponse);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error HTTP: {ex.InnerException?.Source ?? "Unknown"} - {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Timeout: La solicitud tardó demasiado.");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Error al parsear la respuesta JSON: {ex.Message}");
            }
        }

        public async Task<List<Receta>> ObtenerRecetasDesdeIngredientes(List<Productos> productos)
        {
            if (productos == null || !productos.Any())
                throw new ArgumentException("La lista de productos no puede estar vacía.");

            // 1. Generar prompt con ingredientes del usuario
            var ingredientesPrompt = string.Join(", ", productos.Select(p => $"{p.nombre} ({p.quantity} {p.unidad})"));
            string prompt = $"Tengo los siguientes ingredientes en casa: {ingredientesPrompt}. " +
                            "¿Podés sugerirme 3 recetas en formato JSON? Cada receta debe tener: nombre, descripción, calorías, dificultad, tiempo de preparación (en minutos), estilo y lista de ingredientes (nombre y cantidad).";

            // 2. Construir el request a DeepSeek
            var request = new DeepSeekRequest
            {
                Model = "deepseek-chat", // ← Usar modelo correcto si es distinto
                Messages = new List<ChatMessage>
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = prompt
                    }
                }
            };

            // 3. Enviar request a la IA
            var response = await SendChatRequestAsync(request);

            string rawJson = response?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(rawJson))
                throw new Exception("La respuesta de la IA está vacía o no fue válida.");

            try
            {
                var recetas = JsonConvert.DeserializeObject<List<Receta>>(rawJson);
                return recetas ?? new List<Receta>();
            }
            catch (JsonException ex)
            {
                throw new Exception("Error al convertir la respuesta de la IA a recetas: " + ex.Message);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
