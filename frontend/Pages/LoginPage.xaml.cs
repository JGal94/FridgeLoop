using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;
using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _auth = new();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCorreoLogin?.Text))
            {
                await DisplayAlert("Falta", "Correo", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPasswordLogin?.Text))
            {
                await DisplayAlert("Falta", "Contraseña", "OK");
                return;
            }

            try
            {
                var r = await _auth.LoginAsync(txtCorreoLogin.Text!.Trim(), txtPasswordLogin.Text!);

                if (r.resultado && !string.IsNullOrWhiteSpace(r.tokenJwt) && r.usuario != null)
                {
                    Sesion.Id = r.usuario.id;
                    Sesion.Nombre = r.usuario.nombre;
                    Sesion.Correo = r.usuario.correoElectronico;
                    Sesion.Token = r.tokenJwt;

                    await SecureStorage.SetAsync("auth_token", r.tokenJwt);

                    await DisplayAlert("Bienvenido", $"Hola, {Sesion.Nombre}", "OK");
                    await Navigation.PushAsync(new DashboardPage());
                }
                else
                {
                    var msg = r.listaDeErrores.FirstOrDefault()?.message ?? "Credenciales inválidas o usuario no verificado";
                    await DisplayAlert("Login", msg, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un problema al iniciar sesión: {ex.Message}", "OK");
            }
        }
    }
}
