using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Services
{
    /// <summary>
    /// Cliente para /api/perfil (PUT nombre, PUT password).
    /// Reutiliza HttpServiceBase (BaseApi, bearer, SendAsync).
    /// </summary>
    public class PerfilService : HttpServiceBase
    {
        // ====== contratos base (compatibles con tu backend) ======
        public class ErrorDto { public int ErrorCode { get; set; } public string Message { get; set; } = ""; }

        public class ResBase
        {
            public bool resultado { get; set; }
            public string? mensaje { get; set; }
            public System.Collections.Generic.List<ErrorDto>? listaDeErrores { get; set; }
        }

        public static string FirstError(ResBase? r, string fallback = "Ocurrió un error.")
            => r?.listaDeErrores?.FirstOrDefault()?.Message ?? r?.mensaje ?? fallback;

        // ====== Cambiar nombre ======
        public class ReqActualizarNombre { public string nuevoNombre { get; set; } = ""; }
        public class ResActualizarNombre : ResBase { public string? nombreActual { get; set; } }

        public Task<ResActualizarNombre?> CambiarNombreAsync(string nuevoNombre, CancellationToken ct = default)
        {
            var body = new ReqActualizarNombre { nuevoNombre = nuevoNombre };
            return SendAsync<ResActualizarNombre>(
                () => Http.PutAsync("api/perfil/nombre", J(body), ct),
                ct);
        }

        // ====== Cambiar password ======
        public class ReqCambiarPassword
        {
            public string passwordActual { get; set; } = "";
            public string passwordNueva { get; set; } = "";
            public string confirmarPassword { get; set; } = "";
        }
        public class ResCambiarPassword : ResBase { }

        public Task<ResCambiarPassword?> CambiarPasswordAsync(
            string actual, string nueva, string confirmar, CancellationToken ct = default)
        {
            var body = new ReqCambiarPassword
            {
                passwordActual = actual,
                passwordNueva = nueva,
                confirmarPassword = confirmar
            };

            return SendAsync<ResCambiarPassword>(
                () => Http.PutAsync("api/perfil/password", J(body), ct),
                ct);
        }
    }
}
