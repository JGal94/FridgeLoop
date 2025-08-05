using System.Web.Http; // ← no System.Web.Mvc
using Backend;
using Entidades.Request;
using Entidades.Response;

namespace API.Controllers
{
    [JwtAuthorize]
    public class ProductoController : ApiController // ✅ API Controller
    {
        [HttpPost]
        [Route("api/producto/insertar")]
        public ResInsertarProducto InsertarProducto(ReqInsertarProducto req)
        {
            return new LogicaProducto().InsertarProducto(req);
        }

        [HttpGet]
        [Route("api/producto/obtener")]
        public ResObtenerProductos ObtenerProductos()
        {
            return new LogicaProducto().ObtenerProductos();
        }

        [HttpPost]
        [Route("api/producto/actualizar")]
        public ResActualizarProducto ActualizarProducto(ReqActualizarProducto req)
        {
            return new LogicaProducto().ActualizarProducto(req);
        }
    }
}
