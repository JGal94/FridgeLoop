using Frontend_Proyecto_Fridgeloop.Entidades.Entity;
using Frontend_Proyecto_Fridgeloop.Helpers;
using Frontend_Proyecto_Fridgeloop.Services;
using Newtonsoft.Json;  // por Constants.BaseApi si lo usa tu base
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using System.Text.Json;


namespace Frontend_Proyecto_Fridgeloop.Services

{
    /// <summary>
    /// Servicio de Productos conectado a la API.
    /// Requiere que HttpServiceBase exista con: Http (HttpClient), J(object), SendAsync<T>(..., ct).
    /// </summary>
    public class ProductService : HttpServiceBase
    {
        public ProductService() : base() { }



        public class ResObtenerProductoPorCodigo : ResBase
        {
            public ProductoApiDto? producto { get; set; }
            public List<ProductoApiDto>? productos { get; set; }
        }

        public async Task<ProductoDto?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
        {
            var url = "api/producto/obtenerporcodigo";

            var res = await SendAsync<ResObtenerProductoPorCodigo>(
                () => Http.PostAsync(url, J(new { codigoBarras = codigo }), ct),
                ct);

            if (res == null || res.resultado != true) return null;

            var p = res.producto ?? res.productos?.FirstOrDefault();
            if (p == null) return null;

            return new ProductoDto
            {
                ProductID = p.idProducto ?? 0,
                Name = p.nombre,
                CategoryID = p.idCategoria,
                Unit = p.unidad,
                Quantity = p.quantity,
                ExpirationDate = p.expirationDate
            };
        }



        // ================= DTOs base (comunes) =================
        public class ErrorDto { public int ErrorCode { get; set; } public string Message { get; set; } = ""; }

        public class ResBase
        {
            public bool resultado { get; set; }
            public string? mensaje { get; set; }
            public List<ErrorDto> listaDeErrores { get; set; } = new();
        }

        // ======= CONTRATO DEL BACKEND (definitivo) =======

        // GET /api/producto/obtener -> devuelve lista con "idProducto", "nombre", "idCategoria", "unidad", "quantity", "expirationDate"
        public class ProductoApiDto
        {
            public int? idProducto { get; set; }
            public string nombre { get; set; } = "";
            public int idCategoria { get; set; }
            public string unidad { get; set; } = "";
            public decimal? quantity { get; set; }
            public DateTime? expirationDate { get; set; }
        }

        public class ResObtenerProductos : ResBase
        {
            public List<ProductoApiDto> productos { get; set; } = new();
        }

        // POST /api/producto/insertar -> body: { productos: { ... } }
        public class ResInsertarProducto : ResBase
        {
            public int? idProducto { get; set; }
        }

        // (opcional) por-vencer
        public class ProductoPorVencerApi
        {
            public int ProductID { get; set; }
            public string Name { get; set; } = "";
            public int CategoryID { get; set; }
            public string Unit { get; set; } = "";
            public decimal? Quantity { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public int DiasRestantes { get; set; }
        }
        public class ResProductosPorVencer : ResBase
        {
            public List<ProductoPorVencerApi> productos { get; set; } = new();
        }

        // ======= DTO usado en la UI (para bindings en XAML) =======
        public class ProductoDto
        {
            public int ProductID { get; set; }
            public string Name { get; set; } = "";
            public int CategoryID { get; set; }
            public string Unit { get; set; } = "";
            public decimal? Quantity { get; set; }
            public DateTime? ExpirationDate { get; set; }
        }

        // ================= Métodos =================

        /// <summary>
        /// POST /api/producto/insertar  — body: { productos = { nombre, idCategoria, unidad, quantity?, expirationDate? } }
        /// </summary>
        public Task<ResInsertarProducto?> InsertarAsync(ProductoDto p, CancellationToken ct = default)
        {
            var body = new
            {
                productos = new
                {
                    nombre = p.Name,
                    idCategoria = p.CategoryID,
                    unidad = p.Unit,
                    quantity = p.Quantity,
                    expirationDate = p.ExpirationDate
                }
            };

            return SendAsync<ResInsertarProducto>(() => Http.PostAsync("api/producto/insertar", J(body), ct), ct);
        }

        /// <summary>
        /// GET /api/producto/obtener — devuelve español; mapeamos a ProductoDto para la UI.
        /// </summary>
        public async Task<List<ProductoDto>?> ObtenerListaAsync(int page = 1, int pageSize = 20, string? q = null, CancellationToken ct = default)
        {
            var url = $"api/producto/obtener?page={page}&pageSize={pageSize}" +
                      (string.IsNullOrWhiteSpace(q) ? "" : $"&q={Uri.EscapeDataString(q)}");

            var res = await SendAsync<ResObtenerProductos>(() => Http.GetAsync(url, ct), ct);
            if (res == null || res.resultado != true) return null;

            return res.productos.Select(p => new ProductoDto
            {
                ProductID = p.idProducto ?? 0,
                Name = p.nombre,
                CategoryID = p.idCategoria,
                Unit = p.unidad,
                Quantity = p.quantity,
                ExpirationDate = p.expirationDate
            }).ToList();
        }

        /// <summary>
        /// Solo para depurar: devuelve el JSON crudo del GET /obtener.
        /// </summary>
        public async Task<string> ObtenerRawAsync(int page = 1, int pageSize = 20, string? q = null, CancellationToken ct = default)
        {
            var url = $"api/producto/obtener?page={page}&pageSize={pageSize}" +
                      (string.IsNullOrWhiteSpace(q) ? "" : $"&q={Uri.EscapeDataString(q)}");
            var r = await Http.GetAsync(url, ct);
            return await r.Content.ReadAsStringAsync(ct);
        }

        /// <summary>
        /// PUT /api/producto/inventario — cambia cantidad/fecha (body en español).
        /// </summary>
        public Task<ResBase?> ActualizarInventarioAsync(int productId, decimal? qty, DateTime? exp, CancellationToken ct = default)
        {
            var apiBody = new
            {
                idProducto = productId,
                cantidad = qty,     // puede ir null
                fechaExpiracion = exp      // puede ir null
            };

            return SendAsync<ResBase>(() => Http.PutAsync("api/producto/inventario", J(apiBody), ct), ct);
        }

        /// <summary>
        /// (Opcional) GET /api/producto/porvencer
        /// </summary>
        public Task<ResProductosPorVencer?> ObtenerPorVencerAsync(int dias = 7, bool incluirVencidos = false, int maxDiasVencidos = 7, int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            var url = $"api/producto/porvencer?dias={dias}&incluirVencidos={incluirVencidos}&maxDiasVencidos={maxDiasVencidos}&page={page}&pageSize={pageSize}";
            return SendAsync<ResProductosPorVencer>(() => Http.GetAsync(url, ct), ct);
        }

        // Helper de errores legibles
        public static string FirstError(ResBase? r, string fallback = "Ocurrió un error.")
            => r?.listaDeErrores?.FirstOrDefault()?.Message ?? r?.mensaje ?? fallback;
    }
}

