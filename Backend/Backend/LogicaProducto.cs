using AccesoDatos;
using Entidades.Entity;
using Entidades.Enum;
using Entidades.Request;
using Entidades.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class LogicaProducto
    {
        // INSERTAR: mantiene tu flujo actual (crea producto y registra inventario si tu SP lo hace)
        public ResInsertarProducto InsertarProducto(int userId, ReqInsertarProducto req)
        {
            var res = new ResInsertarProducto();

            try
            {
                if (userId <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    };
                    return res;
                }

                if (req?.productos == null ||
                    string.IsNullOrWhiteSpace(req.productos.nombre) ||
                    string.IsNullOrWhiteSpace(req.productos.unidad) ||
                    req.productos.idCategoria <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.CampoRequeridoFaltante, Message = "Todos los campos del producto son obligatorios." }
                    };
                    return res;
                }

                // Normaliza
                req.productos.nombre = req.productos.nombre.Trim();
                req.productos.unidad = req.productos.unidad.Trim();

                var expUtc = req.productos.expirationDate?.ToUniversalTime();

                using (var linq = new linqDataContext())
                {
                    int? productID = null;
                    int? errorId = null;
                    string errorMensaje = null;

                    // Tu SP actual: inserta producto (y normalmente también inventario)
                    linq.InsertProduct(
                        req.productos.nombre,
                        req.productos.idCategoria,
                        req.productos.unidad,
                        userId,                         // dueño desde el token
                        req.productos.quantity,
                        expUtc,
                        ref productID,
                        ref errorId,
                        ref errorMensaje
                    );

                    if (errorId.GetValueOrDefault() > 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores = new List<Error> {
                            new Error { ErrorCode = EnumErrores.ErrorDeBaseDatos, Message = errorMensaje ?? "Error al insertar el producto." }
                        };
                        return res;
                    }

                    if (!productID.HasValue || productID.Value <= 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores = new List<Error> {
                            new Error { ErrorCode = EnumErrores.ErrorDeBaseDatos, Message = "No se pudo insertar el producto." }
                        };
                        return res;
                    }

                    res.idProducto = productID.Value;
                    res.resultado = true;
                    res.mensaje = "Producto insertado correctamente.";
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores = new List<Error> {
                    new Error { ErrorCode = EnumErrores.ErrorNoControlado, Message = ex.Message }
                };
            }

            return res;
        }

        // OBTENER: usa GetProductsByUser (paginación + búsqueda). Fallback a GetProducts() si aún no agregas el SP al .dbml
        public ResObtenerProductos ObtenerProductos(int userId, int page = 1, int pageSize = 20, string q = null)
        {
            var res = new ResObtenerProductos { productos = new List<Productos>() };

            try
            {
                if (userId <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores = new List<Error> {
                new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
            };
                    return res;
                }

                using (var linq = new linqDataContext())
                {
                    // Fallback que siempre compila con los SP actuales:
                    var prods = linq.GetProducts().ToList();              // Products: ProductID, Name, CategoryID, Unit
                    var inv = linq.GetUserInventory().ToList();         // UserInventory: InventoryID, UserID, ProductID, Quantity, ExpirationDate

                    var items = (from ui in inv
                                 where ui.UserID == userId
                                 join p in prods on ui.ProductID equals p.ProductID
                                 where string.IsNullOrWhiteSpace(q) || p.Name.Contains(q)
                                 orderby p.Name, p.ProductID
                                 select new { p, ui })
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

                    foreach (var it in items)
                    {
                        res.productos.Add(new Productos
                        {
                            // idProducto = it.p.ProductID,           // descomenta si tu DTO lo tiene
                            nombre = it.p.Name,
                            idCategoria = it.p.CategoryID ?? 0,
                            unidad = it.p.Unit,
                            quantity = it.ui.Quantity,
                            expirationDate = it.ui.ExpirationDate
                        });
                    }

                    res.resultado = true;
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores = new List<Error> {
            new Error { ErrorCode = EnumErrores.ErrorNoControlado, Message = ex.Message }
        };
                return res;
            }
        }


        // ACTUALIZAR METADATOS DEL PRODUCTO (nombre/categoría/unidad)
        public ResActualizarProducto ActualizarProducto(int userId, ReqActualizarProducto req)
        {
            var res = new ResActualizarProducto();

            try
            {
                if (userId <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    };
                    return res;
                }

                if (req == null || req.idProducto <= 0 ||
                    string.IsNullOrWhiteSpace(req.nombre) ||
                    string.IsNullOrWhiteSpace(req.unidad) ||
                    req.idCategoria <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.CampoRequeridoFaltante, Message = "Todos los campos del producto son obligatorios." }
                    };
                    return res;
                }

                req.nombre = req.nombre.Trim();
                req.unidad = req.unidad.Trim();

                using (var linq = new linqDataContext())
                {
                    // Si creaste el SP que asegura pertenencia:
                    // linq.UpdateProductForUser(req.idProducto, userId, req.nombre, req.idCategoria, req.unidad);
                    // Fallback: UpdateProduct "global" (NO valida pertenencia)
                    linq.UpdateProduct(
                        req.idProducto,
                        req.nombre,
                        req.idCategoria,
                        req.unidad
                    );

                    res.resultado = true;
                    res.mensaje = "Producto actualizado correctamente.";
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores = new List<Error> {
                    new Error { ErrorCode = EnumErrores.ErrorNoControlado, Message = ex.Message }
                };
            }

            return res;
        }

        // NUEVO: ACTUALIZAR INVENTARIO DEL USUARIO (cantidad/fecha)
        public ResActualizarProducto ActualizarInventario(int userId, ReqActualizarInventario req)
        {
            var res = new ResActualizarProducto();

            try
            {
                if (userId <= 0 || req == null || req.idProducto <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.CampoRequeridoFaltante, Message = "UserId y datos de inventario son obligatorios." }
                    };
                    return res;
                }

                var expUtc = req.fechaExpiracion?.ToUniversalTime();

                using (var linq = new linqDataContext())
                {
                    linq.UpdateUserInventoryForUser(
                        userId,
                        req.idProducto,
                        req.cantidad,
                        expUtc
                    );

                    res.resultado = true;
                    res.mensaje = "Inventario actualizado correctamente.";
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores = new List<Error> {
                    new Error { ErrorCode = EnumErrores.ErrorNoControlado, Message = ex.Message }
                };
            }

            return res;
        }
        public ResProductosPorVencer ObtenerProductosPorVencer_SP(
    int userId, int dias = 7, bool incluirVencidos = false, int maxDiasVencidos = 7, int page = 1, int pageSize = 50)
        {
            var res = new ResProductosPorVencer { productos = new List<ProductoPorVencer>(), listaDeErrores = new List<Error>() };

            try
            {
                if (userId <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." });
                    return res;
                }

                using (var linq = new linqDataContext())
                {
                    var rows = linq.SP_UserInventory_PorVencer(userId, dias, incluirVencidos, maxDiasVencidos, page, pageSize).ToList();

                    res.productos = rows.Select(r => new ProductoPorVencer
                    {
                        idProducto = r.ProductID,
                        nombre = r.Name,
                        idCategoria = r.CategoryID,
                        unidad = r.Unit,
                        cantidad = r.Quantity,
                        fechaExpiracion = r.ExpirationDate,
                        diasRestantes = r.DiasRestantes
                    }).ToList();

                    res.resultado = true;
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                if (res.listaDeErrores == null) res.listaDeErrores = new List<Error>();
                res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.ErrorNoControlado, Message = ex.Message });
                return res;
            }
        }
    }

}