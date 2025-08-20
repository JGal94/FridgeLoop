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
using Newtonsoft.Json.Linq;


namespace Frontend_Proyecto_Fridgeloop.Services

{
    public class ProductService : HttpServiceBase
    {
        public ProductService() : base() { }

        // ========= DTOs base =========
        public class ErrorDto { public int ErrorCode { get; set; } public string Message { get; set; } = ""; }

        public class ResBase
        {
            public bool resultado { get; set; }
            public string? mensaje { get; set; }
            public List<ErrorDto> listaDeErrores { get; set; } = new();
        }

        // ========= API (ES) =========
        public class ProductoApiDto
        {
            // Variantes vistas en APIs
            public int? idProducto { get; set; }
            public int? IdProducto { get; set; }
            public int? productId { get; set; }
            public int? productID { get; set; }
            public int? ProductID { get; set; }
            public int? productoId { get; set; }
            public int? ProductoID { get; set; }
            public int? id { get; set; }
            public int? Id { get; set; }
            public int? ID { get; set; }

            public string nombre { get; set; } = "";
            public int idCategoria { get; set; }
            public string unidad { get; set; } = "";
            public decimal? quantity { get; set; }
            public DateTime? expirationDate { get; set; }
        }

        public class ResInsertarProducto : ResBase
        {
            public int? idProducto { get; set; }
        }

        public class ResObtenerProductos : ResBase
        {
            public List<ProductoApiDto> productos { get; set; } = new();
        }

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

        // ========= DTO para la UI (EN) =========
        public class ProductoDto
        {
            public int ProductID { get; set; }
            public string Name { get; set; } = "";
            public int CategoryID { get; set; }
            public string Unit { get; set; } = "";
            public int UserID { get; set; }
            public decimal? Quantity { get; set; }
            public DateTime? ExpirationDate { get; set; }
        }

        public class ReqActualizarProducto
        {
            public int ProductID { get; set; }
            public int UserID { get; set; }
            public string Name { get; set; } = "";
            public int CategoryID { get; set; }
            public string Unit { get; set; } = "";
        }

        public class ReqActualizarInventario
        {
            public int UserID { get; set; }
            public int ProductID { get; set; }
            public decimal? Quantity { get; set; }
            public DateTime? ExpirationDate { get; set; }
        }

        // ========= Métodos =========

        // POST /api/producto/insertar — body: { productos = { nombre, idCategoria, unidad, quantity?, expirationDate? } }
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

        // GET /api/producto/obtener — mapeo + fallback para id
        public async Task<List<ProductoDto>?> ObtenerListaAsync(int page = 1, int pageSize = 20, string? q = null, CancellationToken ct = default)
        {
            var url = $"api/producto/obtener?page={page}&pageSize={pageSize}" +
                      (string.IsNullOrWhiteSpace(q) ? "" : $"&q={Uri.EscapeDataString(q)}");

            var res = await SendAsync<ResObtenerProductos>(() => Http.GetAsync(url, ct), ct);
            if (res == null || res.resultado != true)
                return null;

            // Mapeo directo a DTO de UI
            var list = res.productos.Select(p => new ProductoDto
            {
                ProductID = (p.idProducto ?? p.IdProducto ?? p.ProductID ?? p.productID ?? p.productId ?? p.productoId ?? p.ProductoID ?? p.id ?? p.Id ?? p.ID ?? 0),
                Name = p.nombre,
                CategoryID = p.idCategoria,
                Unit = p.unidad,
                Quantity = p.quantity,
                ExpirationDate = p.expirationDate
            }).ToList();

            // ¿Quedó alguien con id = 0? — Fallback: examina JSON crudo y extrae id
            if (list.Any(x => x.ProductID == 0))
            {
                var raw = await ObtenerRawAsync(page, pageSize, q, ct);
                try
                {
                    var jo = JObject.Parse(raw);
                    var productos = jo["productos"] as JArray;
                    if (productos != null)
                    {
                        // regex que cubre variantes comunes del id de producto
                        var idRegex = new Regex(@"^(idProducto|IdProducto|productoId|ProductoID|productId|productID|ProductID|id|Id|ID)$",
                                                RegexOptions.IgnoreCase);

                        for (int i = 0; i < productos.Count && i < list.Count; i++)
                        {
                            if (list[i].ProductID != 0) continue;

                            var obj = (JObject)productos[i];
                            var prop = obj.Properties()
                                          .FirstOrDefault(p => idRegex.IsMatch(p.Name) && (p.Value.Type == JTokenType.Integer || p.Value.Type == JTokenType.Float));

                            if (prop != null && int.TryParse(prop.Value.ToString(), out var idVal))
                                list[i].ProductID = idVal;
                        }
                    }
                }
                catch { /* ignoramos parsing errors del fallback */ }
            }

            return list;
        }

        // JSON crudo del GET (útil para depurar)
        public async Task<string> ObtenerRawAsync(int page = 1, int pageSize = 20, string? q = null, CancellationToken ct = default)
        {
            var url = $"api/producto/obtener?page={page}&pageSize={pageSize}" +
                      (string.IsNullOrWhiteSpace(q) ? "" : $"&q={Uri.EscapeDataString(q)}");
            var r = await Http.GetAsync(url, ct);
            return await r.Content.ReadAsStringAsync(ct);
        }

        // PUT /api/producto/actualizar — editar metadatos (si lo usas)
        public Task<ResBase?> ActualizarProductoAsync(ReqActualizarProducto body, CancellationToken ct = default) =>
            SendAsync<ResBase>(() => Http.PutAsync("api/producto/actualizar", J(body), ct), ct);

        // PUT /api/producto/inventario — body en español (idProducto, cantidad, fechaExpiracion)
        public Task<ResBase?> ActualizarInventarioAsync(ReqActualizarInventario body, CancellationToken ct = default)
        {
            var apiBody = new
            {
                idProducto = body.ProductID,
                cantidad = body.Quantity,
                fechaExpiracion = body.ExpirationDate
            };
            return SendAsync<ResBase>(() => Http.PutAsync("api/producto/inventario", J(apiBody), ct), ct);
        }

        // GET /api/producto/porvencer
        public Task<ResProductosPorVencer?> ObtenerPorVencerAsync(int dias = 7, bool incluirVencidos = false, int maxDiasVencidos = 7, int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            var url = $"api/producto/porvencer?dias={dias}&incluirVencidos={incluirVencidos}&maxDiasVencidos={maxDiasVencidos}&page={page}&pageSize={pageSize}";
            return SendAsync<ResProductosPorVencer>(() => Http.GetAsync(url, ct), ct);
        }

        // Helper de errores legibles
        public static string FirstError(ResBase? r, string fallback = "Ocurrió un error.")
            => r?.listaDeErrores?.FirstOrDefault()?.Message ?? r?.mensaje ?? fallback;

        // --------- Resolver id on‑demand por nombre (por si el ítem seleccionado llega sin id) ---------
        public async Task<int> ResolverIdPorNombreAsync(string name, CancellationToken ct = default)
        {
            var lista = await ObtenerListaAsync(1, 5, name, ct) ?? new List<ProductoDto>();
            var match = lista.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            return match?.ProductID ?? 0;
        }
    }
}

