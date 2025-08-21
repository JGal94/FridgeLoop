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
        protected readonly HttpClient Http;

        protected HttpServiceBase()
        {
#if DEBUG && ANDROID
            // Solo DEBUG en Android: aceptar cert de desarrollo (Kestrel).
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };
            Http = new HttpClient(handler)
            {
                BaseAddress = new Uri(Constants.BaseApi),
                Timeout = TimeSpan.FromSeconds(30)
            };
#else
            Http = new HttpClient
            {
                BaseAddress = new Uri(Constants.BaseApi),
                Timeout = TimeSpan.FromSeconds(30)
            };
#endif
            if (!Http.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")))
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Establece el Authorization: Bearer {token} si existe en SecureStorage ("auth_token").
        /// No reasigna si ya es el mismo valor.
        /// </summary>
        protected async Task EnsureBearerAsync()
        {
            var token = await SecureStorage.GetAsync("auth_token"); // <- cambia la clave si usas otra
            var current = Http.DefaultRequestHeaders.Authorization;

            if (!string.IsNullOrWhiteSpace(token))
            {
                if (current == null || current.Scheme != "Bearer" || current.Parameter != token)
                    Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                if (current != null) Http.DefaultRequestHeaders.Authorization = null;
            }
        }

        protected StringContent J(object body) =>
            new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        /// <summary>
        /// Envío robusto: adjunta Bearer, reintenta 5xx/timeout/429 con backoff
        /// y loguea body en errores para diagnóstico.
        /// </summary>
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
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"[HttpServiceBase] Falló deserialización (200). Payload:\n{payload}");
                            return default;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[HTTP ERROR] {(int)res.StatusCode} {res.StatusCode}\n{payload}");

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
                    // Timeout => reintentar
                    attempt++;
                    if (attempt > maxRetries) throw;
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
            }
        }
    }
}
