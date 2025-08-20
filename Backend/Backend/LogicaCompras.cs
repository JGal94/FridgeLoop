using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccesoDatos;
using Entidades.Entity;
using Entidades.Enum;
using Entidades.Response;
using Entidades.Request;

using Tx = System.Transactions;
using Newtonsoft.Json;





namespace Backend
{
    public class LogicaCompras
    {
        // Registra compra + actualiza inventario (upsert) por cada item
        public ResRegistrarCompra RegistrarCompra(int userId, ReqRegistrarCompra req)
        {
            var res = new ResRegistrarCompra { listaDeErrores = new List<Error>() };

            try
            {
                if (userId <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." });
                    return res;
                }

                if (req == null || req.items == null || req.items.Count == 0)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.CampoRequeridoFaltante, Message = "La compra debe tener al menos un item." });
                    return res;
                }

                // Validaciones rápidas de cada item
                foreach (var it in req.items)
                {
                    if (it.idProducto <= 0 || it.cantidad <= 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.CampoRequeridoFaltante, Message = "idProducto y cantidad son obligatorios y deben ser > 0." });
                        return res;
                    }
                    if (it.precioUnitario.HasValue && it.precioUnitario.Value < 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.FormatoDatoInvalido, Message = "precioUnitario no puede ser negativo." });
                        return res;
                    }
                }

                // JSON que espera el SP: [{ProductID, Quantity, UnitPrice?, ExpirationDate?}]
                var itemsJson = JsonConvert.SerializeObject(
                    req.items.Select(i => new
                    {
                        ProductID = i.idProducto,
                        Quantity = i.cantidad,
                        UnitPrice = i.precioUnitario,                          // null si no viene
                        ExpirationDate = i.fechaExpiracion?.Date               // el SP lo lee como DATE
                    })
                );

                using (var linq = new linqDataContext())
                {
                    int? purchaseId = 0;
                    int? errorId = 0;
                    string errorMsg = "";

                    // El SP inserta Purchases + PurchaseDetails y actualiza inventario en una transacción
                    linq.SP_Compras_Registrar(
                        userId,
                        req.fechaCompra ?? DateTime.UtcNow,                    // el SP usa GETUTCDATE() si viene NULL
                        itemsJson,
                        ref purchaseId,
                        ref errorId,
                        ref errorMsg
                    ); // 

                    if ((errorId ?? 0) != 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = (EnumErrores)(errorId ?? 0), Message = errorMsg });
                        return res;
                    }

                    // Total calculado igual que en el SP (suma de UnitPrice*Quantity)
                    res.total = req.items.Sum(i => (i.precioUnitario ?? 0m) * i.cantidad);
                    res.idCompra = purchaseId ?? 0;
                    res.resultado = true;
                    res.mensaje = "Compra registrada correctamente.";
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                if (res.listaDeErrores == null)
                    res.listaDeErrores = new List<Error>();

                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });

                return res;
            }

        }

        // Placeholders: implementa cuando tengas tablas de compras en BD
        public ResObtenerCompras ObtenerCompras(int userId, ReqObtenerCompras req)
        {
            var res = new ResObtenerCompras
            {
                resultado = false,
                mensaje = null,
                listaDeErrores = new List<Error>(),
                compras = new List<CompraResumen>(),
                totalFiltrado = 0
            };

            try
            {
                // 1) Validaciones básicas
                if (userId <= 0)
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.TokenInvalido,
                        Message = "Usuario no autenticado."
                    });
                    return res;
                }

                // 2) Saneo de paginación
                var page = (req?.page ?? 1);
                var pageSize = (req?.pageSize ?? 20);
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                // 3) Filtros de fecha (opcionales)
                var desde = req?.desde;
                var hasta = req?.hasta;

                using (var linq = new linqDataContext())
                {
                    // SP: devuelve PurchaseID, PurchaseDate, TotalAmount, Items (SUM de Quantity)
                    //    y aplica filtros por usuario, rango de fechas y paginación
                    var rows = linq
                        .SP_Compras_ObtenerPorUsuario(userId, page, pageSize, desde, hasta)
                        .ToList(); // ← LINQ to SQL mapea el resultset

                    // 4) Mapeo a tu DTO actual (items:int). OJO: Items es decimal? en el SP, por eso hago cast.
                    res.compras = rows.Select(r => new CompraResumen
                    {
                        idCompra = r.PurchaseID,
                        fechaCompra = r.PurchaseDate ?? DateTime.UtcNow,
                        total = r.TotalAmount ?? 0m,
                        items = (int)r.Items,   // 👈 sin ?? porque es decimal, casteo a int
                        notas = null
                    }).ToList();


                    // 5) Total mostrado (del tamaño de la página recuperada)
                    //    Si quieres el total global, agrega un segundo resultset al SP que devuelva COUNT(*)
                    res.totalFiltrado = res.compras.Count;
                    res.resultado = true;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.mensaje = "Error al obtener las compras.";
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
            }

            return res;
        }

        public ResObtenerCompra ObtenerCompra(int userId, ReqObtenerCompra req)
        {
            return new ResObtenerCompra
            {
                resultado = false,
                mensaje = "Detalle de compra pendiente de persistencia en BD.",
                listaDeErrores = new List<Error> { new Error { ErrorCode = EnumErrores.ErrorDeBaseDatos, Message = "No existe tabla de compras/detalle." } },
                compra = null
            };
        }

        public ResEliminarCompra EliminarCompra(int userId, ReqEliminarCompra req)
        {
            return new ResEliminarCompra
            {
                resultado = false,
                mensaje = "Eliminar compra pendiente de persistencia en BD.",
                listaDeErrores = new List<Error> { new Error { ErrorCode = EnumErrores.ErrorDeBaseDatos, Message = "No existe tabla de compras/detalle." } }
            };
        }
    }
}
