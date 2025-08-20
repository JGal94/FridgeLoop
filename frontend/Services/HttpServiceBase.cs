using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontend_Proyecto_Fridgeloop.Helpers;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;



namespace Frontend_Proyecto_Fridgeloop.Services
{
    public abstract class HttpServiceBase
    {
        // HttpClient común para todos los servicios
        protected readonly HttpClient Http;

        // Constructor SIN parámetros (como pedías)
        protected HttpServiceBase()
        {
            Http = new HttpClient
            {
                BaseAddress = new Uri(Constants.BaseApi),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        // 1) Pone el bearer si existe en SecureStorage
        protected async Task EnsureBearerAsync()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                Http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Http.DefaultRequestHeaders.Authorization = null;
            }
        }

        // 2) Serializa a JSON
        protected StringContent J(object body) =>
            new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        // 3) Envío robusto con reintentos y CancellationToken requerido (firma que usa ProductService)
        protected async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> send, CancellationToken ct, int maxRetries = 2)
        {
            await EnsureBearerAsync().ConfigureAwait(false);

            var attempt = 0;
            var delay = TimeSpan.FromMilliseconds(400);

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    using var res = await send().ConfigureAwait(false);
                    var payload = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                    if ((int)res.StatusCode >= 200 && (int)res.StatusCode < 300)
                    {
                        try { return JsonConvert.DeserializeObject<T>(payload); }
                        catch { return default; }
                    }

                    // 4xx (excepto 429) => no reintentar
                    if ((int)res.StatusCode >= 400 && (int)res.StatusCode < 500 && res.StatusCode != (HttpStatusCode)429)
                        throw new HttpRequestException($"Error {(int)res.StatusCode}: {payload}");

                    // 5xx o 429 => reintentos exponenciales
                    attempt++;
                    if (attempt > maxRetries)
                        throw new HttpRequestException($"Error {(int)res.StatusCode}: {payload}");

                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested)
                {
                    // timeout -> reintentar
                    attempt++;
                    if (attempt > maxRetries) throw;
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
            }
        }
    }
}
