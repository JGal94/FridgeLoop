using System.Web.Http;
using Entidades.Request;
using Entidades.Response;
using Backend;

namespace API.Controllers
{
    public class UsuarioController : ApiController
    {
        [HttpPost]
        [Route ("api/usuario/insertar")]
        public ResInsertarUsuario insertar(ReqInsertarUsuario req)
        {
            return new LogicaUsuario().InsertarUsuario(req);
        }

        [HttpPost]
        [Route("api/usuario/obtener")]
        public ResObtenerUsuario obtener(ReqObtenerUsuario req) { 
            return new LogicaUsuario().ObtenerUsuario(req);
        }

        [HttpPost]
        [Route("api/usuario/login")]
        public ResLogin login(ReqLogin req)
        {
            return new LogicaUsuario().Login(req);
        }

        [HttpPost]
        [Route("api/usuario/activar")]
        public ResActivarUsuario activar(ReqActivarUsuario req)
        {
            return new LogicaUsuario().ActivarUsuario(req);
        }
    }
}
