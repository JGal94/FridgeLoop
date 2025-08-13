using System.Web.Http; // ← no System.Web.Mvc
using Backend;
using Entidades.Request;
using Entidades.Response;
using System.Security.Claims;
using Entidades.Enum;
using Entidades.Entity;
using System.Collections.Generic;

namespace API.Controllers   // <-- ajusta si tu namespace difiere
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

        [HttpGet]
        [Route("obtener")]
        public ResObtenerProductos ObtenerProductos([FromUri] int page = 1, [FromUri] int pageSize = 20, [FromUri] string q = null)
        {
            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResObtenerProductos
                {
                    resultado = false,
                    productos = new List<Productos>(),
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    }
                };
            }

            return CreateLogic().ObtenerProductos(userId, page, pageSize, q);
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
    }
}