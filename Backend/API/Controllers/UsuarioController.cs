using System.Web.Http;
using Entidades.Request;
using Entidades.Response;
using Backend;
using System.Configuration;
using Entidades.Entity;
using System;
using Entidades.Enum;
using System.Collections.Generic;

namespace API.Controllers
{
    [RoutePrefix("api/usuario")]
    public class UsuarioController : ApiController
    {
        // Lee la config desde web.config y la entrega al backend
        private JwtSettings GetJwtSettings() => new JwtSettings
        {
            Issuer = ConfigurationManager.AppSettings["Jwt:Issuer"],
            Audience = ConfigurationManager.AppSettings["Jwt:Audience"],
            Secret = ConfigurationManager.AppSettings["Jwt:Secret"],
            HoursToExpire = int.Parse(ConfigurationManager.AppSettings["Jwt:HoursToExpire"] ?? "4")
        };

        // Crea la lógica ya con la config inyectada
        private LogicaUsuario CreateLogic() => new LogicaUsuario(GetJwtSettings());


        // PUBLIC: insertar
        [HttpPost]
        [Route("insertar")]
        public ResInsertarUsuario Insertar(ReqInsertarUsuario req)
        {
            return CreateLogic().InsertarUsuario(req);
        }

        // PUBLIC: activar
        [HttpPost]
        [Route("activar")]
        public ResActivarUsuario Activar(ReqActivarUsuario req)
        {
            return CreateLogic().ActivarUsuario(req);
        }

        // PUBLIC: login (genera JWT y abre sesión en BD)
        [HttpPost]
        [Route("login")]
        public ResLogin Login(ReqLogin req)
        {
            return CreateLogic().Login(req);
        }

        // PROTEGIDO: obtener datos de usuario
        [HttpPost]
        [Route("obtener")]
        [JwtAuthorize]
        public ResObtenerUsuario Obtener(ReqObtenerUsuario req)
        {
            return CreateLogic().ObtenerUsuario(req);
        }

        // PROTEGIDO: cerrar sesión
        [HttpPost]
        [Route("cerrarsesion")]
        [JwtAuthorize]
        public ResCerrarSesion CerrarSesion()
        {
            // 1) Lee el Bearer del header de forma segura
            var auth = Request?.Headers?.Authorization;
            var hasBearer = auth != null
                && auth.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(auth.Parameter);

            if (!hasBearer)
            {
                return new ResCerrarSesion
                {
                    resultado = false,
                    listaDeErrores = new List<Error>
            {
                new Error {
                    ErrorCode = EnumErrores.TokenInvalido,
                    Message = "Falta el token Bearer."
                }
            }
                };
            }

            // 2) Construye el request para la lógica (sin depender del body)
            var req = new ReqCerrarSesion { token = auth.Parameter };

            // 3) Ejecuta la lógica
            return CreateLogic().CerrarSesion(req);
        }

    }
}
