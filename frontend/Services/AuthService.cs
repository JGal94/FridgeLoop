using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;

        public AuthService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(Constants.BaseApi),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        // ==== DTOs según tu backend ====
        public class ErrorDto { public int errorCode { get; set; } public string message { get; set; } = ""; }
        public class ResBase { public bool resultado { get; set; } public List<ErrorDto> listaDeErrores { get; set; } = new(); }

        public class UsuarioDto { public string nombre { get; set; } = ""; public string correoElectronico { get; set; } = ""; public string password { get; set; } = ""; }
        public class ReqInsertarUsuario { public UsuarioDto usuario { get; set; } = new(); }

        public class ReqActivarUsuario { public string correo { get; set; } = ""; public string codigo { get; set; } = ""; }

        public class ReqLogin { public string correo { get; set; } = ""; public string password { get; set; } = ""; public string origen { get; set; } = "MAUI"; public string direccionIP { get; set; } = "0.0.0.0"; }
        public class UsuarioRes { public int id { get; set; } public string nombre { get; set; } = ""; public string correoElectronico { get; set; } = ""; public string? password { get; set; } }
        public class ResLogin : ResBase { public UsuarioRes? usuario { get; set; } public string tokenJwt { get; set; } = ""; }

        private StringContent ToJson(object body)
            => new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        public async Task<ResBase> RegistrarAsync(string nombre, string correo, string password)
        {
            var body = new ReqInsertarUsuario { usuario = new UsuarioDto { nombre = nombre, correoElectronico = correo, password = password } };
            var res = await _http.PostAsync("api/usuario/insertar", ToJson(body));
            var payload = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResBase>(payload) ?? new ResBase { resultado = false };
        }

        public async Task<ResBase> ActivarAsync(string correo, string codigo)
        {
            var res = await _http.PostAsync("api/usuario/activar", ToJson(new ReqActivarUsuario { correo = correo, codigo = codigo }));
            var payload = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResBase>(payload) ?? new ResBase { resultado = false };
        }

        public async Task<ResLogin> LoginAsync(string correo, string password)
        {
            var body = new ReqLogin { correo = correo, password = password, origen = "MAUI", direccionIP = "0.0.0.0" };
            var res = await _http.PostAsync("api/usuario/login", ToJson(body));
            var payload = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResLogin>(payload) ?? new ResLogin { resultado = false };
        }

        public void SetBearer(string token)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
