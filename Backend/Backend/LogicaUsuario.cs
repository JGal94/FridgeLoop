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
using BCrypt.Net;


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

                // 🔐 Hash de la contraseña
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.usuario.password);

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

                int? idUsuario = 0;
                int? errorId = 0;
                string errorMensaje = "";

                using (var linq = new linqDataContext())
                {
                    linq.InsertUser(
                        req.usuario.nombre,
                        req.usuario.correoElectronico,
                        hashedPassword, // Guardamos la contraseña encriptada
                        codigoVerificacion,
                        ref idUsuario,
                        ref errorId,
                        ref errorMensaje
                    );
                }

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
            var res = new ResLogin { listaDeErrores = new List<Error>() };

            try
            {
                if (string.IsNullOrWhiteSpace(req.correo) || string.IsNullOrWhiteSpace(req.password))
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.CredencialesIncorrectas,
                        Message = "Correo o contraseña vacíos."
                    });
                    return res;
                }

                GetUserByEmailResult userRow;
                using (var linq = new linqDataContext())
                    userRow = linq.GetUserByEmail(req.correo).FirstOrDefault();

                if (userRow == null)
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.UsuarioNoEncontrado,
                        Message = "Usuario no encontrado."
                    });
                    return res;
                }

                if (!(userRow.IS_ACTIVE ?? false))
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.UsuarioNoActivado,
                        Message = "El usuario no está activado."
                    });
                    return res;
                }

                var hash = userRow.PASSWORD_HASH ?? string.Empty;
                if (!BCrypt.Net.BCrypt.Verify(req.password, hash))
                {
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.CredencialesIncorrectas,
                        Message = "Correo o contraseña incorrectos."
                    });
                    return res;
                }

                var usuario = new Usuario
                {
                    id = userRow.ID_USUARIO,  
                    nombre = userRow.NOMBRE,
                    correoElectronico = userRow.CORREO_ELECTRONICO
                };
                res.Usuario = usuario;

                // JWT y sesión en UTC
                var nowUtc = DateTime.UtcNow;
                var expiresUtc = nowUtc.AddHours(_jwt.HoursToExpire);

                var token = GenerarJwtToken(usuario);  // usa UtcNow adentro
                res.TokenJwt = token;

                var sesion = new Sesion
                {
                    token = token,
                    usuario = usuario,
                    origen = string.IsNullOrWhiteSpace(req.origen) ? "api" : req.origen,
                    direccionIP = req.direccionIP ?? string.Empty,
                    fechaCreacion = nowUtc,
                    fechaExpiracion = expiresUtc
                };

                var resSesion = new LogicaSesion().AbrirSesion(sesion);
                if (!resSesion.resultado)
                {
                    if (resSesion.listaDeErrores != null) res.listaDeErrores.AddRange(resSesion.listaDeErrores);
                    return res;
                }

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
            res.listaDeErrores = new List<Error>();

            try
            {
                // Validación del token
                if (string.IsNullOrWhiteSpace(req.token))
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.CampoRequeridoFaltante,
                        Message = "El token de sesión es requerido para cerrar sesión."
                    });
                    return res;
                }

                int? errorId = 0;
                string errorMensaje = "";

                using (var linq = new linqDataContext())
                {
                    // Llamar al SP con parámetros de salida
                    linq.CloseUserSession(req.token, ref errorId, ref errorMensaje);
                }

                // Si hubo error según SP
                if (errorId != 0)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = (EnumErrores)errorId.Value,
                        Message = errorMensaje
                    });
                    return res;
                }

                // Operación exitosa
                res.resultado = true;
                res.mensaje = "Sesión cerrada correctamente.";
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
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
        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>

        private readonly JwtSettings _jwt;
        public LogicaUsuario(JwtSettings jwt) { _jwt = jwt; }

        private string GenerarJwtToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var nowUtc = DateTime.UtcNow;
            var expiresUtc = nowUtc.AddHours(_jwt.HoursToExpire);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, usuario.correoElectronico),
        new Claim("id", usuario.id.ToString()),
        new Claim("nombre", usuario.nombre),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                notBefore: nowUtc,          // <-- importante
                expires: expiresUtc,       // <-- en UTC
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



    }


}
