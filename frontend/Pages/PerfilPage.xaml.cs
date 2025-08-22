using System;
using System.Threading;
using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;
using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class PerfilPage : ContentPage
    {
        private readonly PerfilService _svc = new();

        public PerfilPage()
        {
            InitializeComponent();

            // Si tienes Sesion.Nombre, muéstralo por conveniencia
            if (!string.IsNullOrWhiteSpace(Sesion.Nombre))
                txtNombre.Text = Sesion.Nombre;
        }

        private async void OnGuardarNombreClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var nuevo = txtNombre?.Text?.Trim();
                if (string.IsNullOrWhiteSpace(nuevo))
                {
                    await DisplayAlert("Falta", "Escribe el nuevo nombre.", "OK");
                    return;
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var r = await _svc.CambiarNombreAsync(nuevo, cts.Token);

                if (r?.resultado == true)
                {
                    // Actualiza nombre en memoria si usas Sesion
                    Sesion.Nombre = r.nombreActual ?? nuevo;
                    await DisplayAlert("Listo", "Nombre actualizado.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", PerfilService.FirstError(r, "No se pudo actualizar el nombre."), "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }

        private async void OnCambiarPasswordClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var actual = txtPwdActual?.Text ?? "";
                var nueva = txtPwdNueva?.Text ?? "";
                var confirmar = txtPwdConfirmar?.Text ?? "";

                if (string.IsNullOrWhiteSpace(actual) ||
                    string.IsNullOrWhiteSpace(nueva) ||
                    string.IsNullOrWhiteSpace(confirmar))
                {
                    await DisplayAlert("Faltan datos", "Completa todas las contraseñas.", "OK");
                    return;
                }

                if (nueva != confirmar)
                {
                    await DisplayAlert("Validación", "La confirmación no coincide.", "OK");
                    return;
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var r = await _svc.CambiarPasswordAsync(actual, nueva, confirmar, cts.Token);

                if (r?.resultado == true)
                {
                    await DisplayAlert("Listo", "Contraseña actualizada.", "OK");
                    // Opcional: obliga a re-login si tu backend invalida sesiones
                    // await SecureStorage.SetAsync("auth_token", ""); // o Remove
                }
                else
                {
                    await DisplayAlert("Error", PerfilService.FirstError(r, "No se pudo actualizar la contraseña."), "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }
    }
}
