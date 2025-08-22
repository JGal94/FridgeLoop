using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Services
{
    public class NotificacionesService : HttpServiceBase
    {
        // ===== Base de respuestas =====
        public class ErrorDto { public int ErrorCode { get; set; } public string Message { get; set; } = ""; }
        public class ResBase { public bool resultado { get; set; } public string? mensaje { get; set; } public List<ErrorDto>? listaDeErrores { get; set; } }

        public static string FirstError(ResBase? r, string fallback = "Ocurrió un error.")
            => r?.listaDeErrores?.FirstOrDefault()?.Message ?? r?.mensaje ?? fallback;

        // ====== DTO segun backend /api/producto/porvencer ======
        public class ProductoPorVencerDto
        {
            public int idProducto { get; set; }
            public string? nombre { get; set; }
            public int idCategoria { get; set; }
            public string? unidad { get; set; }
            public decimal cantidad { get; set; }
            public DateTime? fechaExpiracion { get; set; }
            public int diasRestantes { get; set; }
        }

        public class ResProductosPorVencer : ResBase
        {
            public List<ProductoPorVencerDto>? productos { get; set; }
            // (si el backend agrega paginación, puedes añadir page, pageSize, total, etc.)
        }

        // ====== Modelo para UI ======
        public class NotificacionItem
        {
            public int IdProducto { get; set; }
            public string Titulo { get; set; } = "";
            public string Subtitulo { get; set; } = "";
            public string Tipo { get; set; } = "info"; // info|warning|danger
            public DateTime? FechaExpira { get; set; }
            public int Dias { get; set; }
            public string? Unidad { get; set; }
            public decimal? Cantidad { get; set; }
        }

        /// <summary>
        /// Llama GET /api/producto/porvencer?dias=7&incluirVencidos=true&maxDiasVencidos=7&page=1&pageSize=50
        /// </summary>
        public async Task<List<NotificacionItem>> ObtenerNotificacionesAsync(
            int dias = 7, bool incluirVencidos = true, int maxDiasVencidos = 7,
            int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            // OJO: ruta exacta del backend: "porvencer" (sin guion)
            var url = $"api/producto/porvencer?dias={dias}&incluirVencidos={incluirVencidos}&maxDiasVencidos={maxDiasVencidos}&page={page}&pageSize={pageSize}";
            var res = await SendAsync<ResProductosPorVencer>(() => Http.GetAsync(url, ct), ct);

            var list = new List<NotificacionItem>();

            if (res?.resultado == true && res.productos != null)
            {
                foreach (var p in res.productos)
                {
                    var tipo = p.diasRestantes < 0 ? "danger" : (p.diasRestantes <= 3 ? "warning" : "info");

                    list.Add(new NotificacionItem
                    {
                        IdProducto = p.idProducto,
                        Titulo = p.nombre ?? "Producto",
                        Subtitulo = p.diasRestantes < 0
                            ? $"Vencido hace {Math.Abs(p.diasRestantes)} día(s)."
                            : $"Expira en {p.diasRestantes} día(s).",
                        Tipo = tipo,
                        FechaExpira = p.fechaExpiracion,
                        Dias = p.diasRestantes,
                        Unidad = p.unidad,
                        Cantidad = p.cantidad
                    });
                }
            }

            return list;
        }

        /// <summary>
        /// Útil para depurar si algo sale vacío. Devuelve el JSON crudo.
        /// </summary>
        public async Task<string> ObtenerNotificacionesRawAsync(
            int dias = 7, bool incluirVencidos = true, int maxDiasVencidos = 7,
            int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            var url = $"api/producto/porvencer?dias={dias}&incluirVencidos={incluirVencidos}&maxDiasVencidos={maxDiasVencidos}&page={page}&pageSize={pageSize}";
            await EnsureBearerAsync();
            using var res = await Http.GetAsync(url, ct);
            return await res.Content.ReadAsStringAsync(ct);
        }
    }
}
