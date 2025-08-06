using AccesoDatos;
using Entidades.Entity;
using Entidades.Enum;
using Entidades.Request;
using Entidades.Response;
using Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Logica
{
    public class LogicaReceta
    {
        public async Task<ResRecetasIA> ObtenerRecetasIA(ReqRecetasIA req)
        {
            var res = new ResRecetasIA();
            res.listaDeErrores = new List<Error>();

            try
            {
                if (req == null || req.idUsuario <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.RequestNulo,
                        Message = "El request o el ID de usuario no pueden ser nulos."
                    });
                    return res;
                }

                List<Productos> productosUsuario = new List<Productos>();

                using (var linq = new linqDataContext())
                {
                    productosUsuario = linq.GetProductosInventarioUsuario(req.idUsuario)
                                           .Select(p => new Productos
                                           {
                                               nombre = p.nombre,
                                               idCategoria = p.idCategoria,
                                               unidad = p.unidad,
                                               userID = p.userID,
                                               quantity = p.quantity,
                                               expirationDate = p.expirationDate
                                           }).ToList();
                }

                if (productosUsuario == null || !productosUsuario.Any())
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.InventarioVacio,
                        Message = "El inventario del usuario está vacío."
                    });
                    return res;
                }

                var DeepSeekApiClient = new DeepSeekApiClient();

                var recetas = await DeepSeekApiClient.ObtenerRecetasDesdeIngredientes(productosUsuario);

                if (recetas == null)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.IARecomendacionFallida,
                        Message = "No se pudieron generar recetas con los ingredientes proporcionados."
                    });
                    return res;
                }

                res.recetas = recetas;
                res.resultado = true;
                return res;
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = $"Ocurrió un error inesperado: {ex.Message}"
                });
                return res;
            }
        }
    }
}
