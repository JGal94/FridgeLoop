using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using AccesoDatos;          // linqDataContext + SPs del .dbml
using Entidades.Entity;    // Error
using Entidades.Enum;      // EnumErrores
using Entidades.Request;
using Entidades.Response;




namespace Backend
{
    public class LogicaPerfil
    {
        
            // Política simple: min 8, mayúscula, minúscula y dígito
            private bool PasswordFuerte(string pwd)
            {
                if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 8) return false;
                bool up = false, low = false, dig = false;
                foreach (var c in pwd)
                {
                    if (char.IsUpper(c)) up = true;
                    else if (char.IsLower(c)) low = true;
                    else if (char.IsDigit(c)) dig = true;
                    if (up && low && dig) return true;
                }
                return false;
            }

        public ResCambiarPassword CambiarPassword(int userId, ReqCambiarPassword req)
        {
            var res = new ResCambiarPassword { listaDeErrores = new List<Error>() };

            try
            {
                if (userId <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." });
                    return res;
                }
                if (req == null)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.RequestNulo, Message = "El request no puede ser nulo." });
                    return res;
                }
                if (string.IsNullOrWhiteSpace(req.passwordActual) ||
                    string.IsNullOrWhiteSpace(req.passwordNueva) ||
                    string.IsNullOrWhiteSpace(req.confirmarPassword))
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.CampoRequeridoFaltante, Message = "Contraseñas requeridas." });
                    return res;
                }
                if (req.passwordNueva != req.confirmarPassword)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.FormatoDatoInvalido, Message = "La confirmación no coincide." });
                    return res;
                }
                if (!PasswordFuerte(req.passwordNueva))
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.PasswordDebil, Message = "La nueva contraseña no cumple la política mínima." });
                    return res;
                }
                if (req.passwordNueva == req.passwordActual)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.FormatoDatoInvalido, Message = "La nueva contraseña no puede ser igual a la actual." });
                    return res;
                }

                using (var linq = new linqDataContext())
                {
                    // 1) Trae la info de seguridad (hash + activo) SIN tocar tu GetUserById
                    var sec = linq.GetUserSecurityById(userId).FirstOrDefault();
                    if (sec == null || !(sec.IS_ACTIVE ?? false))
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.UsuarioNoEncontrado, Message = "Usuario no encontrado o inactivo." });
                        return res;
                    }

                    var hashActual = sec.PASSWORD_HASH;
                    if (string.IsNullOrEmpty(hashActual) || !BCrypt.Net.BCrypt.Verify(req.passwordActual, hashActual))
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.CredencialesIncorrectas, Message = "La contraseña actual es incorrecta." });
                        return res;
                    }

                    // 2) Nuevo hash
                    var nuevoHash = BCrypt.Net.BCrypt.HashPassword(req.passwordNueva);

                    // 3) Actualiza hash vía SP
                    int? err = 0; string msg = "";
                    linq.SP_Usuario_CambiarPassword(userId, nuevoHash, ref err, ref msg);

                    if ((err ?? 0) != 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.ErrorDeBaseDatos, Message = msg ?? "No fue posible actualizar la contraseña." });
                        return res;
                    }

                    // (Opcional) Invalidar sesiones:
                    // linq.SP_Sesion_CerrarTodasPorUsuario(userId);

                    res.resultado = true;
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                if (res.listaDeErrores == null) res.listaDeErrores = new List<Error>();
                res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.ErrorNoControlado, Message = ex.Message });
                return res;
            }
        }


        public ResActualizarNombre ActualizarNombre(int userId, ReqActualizarNombre req)
            {
                var res = new ResActualizarNombre { listaDeErrores = new List<Error>() };

                try
                {
                    if (userId <= 0)
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.TokenInvalido, Message = "Usuario no autenticado." });
                        return res;
                    }
                    if (req == null || string.IsNullOrWhiteSpace(req.nuevoNombre))
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.NombreNuloOVacio, Message = "El nombre no puede estar vacío." });
                        return res;
                    }
                    if (req.nuevoNombre.Length > 100)
                    {
                        res.resultado = false;
                        res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.FormatoDatoInvalido, Message = "El nombre supera el máximo permitido." });
                        return res;
                    }

                    using (var linq = new linqDataContext())
                    {
                        int? err = 0; string msg = "";
                        linq.SP_Usuario_ActualizarNombre(userId, req.nuevoNombre.Trim(), ref err, ref msg);

                        if ((err ?? 0) != 0)
                        {
                            res.resultado = false;
                            res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.ErrorDeBaseDatos, Message = msg ?? "No fue posible actualizar el nombre." });
                            return res;
                        }

                        res.resultado = true;
                        
                        res.nombreActual = req.nuevoNombre.Trim();
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    res.resultado = false;
                    if (res.listaDeErrores == null) res.listaDeErrores = new List<Error>();
                    res.listaDeErrores.Add(new Error { ErrorCode = EnumErrores.ErrorNoControlado, Message = ex.Message });
                    return res;
                }
            
        }

    }
}
