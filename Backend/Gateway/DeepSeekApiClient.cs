using Gateway.Request;
using Gateway.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace Gateway
{
    public sealed class DeepSeekApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiBaseUrl = "https://api.deepseek.com/v1"; // Reemplazar con la URL real

        public DeepSeekApiClient(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
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
                throw new Exception($"Error HTTP: {ex.InnerException.Source} - {ex.Message}");
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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
