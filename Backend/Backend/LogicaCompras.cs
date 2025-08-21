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
