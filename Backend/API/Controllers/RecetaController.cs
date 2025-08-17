using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Security.Claims;   
using System.Threading;

using Backend.Logica;         
using Entidades.Enum;         
using Entidades.Request;      
using Entidades.Response;     
using Entidades.Entity;       

namespace API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/receta")]
    public class RecetaController : ApiController
    {
        private LogicaReceta CreateLogic() => new LogicaReceta();

        private int GetUserId()
        {
            var cp = User as ClaimsPrincipal;
            var idStr = cp?.FindFirst("id")?.Value;
            int uid;
            return int.TryParse(idStr, out uid) ? uid : 0;
        }

        // POST api/receta/ia
        // Usa el inventario del usuario autenticado para pedir recetas a la IA
        [HttpPost]
        [Route("ia")]
        public async System.Threading.Tasks.Task<ResRecetasIA> ObtenerRecetasIA([FromBody] ReqRecetasIA req)
        {
            var userId = GetUserId();
            if (userId <= 0)
            {
                return new ResRecetasIA
                {
                    resultado = false,
                    listaDeErrores = new List<Error>
                    {
                        new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." }
                    }
                };
            }

            if (req == null) req = new ReqRecetasIA();
            // Seguridad: forzamos el idUsuario desde el token
            req.idUsuario = userId;

            // Llama a la lógica (async)
            return await CreateLogic().ObtenerRecetasIA(req);
        }

        // POST api/receta/preparar
        // Registra una receta (y actualiza inventario según tus SPs)
        [HttpPost]
        [Route("preparar")]
        public ResInsertarReceta PrepararReceta([FromBody] ReqInsertarReceta req)
        {
            var userId = GetUserId();
            if (userId <= 0)
                return new ResInsertarReceta { Exito = false, Mensaje = "Usuario no autenticado." };

            if (req == null)
                return new ResInsertarReceta { Exito = false, Mensaje = "El request no puede ser nulo." };

            // Seguridad: forzamos el UserID desde el token
            req.UserID = userId;

            // ModelState opcional: si usas DataAnnotations en tus DTO
            if (!ModelState.IsValid)
                return new ResInsertarReceta { Exito = false, Mensaje = "Datos inválidos en la solicitud." };

            return CreateLogic().PrepararReceta(req);
        }
    }
}
