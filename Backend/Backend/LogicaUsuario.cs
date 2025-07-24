using AccesoDatos;
using Entidades.Entity;
using Entidades.Enum;
using Entidades.Request;
using Entidades.Response;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;









namespace Backend
{
    public class LogicaUsuario
    {
        public ResInsertarUsuario InsertarUsuario(ReqInsertarUsuario req)
        {
            var res = new ResInsertarUsuario();
            res.listaDeErrores = new List<Error>();

            try
            {
                if (req == null || req.usuario == null)
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.RequestNulo,
                        Message = "El request o el usuario no puede ser nulo."
                    });
                    res.resultado = false;
                    return res;
                }

                ValidarUsuario(req.usuario, res.listaDeErrores);

                if (res.listaDeErrores.Any())
                {
                    res.resultado = false;
                    return res;
                }

                string codigoVerificacion = GenerarCodigoVerificacion();

                // Primero intentamos enviar el correo
                if (!Utilitarios.EnviarCorreoValidacion(req.usuario.correoElectronico, codigoVerificacion))
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.NoSeEnvioElCorreo,
                        Message = "No se pudo enviar el correo de verificación."
                    });
                    res.resultado = false;
                    return res;
                }

                // Si el correo se envió correctamente, insertamos al usuario
                int? idUsuario = 0;
                int? errorId = 0;
                string errorMensaje = "";

                using (var linq = new linqDataContext())
                {
                    linq.InsertUser(
                        req.usuario.nombre,
                        req.usuario.correoElectronico,
                        req.usuario.password,
                        codigoVerificacion,
                        ref idUsuario,
                        ref errorId,
                        ref errorMensaje
                    );
                }

                // Verificar si hubo error desde el SP
                if (errorId != 0)
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = (EnumErrores)errorId,
                        Message = errorMensaje
                    });
                    res.resultado = false;
                    return res;
                }

                // Guardar ID si todo fue exitoso
                req.usuario.id = idUsuario ?? 0;

                res.resultado = true;
                return res;
            }
            catch (Exception ex)
            {
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
                res.resultado = false;
                return res;
            }
        }



        public ResLogin Login(ReqLogin req)
        {
            var res = new ResLogin();
            res.listaDeErrores = new List<Error>();

            try
            {
                if (string.IsNullOrEmpty(req.correo) || string.IsNullOrEmpty(req.password))
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.CredencialesIncorrectas,
                        Message = "Correo o contraseña vacíos."
                    });
                    res.resultado = false;
                    return res;
                }

                var loginResult = new LoginResult();
                using (var linq = new linqDataContext())
                {
                    loginResult = linq.Login(req.correo, req.password).FirstOrDefault();
                }

                if (loginResult == null)
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.UsuarioNoEncontrado,
                        Message = "Usuario no encontrado o no activado."
                    });
                    res.resultado = false;
                    return res;
                }

                res.Usuario = new Usuario
                {
                    id = loginResult.ID_USUARIO,
                    nombre = loginResult.NOMBRE,
                    correoElectronico = loginResult.CORREO_ELECTRONICO
                };
                string jwtToken = GenerarJwtToken(res.Usuario); // ← Generar el token JWT
                var sesion = new Sesion
                {
                    token = jwtToken,
                    usuario = res.Usuario,
                    origen = req.origen,
                    fechaCreacion = DateTime.Now
                };

                var logSesion = new LogicaSesion();
                logSesion.AbrirSesion(sesion);

                res.resultado = true;
                return res;
            }
            catch (Exception ex)
            {
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
                res.resultado = false;
                return res;
            }
        }

        public ResObtenerUsuario ObtenerUsuario(ReqObtenerUsuario req)
        {
            var res = new ResObtenerUsuario();
            res.listaDeErrores = new List<Error>();

            try
            {
                if (req.idUsuario <= 0)
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.UsuarioFaltante,
                        Message = "ID de usuario inválido."
                    });
                    res.resultado = false;
                    return res;
                }

                var result = new GetUserByIdResult();
                using (var linq = new linqDataContext())
                {
                    result = linq.GetUserById(req.idUsuario).FirstOrDefault();
                }

                if (result == null)
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.UsuarioNoEncontrado,
                        Message = "Usuario no encontrado."
                    });
                    res.resultado = false;
                    return res;
                }

                res.usuario = new Usuario
                {
                    id = result.ID_USUARIO,
                    nombre = result.NOMBRE,
                    correoElectronico = result.CORREO_ELECTRONICO
                };

                res.resultado = true;
                return res;
            }
            catch (Exception ex)
            {
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
                res.resultado = false;
                return res;
            }
        }

        public ResActivarUsuario ActivarUsuario(ReqActivarUsuario req)
        {
            var res = new ResActivarUsuario();
            res.listaDeErrores = new List<Error>();

            try
            {
                int? idUsuario = 0;
                int? errorId = 0;
                string errorBD = "";
                int? filasAfectadas = 0;

                using (var linq = new linqDataContext())
                {
                    linq.ActiveUser(req.correo, req.codigo, ref idUsuario, ref errorId, ref errorBD, ref filasAfectadas);
                }

                if (errorId != 0)
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = (EnumErrores)errorId,
                        Message = errorBD
                    });
                    res.resultado = false;
                }
                else
                {
                    res.resultado = true;
                }

                return res;
            }
            catch (Exception ex)
            {
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
                res.resultado = false;
                return res;
            }
        }

        public ResCerrarSesion CerrarSesion(ReqCerrarSesion req)
        {
            var res = new ResCerrarSesion();

            try
            {
                // Validación de parámetros requeridos
                if (string.IsNullOrWhiteSpace(req.token))
                {
                    res.resultado = false;
                    res.listaDeErrores = new List<Error>
            {
                new Error
                {
                    ErrorCode = EnumErrores.CampoRequeridoFaltante,
                    Message = "El token de sesión es requerido para cerrar sesión."
                }
            };
                    return res;
                }

                using (var linq = new linqDataContext())
                {
                    // Llamada al stored procedure InvalidateSession
                    // Este SP actualiza UserSessions SET IsActive = 0 WHERE Token = @Token
                    linq.InvalidateSession(req.token);

                    // Operación exitosa
                    res.resultado = true;
                    res.mensaje = "Sesión cerrada correctamente.";
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores = new List<Error>
        {
            new Error
            {
                ErrorCode = EnumErrores.ErrorNoControlado,
                Message = ex.Message
            }
        };
            }

            return res;
        }

        private void ValidarUsuario(Usuario usuario, List<Error> errores)
        {
            if (string.IsNullOrEmpty(usuario.nombre))
                errores.Add(new Error { ErrorCode = EnumErrores.NombreNuloOVacio, Message = "El nombre es requerido." });

            if (string.IsNullOrEmpty(usuario.correoElectronico))
                errores.Add(new Error { ErrorCode = EnumErrores.CorreoNuloOVacio, Message = "El correo es requerido." });
            else if (!EsCorreoValido(usuario.correoElectronico))
                errores.Add(new Error { ErrorCode = EnumErrores.FormatoCorreoInvalido, Message = "Formato de correo inválido." });

            if (string.IsNullOrEmpty(usuario.password))
                errores.Add(new Error { ErrorCode = EnumErrores.PasswordNuloOVacio, Message = "La contraseña es requerida." });
            else if (!EsPasswordFuerte(usuario.password))
                errores.Add(new Error { ErrorCode = EnumErrores.PasswordDebil, Message = "La contraseña es débil." });
        }

        private bool EsPasswordFuerte(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));
        }

        private bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }

        private string GenerarCodigoVerificacion()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerarJwtToken(Usuario usuario)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Felipensativo"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, usuario.correoElectronico),
        new Claim("id", usuario.id.ToString()),
        new Claim("nombre", usuario.nombre),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: "tuApp",
                audience: "tusUsuarios",
                claims: claims,
                expires: DateTime.Now.AddHours(4),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }


}
