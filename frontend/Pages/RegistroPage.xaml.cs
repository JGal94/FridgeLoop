using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class RegistroPage : ContentPage
    {
        private readonly AuthService _auth = new();

        public RegistroPage()
        {
            InitializeComponent();
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre?.Text)) { DisplayAlert("Falta", "Nombre", "OK"); return false; }
            if (string.IsNullOrWhiteSpace(txtCorreo?.Text)) { DisplayAlert("Falta", "Correo", "OK"); return false; }
            if (string.IsNullOrWhiteSpace(txtPassword?.Text)) { DisplayAlert("Falta", "Contraseña", "OK"); return false; }
            return true;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (!Validar()) return;

            try
            {
                var res = await _auth.RegistrarAsync(
                    txtNombre.Text!.Trim(),
                    txtCorreo.Text!.Trim(),
                    txtPassword.Text!
                );

                if (res.resultado)
                {
                    await DisplayAlert("Registro", "Te enviamos un código de verificación a tu correo.", "OK");
                    await Navigation.PushAsync(new VerificacionPage(txtCorreo.Text!.Trim()));
                }
                else
                {
                    var msg = res.listaDeErrores.FirstOrDefault()?.message ?? "No se pudo registrar";
                    await DisplayAlert("Registro", msg, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un problema al registrarte: {ex.Message}", "OK");
            }
        }

    }
}
