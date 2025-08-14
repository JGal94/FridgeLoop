using System.Web.Http; // ← no System.Web.Mvc
using Backend;
using Entidades.Request;
using Entidades.Response;
using System.Security.Claims;
using Entidades.Enum;
using Entidades.Entity;
using System.Collections.Generic;

namespace API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/compra")]
    public class CompraController : ApiController
    {
        private LogicaCompras CreateLogic() => new LogicaCompras();

        private int GetUserId()
        {
            var identity = User as ClaimsPrincipal;
            var idStr = identity?.FindFirst("id")?.Value;
            int uid;
            return int.TryParse(idStr, out uid) ? uid : 0;
        }

        // POST api/compra/registrar
        [HttpPost]
        [Route("registrar")]
        public ResRegistrarCompra RegistrarCompra([FromBody] ReqRegistrarCompra req)
        {
            if (req == null)
            {
                return new ResRegistrarCompra
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.RequestNulo, Message = "El request no puede ser nulo." }
                    }
                };
            }

            // Nota: si usas DataAnnotations en el futuro, esto ayuda a capturar errores del binder
            if (!ModelState.IsValid)
            {
                return new ResRegistrarCompra
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
                return new ResRegistrarCompra
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    }
                };
            }

            return CreateLogic().RegistrarCompra(userId, req);
        }

        // GET api/compra/obtener?page=1&pageSize=20&desde=...&hasta=...
        [HttpGet]
        [Route("obtener")]
        public ResObtenerCompras ObtenerCompras([FromUri] ReqObtenerCompras req)
        {
            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResObtenerCompras
                {
                    resultado = false,
                    compras = new List<CompraResumen>(),
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    }
                };
            }

            // Garantiza defaults si no vino el objeto por querystring
            if (req == null) req = new ReqObtenerCompras();

            return CreateLogic().ObtenerCompras(userId, req);
        }

        // GET api/compra/{id}
        [HttpGet]
        [Route("{id:int}")]
        public ResObtenerCompra ObtenerCompra(int id)
        {
            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResObtenerCompra
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    }
                };
            }

            var req = new ReqObtenerCompra { idCompra = id };
            return CreateLogic().ObtenerCompra(userId, req);
        }

        // DELETE api/compra/{id}?revertirInventario=true|false
        [HttpDelete]
        [Route("{id:int}")]
        public ResEliminarCompra EliminarCompra(int id, [FromUri] bool revertirInventario = false)
        {
            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResEliminarCompra
                {
                    resultado = false,
                    listaDeErrores = new List<Error> {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    }
                };
            }

            var req = new ReqEliminarCompra
            {
                idCompra = id,
                revertirInventario = revertirInventario
            };

            return CreateLogic().EliminarCompra(userId, req);
        }
    }
}