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

                var fechaUtc = (req.fechaCompra ?? DateTime.UtcNow);
                decimal total = 0m;
                foreach (var it in req.items)
                {
                    if (it.idProducto <= 0 || it.cantidad <= 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.CampoRequeridoFaltante, Message = "idProducto y cantidad son obligatorios." });
                        return res;
                    }
                    if (it.precioUnitario.HasValue && it.precioUnitario.Value < 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.FormatoDatoInvalido, Message = "precioUnitario no puede ser negativo." });
                        return res;
                    }
                    total += (it.precioUnitario ?? 0m) * it.cantidad;
                }

                using (var scope = new Tx.TransactionScope(
                    Tx.TransactionScopeOption.Required,
                    new Tx.TransactionOptions { IsolationLevel = Tx.IsolationLevel.ReadCommitted },
                    Tx.TransactionScopeAsyncFlowOption.Enabled))
                using (var linq = new linqDataContext())
                {
                    
                    var snapshot = linq.GetUserInventory().Where(x => x.UserID == userId).ToList();

                    foreach (var it in req.items)
                    {
                        var expUtc = it.fechaExpiracion?.ToUniversalTime();
                        var existente = snapshot.FirstOrDefault(x => x.ProductID == it.idProducto);

                        if (existente == null)
                        {
                            linq.InsertUserInventory(userId, it.idProducto, it.cantidad, expUtc);
                            snapshot.Add(new GetUserInventoryResult
                            {
                                InventoryID = 0,
                                UserID = userId,
                                ProductID = it.idProducto,
                                Quantity = it.cantidad,
                                ExpirationDate = expUtc
                            });
                        }
                        else
                        {
                            var newQty = (existente.Quantity ?? 0m) + it.cantidad;
                            var newExp = existente.ExpirationDate;
                            if (expUtc.HasValue)
                            {
                                if (!newExp.HasValue || expUtc.Value < newExp.Value)
                                    newExp = expUtc; // conservar la más próxima
                            }

                            linq.UpdateUserInventory(existente.InventoryID, newQty, newExp);
                            existente.Quantity = newQty;
                            existente.ExpirationDate = newExp;
                        }

                        // (Opcional futuro) detalle de compra:
                        // linq.InsertPurchaseItem(res.idCompra, it.idProducto, it.cantidad, it.precioUnitario, expUtc);
                    }

                    res.total = total;
                    res.resultado = true;
                    res.mensaje = "Compra registrada y inventario actualizado.";
                    scope.Complete();
                }

                return res;
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
            return new ResObtenerCompras
            {
                resultado = false,
                mensaje = "Listado de compras pendiente de persistencia en BD.",
                listaDeErrores = new List<Error> { new Error { ErrorCode = EnumErrores.ErrorDeBaseDatos, Message = "No existe tabla de compras/detalle." } },
                compras = new System.Collections.Generic.List<CompraResumen>(),
                totalFiltrado = 0
            };
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
