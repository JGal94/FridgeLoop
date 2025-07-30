using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Backend;
using Entidades.Request;
using Entidades.Response;

namespace API.Controllers
{
    public class ProductoController : Controller
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