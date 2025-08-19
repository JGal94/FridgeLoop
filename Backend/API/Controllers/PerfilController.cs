using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;



using Backend;
using Entidades.Entity;       // Error
using Entidades.Enum;         // EnumErrores
using Entidades.Request;      // ReqCambiarPassword, ReqActualizarNombre
using Entidades.Response;     
namespace API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/perfil")]
    public class PerfilController : ApiController
    {
        private LogicaPerfil CreateLogic() => new LogicaPerfil();

        private int GetUserId()
        {
            var cp = User as ClaimsPrincipal;
            var idStr = cp?.FindFirst("id")?.Value
                     ?? cp?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? cp?.FindFirst("sub")?.Value;
            int uid;
            return int.TryParse(idStr, out uid) ? uid : 0;
        }

        // PUT api/perfil/password
        [HttpPut]
        [Route("password")]
        public ResCambiarPassword CambiarPassword([FromBody] ReqCambiarPassword req)
        {
            return CreateLogic().CambiarPassword(GetUserId(), req);
        }

        // PUT api/perfil/nombre
        [HttpPut]
        [Route("nombre")]
        public ResActualizarNombre ActualizarNombre([FromBody] ReqActualizarNombre req)
        {
            return CreateLogic().ActualizarNombre(GetUserId(), req);
        }

    }
}
