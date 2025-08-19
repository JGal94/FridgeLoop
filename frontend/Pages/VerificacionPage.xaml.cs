using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class VerificacionPage : ContentPage
    {
        private readonly AuthService _auth = new();
        private readonly string? _correo; // correo pasado desde Registro

        // Constructor para navegación desde Registro (pasa el correo)
        public VerificacionPage(string correo)
        {
            InitializeComponent();
            _correo = correo;
        }

        // Constructor sin parámetros (lo requiere XAML/preview). Úsalo solo si necesitas abrir manual.
        public VerificacionPage()
        {
            InitializeComponent();
        }

        private async void OnActivateClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo?.Text))
            {
                await DisplayAlert("Falta", "Código de verificación", "OK");
                return;
            }

            // Si se abrió sin pasar correo, pídeselo al usuario en otra entrada (no existe en tu XAML actual),
            // pero en el flujo normal viene desde Registro:
            if (string.IsNullOrWhiteSpace(_correo))
            {
                await DisplayAlert("Atención", "Vuelve desde Registro para continuar la verificación.", "OK");
                return;
            }

            try
            {
                var res = await _auth.ActivarAsync(_correo!, txtCodigo.Text!.Trim());
                if (res.resultado)
                {
                    await DisplayAlert("Verificación", "¡Cuenta activada! Ahora inicia sesión.", "OK");
                    await Navigation.PushAsync(new LoginPage());
                }
                else
                {
                    var msg = res.listaDeErrores.FirstOrDefault()?.message ?? "Código inválido o expirado";
                    await DisplayAlert("Verificación", msg, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un problema al activar: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}
