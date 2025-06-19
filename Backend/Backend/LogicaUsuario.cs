// Corregido en base a tu estructura y convenciones actuales
using AcccesoDatos;
using Entidades.Entity;
using Entidades.Enum;
using Entidades.Request;
using Entidades.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Backend
{
    public class LogicaUsuario
    {
        public ResInsertarUsuario Insertar(ReqInsertarUsuario req)
        {
            var res = new ResInsertarUsuario { listaDeErrores = new List<Error>() };

            try
            {
                if (req == null)
                    return ErrorRes(res, EnumErrores.RequestNulo, "Solicitud nula");

                ValidarCamposUsuario(req.Usuario, res);

                if (res.listaDeErrores.Any())
                {
                    res.resultado = false;
                    return res;
                }

                string codigo = GenerarCodigoVerificacion();
                int? idBD = 0;
                int? errorId = 0;
                string errorBD = "";

                using (var linq = new linqDataContext())
                {
                    linq.SP_INGRESAR_USUARIO(
                        req.Usuario.nombre,
                        req.Usuario.correoElectronico,
                        req.Usuario.password,
                        codigo,
                        ref idBD,
                        ref errorId,
                        ref errorBD);
                }

                if (idBD == null || idBD == 0)
                {
                    return ErrorRes(res, EnumErrores.ErrorDeBaseDatos, errorBD);
                }

                if (!Utilitarios.EnviarCorreoValidacion(req.Usuario.correoElectronico, codigo))
                {
                    return ErrorRes(res, EnumErrores.NoSeEnvioElCorreo, "No se pudo enviar el correo de activación.");
                }

                res.resultado = true;
            }
            catch (Exception ex)
            {
                return ErrorRes(res, EnumErrores.ErrorNoControlado, ex.Message);
            }

            return res;
        }

        public ResLogin Login(ReqLogin req)
        {
            var res = new ResLogin();

            try
            {
                using (var linq = new linqDataContext())
                {
                    var loginResult = linq.sp_Login(req.correo, req.password).FirstOrDefault();

                    if (loginResult == null)
                        return ErrorRes(res, EnumErrores.CredencialesIncorrectas, "Credenciales incorrectas");

                    res.Usuario = FactoryUsuarioLogin(loginResult);

                    var sesion = new Sesion
                    {
                        usuario = res.Usuario,
                        token = Guid.NewGuid().ToString(),
                        origen = req.origen
                    };

                    var logSesion = new LogSesion();
                    res.resultado = logSesion.abrir(sesion);

                    if (!res.resultado)
                        return ErrorRes(res, enumCodigoError.ErrorDeBaseDatos, "No se pudo abrir sesión");
                }
            }
            catch (Exception ex)
            {
                return ErrorRes(res, enumCodigoError.ErrorNoControlado, ex.Message);
            }

            return res;
        }

        public ResObtenerUsuario Obtener(ReqObtenerUsuario req)
        {
            var res = new ResObtenerUsuario { listaDeErrores = new List<Error>() };

            try
            {
                if (req.idUsuario == 0)
                    return ErrorRes(res, enumCodigoError.UsuarioFaltante, "ID de usuario requerido");

                using (var linq = new linqDataContext())
                {
                    var usuario = linq.SP_OBTENER_USUARIO(req.idUsuario).FirstOrDefault();

                    if (usuario == null)
                        return ErrorRes(res, enumCodigoError.UsuarioNoEncontrado, "Usuario no encontrado");

                    res.usuario = FactoryUsuario(usuario);
                    res.resultado = true;
                }
            }
            catch (Exception ex)
            {
                return ErrorRes(res, enumCodigoError.ErrorNoControlado, ex.Message);
            }

            return res;
        }

        public ResActivarUsuario Activar(Re qActivarUsuario req)
        {
            var res = new ResActivarUsuario { listaDeErrores = new List<Error>() };

            try
            {
                int? idBD = 0;
                int? errorId = 0;
                string errorBD = "";
                int? filasAfectadas = 0;

                using (var linq = new linqDataContext())
                {
                    linq.SP_ACTIVAR_USUARIO(req.correo, req.codigo, ref idBD, ref errorId, ref errorBD, ref filasAfectadas);
                }

                if (errorId != 0)
                    return ErrorRes(res, enumCodigoError.UsuarioNoActivado, "Token expirado o inválido");

                res.resultado = true;
            }
            catch (Exception ex)
            {
                return ErrorRes(res, enumCodigoError.ErrorNoControlado, ex.Message);
            }

            return res;
        }

        private static void ValidarCamposUsuario(Usuario usuario, ResInsertarUsuario res)
        {
            if (string.IsNullOrEmpty(usuario.nombre))
                res.listaDeErrores.Add(new Error { ErrorCode = enumCodigoError.NombreNuloOVacio, Message = "Nombre es obligatorio" });

            if (string.IsNullOrEmpty(usuario.apellidos))
                res.listaDeErrores.Add(new Error { ErrorCode = enumCodigoError.ApellidoNuloOVacio, Message = "Apellido es obligatorio" });

            if (string.IsNullOrEmpty(usuario.correoElectronico))
            {
                res.listaDeErrores.Add(new Error { ErrorCode = enumCodigoError.CorreoNuloOVacio, Message = "Correo electrónico es obligatorio" });
            }
            else if (!EsCorreo(usuario.correoElectronico))
            {
                res.listaDeErrores.Add(new Error { ErrorCode = enumCodigoError.FormatoCorreoInvalido, Message = "Formato de correo inválido" });
            }

            if (string.IsNullOrEmpty(usuario.password))
            {
                res.listaDeErrores.Add(new Error { ErrorCode = enumCodigoError.PasswordNuloOVacio, Message = "Contraseña es obligatoria" });
            }
            else if (!EsPasswordFuerte(usuario.password))
            {
                res.listaDeErrores.Add(new Error { ErrorCode = enumCodigoError.PasswordDebil, Message = "Contraseña débil" });
            }
        }

        private static bool EsPasswordFuerte(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            return password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit) && password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));
        }

        private static bool EsCorreo(string correo)
        {
            try
            {
                var mailAddress = new MailAddress(correo);
                return mailAddress.Address == correo;
            }
            catch { return false; }
        }

        private static string GenerarCodigoVerificacion()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var codigo = new StringBuilder();
            for (int i = 0; i < 6; i++) codigo.Append(caracteres[random.Next(caracteres.Length)]);
            return codigo.ToString();
        }

        private static Usuario FactoryUsuarioLogin(sp_LoginResult tc)
        {
            return new Usuario
            {
                id = tc.ID_USUARIO,
                nombre = tc.NOMBRE,
                apellidos = tc.APELLIDOS
            };
        }

        private static Usuario FactoryUsuario(SP_OBTENER_USUARIOResult tc)
        {
            return new Usuario
            {
                id = tc.ID_USUARIO,
                nombre = tc.NOMBRE,
                apellidos = tc.APELLIDOS,
                correoElectronico = tc.CORREO_ELECTRONICO
            };
        }

        private static T ErrorRes<T>(T res, EnumErrores codigo, string mensaje) where T : ResBase, new()
        {
            res.resultado = false;
            res.listaDeErrores = new List<Error> { new Error { ErrorCode = codigo, Message = mensaje } };
            return res;
        }
    }
}
