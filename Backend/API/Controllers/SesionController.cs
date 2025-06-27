using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Backend;
using Entidades.Entity;
using Entidades.Request;
using Entidades.Response;

namespace API.Controllers
{
    public class SesionController : Controller
    {
        [HttpPost]
        [Route("api/sesion/abrir")]
        public ResBase AbrirSesion(Sesion sesion)
        {
            return new LogicaSesion().AbrirSesion(sesion);
        }

        [HttpPost]
        [Route("api/sesion/cerrar")]
        public ResBase CerrarSesion(ReqCerrarSesion req)
        {
            return new LogicaSesion().CerrarSesion(req.token);
        }
    }
}
