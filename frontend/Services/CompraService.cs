using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frontend_Proyecto_Fridgeloop.Helpers;
using Newtonsoft.Json.Linq; // <-- usamos JObject / JArray

namespace Frontend_Proyecto_Fridgeloop.Services
{
    // Usa el mismo HttpServiceBase que ya tienes (Http, J, SendAsync<T>)
    public class CompraService : HttpServiceBase
    {
        // ====================== Base ======================
        public class ResBase
        {
            public bool resultado { get; set; }
            public string? mensaje { get; set; }
            public List<ErrorDto>? listaDeErrores { get; set; }
        }

        public class ErrorDto
        {
            public int ErrorCode { get; set; }
            public string Message { get; set; } = "";
        }

        public static string FirstError(ResBase? r, string fallback = "Ocurrió un error.")
            => r?.listaDeErrores?.FirstOrDefault()?.Message ?? r?.mensaje ?? fallback;

        // ================= Registrar compra ===============
        public class ResRegistrarCompra : ResBase
        {
            public int idCompra { get; set; }
            public decimal total { get; set; }
        }

        /// <summary>
        /// Contrato backend: ReqRegistrarCompra + ItemCompraSinId
        /// POST /api/compra/registrar
        /// </summary>
        public async Task<ResRegistrarCompra?> RegistrarCompraAsync(
            DateTime? fecha, IEnumerable<ShoppingItem> items, CancellationToken ct = default)
        {
            var body = new
            {
                fechaCompra = fecha,
                items = items.Select(i => new
                {
                    nombre = i.Nombre,
                    idCategoria = i.IdCategoria,
                    unidad = i.Unidad,
                    cantidad = i.Cantidad,
                    precioUnitario = i.PrecioUnitario,
                    fechaExpiracion = i.FechaExpiracion
                }).ToList()
            };

            return await SendAsync<ResRegistrarCompra>(
                () => Http.PostAsync("api/compra/registrar", J(body), ct), ct);
        }

        // ================= Historial compras ==============
        public class ItemCompraDto
        {
            public string? nombre { get; set; }
            public decimal cantidad { get; set; }
            public string? unidad { get; set; }
            public decimal? precioUnitario { get; set; }
        }

        public class CompraDto
        {
            public int idCompra { get; set; }
            public DateTime? fechaCompra { get; set; }
            public decimal total { get; set; }
            public List<ItemCompraDto>? items { get; set; }
        }

        public class ResObtenerCompras : ResBase
        {
            public List<CompraDto>? compras { get; set; }

            // Si tu backend devuelve estos campos, quedan mapeados.
            public int page { get; set; }
            public int pageSize { get; set; }
            public int totalRows { get; set; }
        }

        /// <summary>
        /// GET /api/compra/obtener?page=1&pageSize=20
        /// Si el backend difiere en nombres/campos, se intenta un mapeo alternativo.
        /// </summary>
        public async Task<ResObtenerCompras?> ObtenerComprasAsync(
            int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var url = $"api/compra/obtener?page={page}&pageSize={pageSize}";

            // 1) Intento directo con deserialización tipada
            var directo = await SendAsync<ResObtenerCompras>(() => Http.GetAsync(url, ct), ct);
            if (directo != null && (directo.resultado || (directo.compras != null)))
                return directo;

            // 2) Fallback tolerante: leo JSON crudo y busco rutas alternas
            try
            {
                var httpRes = await Http.GetAsync(url, ct);
                var raw = await httpRes.Content.ReadAsStringAsync(ct);

                var jo = JObject.Parse(raw);

                // helper para probar múltiples rutas
                static JToken? PickToken(JObject root, params string[] paths)
                {
                    foreach (var p in paths)
                    {
                        var t = root.SelectToken(p);
                        if (t != null) return t;
                    }
                    return null;
                }

                var res = new ResObtenerCompras
                {
                    resultado = (bool?)PickToken(jo, "resultado", "success", "ok") ?? true,
                    mensaje = (string?)PickToken(jo, "mensaje", "message", "error"),
                    compras = new List<CompraDto>()
                };

                // intenta encontrar el array de compras en distintas claves
                var comprasTok = PickToken(jo, "compras", "lista", "data", "result.compras", "payload.compras");
                if (comprasTok is JArray arr)
                {
                    foreach (var c in arr)
                    {
                        var comp = new CompraDto
                        {
                            idCompra = (int?)(c["idCompra"] ?? c["Id"] ?? c["id"]) ?? 0,
                            fechaCompra = (DateTime?)(c["fechaCompra"] ?? c["Fecha"] ?? c["date"]),
                            total = (decimal?)(c["total"] ?? c["Total"] ?? c["montoTotal"]) ?? 0m,
                            items = new List<ItemCompraDto>()
                        };

                        var itemsTok = c["items"] ?? c["detalle"] ?? c["productos"];
                        if (itemsTok is JArray itemsArr)
                        {
                            foreach (var it in itemsArr)
                            {
                                comp.items!.Add(new ItemCompraDto
                                {
                                    nombre = (string?)(it["nombre"] ?? it["Name"] ?? it["producto"] ?? it["Producto"]),
                                    cantidad = (decimal?)(it["cantidad"] ?? it["Cantidad"] ?? it["qty"]) ?? 0,
                                    unidad = (string?)(it["unidad"] ?? it["Unidad"] ?? it["unit"]),
                                    precioUnitario = (decimal?)(it["precioUnitario"] ?? it["Precio"] ?? it["price"])
                                });
                            }
                        }

                        res.compras!.Add(comp);
                    }
                }

                // mapea paginación si existe
                res.page = (int?)(PickToken(jo, "page", "pagina") ?? 1) ?? 1;
                res.pageSize = (int?)(PickToken(jo, "pageSize", "tamanoPagina") ?? pageSize) ?? pageSize;
                res.totalRows = (int?)(PickToken(jo, "totalRows", "total") ?? 0) ?? 0;

                return res;
            }
            catch
            {
                // Retorna lo directo (con su mensaje) o un error genérico
                return directo ?? new ResObtenerCompras
                {
                    resultado = false,
                    mensaje = "No se pudo leer la respuesta del servidor."
                };
            }
        }
    }
}
