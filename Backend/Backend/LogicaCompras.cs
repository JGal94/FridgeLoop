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
using Entidades.Response.Entidades.Response;
using System.Data.SqlClient;





namespace Backend
{
    public class LogicaCompras
    {
        public ResRegistrarCompra RegistrarCompra(int userId, ReqRegistrarCompra req)
        {
            // ---------- Validaciones base ----------
            if (req == null)
                return Fail("La solicitud es nula.", EnumErrores.RequestNulo);

            if (req.items == null || req.items.Count == 0)
                return Fail("Debes enviar al menos un item.", EnumErrores.CampoRequeridoFaltante);

            foreach (var it in req.items)
            {
                if (string.IsNullOrWhiteSpace(it.nombre) || it.idCategoria <= 0 || string.IsNullOrWhiteSpace(it.unidad))
                    return Fail("Cada item requiere nombre, idCategoria y unidad.", EnumErrores.CampoRequeridoFaltante);

                if (it.cantidad <= 0)
                    return Fail("La cantidad debe ser > 0.", EnumErrores.FormatoDatoInvalido);

                if (it.precioUnitario.HasValue && it.precioUnitario.Value < 0)
                    return Fail("El precio no puede ser negativo.", EnumErrores.FormatoDatoInvalido);
            }

            // ---------- Serializar items NOMINALES tal cual los pide el SP ----------
            // (sin IDs: nombre, idCategoria, unidad, cantidad, precioUnitario, fechaExpiracion)
            string itemsJson = Newtonsoft.Json.JsonConvert.SerializeObject(req.items);

            try
            {
                using (var linq = new linqDataContext())
                {
                    int? purchaseId = 0;
                    int? spErr = 0;
                    string spMsg = "";

                    linq.SP_Compras_RegistrarDesdeItemsNominales(
                        userId,
                        req.fechaCompra.HasValue ? req.fechaCompra.Value : DateTime.UtcNow,
                        itemsJson,
                        ref purchaseId,
                        ref spErr,
                        ref spMsg
                    );

                    if (spErr.GetValueOrDefault() != 0)
                        return Fail(
                            string.IsNullOrWhiteSpace(spMsg) ? "No se pudo registrar la compra." : spMsg,
                            EnumErrores.CompraNoRegistrada
                        );

                    // Éxito
                    decimal total = req.items.Sum(i => (i.precioUnitario.HasValue ? i.precioUnitario.Value : 0m) * i.cantidad);

                    return new ResRegistrarCompra
                    {
                        resultado = true,
                        mensaje = string.IsNullOrWhiteSpace(spMsg) ? "Compra registrada correctamente." : spMsg,
                        idCompra = purchaseId.GetValueOrDefault(),
                        total = total,
                        listaDeErrores = new List<Error>()
                    };
                }
            }
            catch (Exception ex)
            {
                // Puedes loguear 'ex' aquí si tienes logger
                return Fail("Ocurrió un error no controlado al registrar la compra.", EnumErrores.ErrorNoControlado);
            }
        }

        // ---------- Helper local uniforme ----------
        private ResRegistrarCompra Fail(string msg, EnumErrores code)
        {
            return new ResRegistrarCompra
            {
                resultado = false,
                mensaje = msg,
                idCompra = 0,
                total = 0m,
                listaDeErrores = new List<Error>
            {
                new Error { ErrorCode = code, Message = msg }     // ✅

            }
            };
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
            var res = new ResObtenerCompra { listaDeErrores = new List<Error>() };

            try
            {
                if (userId <= 0)
                {
                    res.resultado = false;
                    res.mensaje = "Usuario no autenticado.";
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." });
                    return res;
                }
                if (req == null || req.idCompra <= 0)
                {
                    res.resultado = false;
                    res.mensaje = "idCompra es requerido.";
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.CampoRequeridoFaltante, Message = "Parámetros inválidos." });
                    return res;
                }

                // Usa la misma connectionString que tu DataContext
                string cs;
                using (var linq = new linqDataContext())
                    cs = linq.Connection.ConnectionString;

                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SP_Compras_ObtenerDetalle", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@PurchaseID", SqlDbType.Int) { Value = req.idCompra });

                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        // --- 1er resultset: header ---
                        if (!rd.Read())
                        {
                            res.resultado = false;
                            res.mensaje = "Compra no encontrada o no pertenece al usuario.";
                            return res;
                        }

                        var compra = new CompraDetalle
                        {
                            idCompra = rd.GetInt32(rd.GetOrdinal("PurchaseID")),
                            fechaCompra = rd.GetDateTime(rd.GetOrdinal("PurchaseDate")),
                            total = rd.IsDBNull(rd.GetOrdinal("TotalAmount")) ? 0m : rd.GetDecimal(rd.GetOrdinal("TotalAmount")),
                            notas = null,
                            items = new List<ItemCompra>()
                        };

                        // --- 2º resultset: items ---
                        if (rd.NextResult())
                        {
                            while (rd.Read())
                            {
                                var item = new ItemCompra
                                {
                                    idProducto = rd.GetInt32(rd.GetOrdinal("ProductID")),
                                    nombre = rd.IsDBNull(rd.GetOrdinal("ProductName")) ? null : rd.GetString(rd.GetOrdinal("ProductName")),
                                    idCategoria = rd.IsDBNull(rd.GetOrdinal("CategoryID")) ? 0 : rd.GetInt32(rd.GetOrdinal("CategoryID")),
                                    unidad = rd.IsDBNull(rd.GetOrdinal("Unit")) ? null : rd.GetString(rd.GetOrdinal("Unit")),
                                    cantidad = rd.IsDBNull(rd.GetOrdinal("Quantity")) ? 0m : rd.GetDecimal(rd.GetOrdinal("Quantity")),
                                    precioUnitario = rd.IsDBNull(rd.GetOrdinal("UnitPrice")) ? (decimal?)null : rd.GetDecimal(rd.GetOrdinal("UnitPrice"))
                                };
                                compra.items.Add(item);
                            }
                        }

                        res.compra = compra;
                        res.resultado = true;
                    }
                }

                return res;
            }
            catch (Exception ex)
            {
                res.resultado = false;
                if (res.listaDeErrores == null) res.listaDeErrores = new List<Error>();
                res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.ErrorNoControlado, Message = ex.Message });
                res.mensaje = "Error al obtener el detalle de compra.";
                return res;
            }
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
