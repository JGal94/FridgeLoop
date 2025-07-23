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
        public ResInsertarProducto InsertarProducto(ReqInsertarProducto req)
        {
            var res = new ResInsertarProducto();

            try
            {
                // Validación - ahora accedemos a través de req.productos
                if (req.productos == null ||
                    string.IsNullOrWhiteSpace(req.productos.nombre) ||
                    string.IsNullOrWhiteSpace(req.productos.unidad) ||
                    req.productos.idCategoria <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores = new List<Error>
                    {
                        new Error
                        {
                            ErrorCode = EnumErrores.CampoRequeridoFaltante,
                            Message = "Todos los campos del producto son obligatorios."
                        }
                    };
                    return res;
                }

                using (var linq = new linqDataContext())
                {
                    // Variables para los parámetros de salida del stored procedure
                    int? productID = null;
                    int? errorId = null;
                    string errorMensaje = null;

                    // Llamada al stored procedure con TODOS los parámetros requeridos
                    linq.InsertProduct(
                        req.productos.nombre,                    // name (string)
                        req.productos.idCategoria,               // categoryID (int?)
                        req.productos.unidad,                    // unit (string)
                        req.productos.userID ?? 1,               // userID (int?) - valor por defecto si es null
                        req.productos.quantity,                  // quantity (decimal?)
                        req.productos.expirationDate,            // expirationDate (DateTime?)
                        ref productID,                           // productID (ref int?)
                        ref errorId,                             // errorId (ref int?)
                        ref errorMensaje                         // errorMensaje (ref string)
                    );

                    // Verificar si hubo error en el stored procedure
                    if (errorId.HasValue && errorId.Value > 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores = new List<Error>
                        {
                            new Error
                            {
                                ErrorCode = EnumErrores.ErrorDeBaseDatos,
                                Message = errorMensaje ?? "Error al insertar el producto."
                            }
                        };
                        return res;
                    }

                    // Verificar que se obtuvo un ID válido
                    if (!productID.HasValue || productID.Value <= 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores = new List<Error>
                        {
                            new Error
                            {
                                ErrorCode = EnumErrores.ErrorDeBaseDatos,
                                Message = "No se pudo insertar el producto."
                            }
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
                res.listaDeErrores = new List<Error>
                {
                    new Error
                    {
                        ErrorCode = EnumErrores.ErrorNoControlado,
                        Message = ex.Message
                    }
                };
            }

            return res;
        }

        public ResObtenerProductos ObtenerProductos()
        {
            // Creamos la respuesta que vamos a devolver
            ResObtenerProductos res = new ResObtenerProductos
            {
                productos = new List<Productos>()
            };

            try
            {
                using (var linq = new linqDataContext())
                {
                    // Ejecuta el SP que obtiene todos los productos
                    var productos = linq.GetProducts().ToList();

                    // Recorremos los productos devueltos por la base de datos
                    foreach (var p in productos)
                    {
                        // Agregamos cada producto a la lista usando la entidad personalizada
                        res.productos.Add(new Productos
                        {
                            nombre = p.Name,                    // Cambiado de Nombre a nombre para consistencia
                            idCategoria = p.CategoryID ?? 0,    // Cambiado de Categoria string a idCategoria int
                            unidad = p.Unit                     // Cambiado de Unidad a unidad para consistencia
                        });
                    }

                    // Indicamos que todo salió bien
                    res.resultado = true;
                }
            }
            catch (Exception ex)
            {
                // Si ocurre un error, devolvemos una respuesta con error controlado
                res.resultado = false;
                res.listaDeErrores = new List<Error>
                {
                    new Error
                    {
                        ErrorCode = EnumErrores.ErrorNoControlado,
                        Message = ex.Message
                    }
                };
            }

            // Retornamos la respuesta con los productos o el error
            return res;
        }

        public ResActualizarProducto ActualizarProducto(ReqActualizarProducto req)
        {
            var res = new ResActualizarProducto();

            try
            {
                // Valida que los datos sean correctos
                if (req.idProducto <= 0 || string.IsNullOrWhiteSpace(req.nombre) ||
                    string.IsNullOrWhiteSpace(req.unidad) || req.idCategoria <= 0)
                {
                    // Si faltan datos devuelve error
                    res.resultado = false;
                    res.listaDeErrores = new List<Error>
                    {
                        new Error
                        {
                            ErrorCode = EnumErrores.CampoRequeridoFaltante,
                            Message = "Todos los campos del producto son obligatorios."
                        }
                    };
                    return res;
                }

                // Ejecuta linq
                using (var linq = new linqDataContext())
                {
                    // Llama al stored procedure SIN parámetros de salida (según el error)
                    linq.UpdateProduct(
                        req.idProducto,   // ProductID
                        req.nombre,       // Name  
                        req.idCategoria,  // CategoryID
                        req.unidad        // Unit
                    );

                    // Si no lanza excepción, operación exitosa
                    res.resultado = true;
                    res.mensaje = "Producto actualizado correctamente.";
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores = new List<Error>
                {
                    new Error
                    {
                        ErrorCode = EnumErrores.ErrorNoControlado,
                        Message = ex.Message
                    }
                };
            }

            return res;
        }
    }
}