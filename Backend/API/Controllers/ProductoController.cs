using System.Web.Http; 
using Backend;
using Entidades.Request;
using Entidades.Response;
using System.Security.Claims;
using Entidades.Enum;
using Entidades.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers   
{
    [JwtAuthorize]
    [RoutePrefix("api/producto")]
    public class ProductoController : ApiController
    {
        private LogicaProducto CreateLogic() => new LogicaProducto();

        private int GetUserId()
        {
            var identity = User as ClaimsPrincipal;
            var idStr = identity?.FindFirst("id")?.Value;
            return int.TryParse(idStr, out var uid) ? uid : 0;
        }

        [HttpPost]
        [Route("insertar")]
        public ResInsertarProducto InsertarProducto([FromBody] ReqInsertarProducto req)
        {
            if (req == null)
            {
                return new ResInsertarProducto
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.RequestNulo, Message = "El request no puede ser nulo." }
                    }
                };
            }
            if (!ModelState.IsValid)
            {
                return new ResInsertarProducto
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.FormatoDatoInvalido, Message = "Datos inválidos en la solicitud." }
                    }
                };
            }

            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResInsertarProducto
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    }
                };
            }

            // Forzamos pertenencia por token
            return CreateLogic().InsertarProducto(userId, req);
        }

        // API/Controllers/ProductoController.cs
        [HttpGet]
        [Route("obtener")]
        public ResObtenerProductos ObtenerProductos([FromUri] ReqObtenerInventario req)
        {
            var userId = GetUserId();
            if (userId <= 0) { /* ... mismo manejo actual ... */ }

            return CreateLogic().ObtenerProductos(userId, req?.page ?? 1, req?.pageSize ?? 20, req?.q);
        }


        [HttpPut]
        [Route("actualizar")]
        public ResActualizarProducto ActualizarProducto([FromBody] ReqActualizarProducto req)
        {
            if (req == null)
            {
                return new ResActualizarProducto
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.RequestNulo, Message = "El request no puede ser nulo." }
                    }
                };
            }
            if (!ModelState.IsValid)
            {
                return new ResActualizarProducto
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.FormatoDatoInvalido, Message = "Datos inválidos en la solicitud." }
                    }
                };
            }

            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResActualizarProducto
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    }
                };
            }

            return CreateLogic().ActualizarProducto(userId, req);
        }
        [HttpPut]
        [Route("inventario")]
        public ResActualizarProducto ActualizarInventario([FromBody] ReqActualizarInventario req)
        {
            if (req == null)
            {
                return new ResActualizarProducto
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                new Error { ErrorCode = EnumErrores.RequestNulo, Message = "El request no puede ser nulo." }
            }
                };
            }

            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResActualizarProducto
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
            }
                };
            }

            return CreateLogic().ActualizarInventario(userId, req);
        }

        // GET api/producto/porvencer?dias=7&incluirVencidos=false&page=1&pageSize=50
        [HttpGet]
        [Route("porvencer")]
        
        public ResProductosPorVencer ProductosPorVencer([FromUri] int dias = 7, [FromUri] bool incluirVencidos = false,
                                                        [FromUri] int maxDiasVencidos = 7, [FromUri] int page = 1, [FromUri] int pageSize = 50)
        {
            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResProductosPorVencer
                {
                    resultado = false,
                    listaDeErrores = new List<Error>   
    {
        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
    },
                    productos = new List<ProductoPorVencer>()
                };
            }

            return CreateLogic().ObtenerProductosPorVencer_SP(userId, dias, incluirVencidos, maxDiasVencidos, page, pageSize);
        }
        // ... dentro de la clase ProductoController (ya tiene [JwtAuthorize] y RoutePrefix("api/producto"))
        [HttpGet]
        [Route("obtenerporcodigo")]
        public async Task<ResObtenerProductoPorCodigo> ObtenerProductoPorCodigo([FromUri] string codigoBarras)
        {
            var res = new ResObtenerProductoPorCodigo
            {
                resultado = false,
                listaDeErrores = new List<Error>()
            };

            // Si tu controller está con [JwtAuthorize], mantenemos la validación
            var userId = GetUserId();
            if (userId <= 0)
            {
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.TokenInvalido,
                    Message = "Usuario no autenticado."
                });
                return res;
            }

            if (string.IsNullOrWhiteSpace(codigoBarras))
            {
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.CampoRequeridoFaltante,
                    Message = "El código de barras es obligatorio."
                });
                return res;
            }

            // Llama a tu lógica que consulta el API externo (sin insertar)
            // Debe devolver Entidades.Entity.Productos o null si no existe
            var producto = await CreateLogic().ObtenerProductoDeApi(codigoBarras);

            if (producto == null)
            {
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ProductoNoEncontrado,
                    Message = "Producto no encontrado en la fuente externa."
                });
                return res;
            }

            res.Producto = producto;   // <- respeta tu clase ResObtenerProductoPorCodigo
            res.resultado = true;
            return res;
        }

    }
}