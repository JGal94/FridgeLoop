using AccesoDatos;
using Entidades.Entity;
using Entidades.Response;
using Entidades.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class LogicaSesion
    {
        public ResBase AbrirSesion(Sesion sesion)
        {
            var res = new ResBase();
            res.listaDeErrores = new List<Error>();

            try
            {
                if (sesion == null || sesion.usuario == null)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.RequestNulo,
                        Message = "La sesión o el usuario no pueden ser nulos."
                    });
                    return res;
                }

                using (var linq = new linqDataContext())
                {
                    linq.CreateUserSession(
                        sesion.usuario.id,
                        sesion.token,
                        sesion.fechaExpiracion,
                        sesion.origen,
                        sesion.direccionIP
                    );
                }

                res.resultado = true;
                return res;
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
                return res;
            }
        }

        public ResBase CerrarSesion(string token)
        {
            var res = new ResBase();
            res.listaDeErrores = new List<Error>();

            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.TokenInvalido,
                        Message = "El token de sesión es inválido o nulo."
                    });
                    return res;
                }

                int? errorId = 0;
                string errorMensaje = "";

                using (var linq = new linqDataContext())
                {
                    linq.CloseUserSession(token, ref errorId, ref errorMensaje);
                }

                if (errorId != 0)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = (EnumErrores)errorId,
                        Message = errorMensaje
                    });
                    return res;
                }

                res.resultado = true;
                return res;
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
                return res;
            }
        }

    }
}
