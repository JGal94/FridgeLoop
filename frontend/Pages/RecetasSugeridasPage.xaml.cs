using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class RecetasSugeridasPage : ContentPage
    {
        private readonly RecetaService _svcRecetas = new();
        private CancellationTokenSource? _cts;
        private bool _busy;

        public RecetasSugeridasPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try { _cts?.Cancel(); } catch { }
        }

        // Botón "Generar"
        private async void OnGenerarClicked(object sender, EventArgs e)
        {
            if (_busy) return;

            _busy = true;
            btnGenerar.IsEnabled = false;
            ai.IsVisible = ai.IsRunning = true;
            lblEstado.Text = "Generando recetas...";

            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource(TimeSpan.FromSeconds(75));

                // Ahora el servicio toma el idUsuario internamente
                var r = await _svcRecetas.ObtenerRecetasIAAsync(_cts.Token);

                if (r?.resultado == true && r.recetas?.Any() == true)
                {
                    cv.ItemsSource = r.recetas;
                    lblEstado.Text = "Recetas generadas por IA.";
                }
                else
                {
                    await DisplayAlert("IA", r?.mensaje ?? "No se pudieron generar recetas. Intenta de nuevo.", "OK");
                    lblEstado.Text = "— Aún no hay recetas —";
                    cv.ItemsSource = null;
                }
            }
            catch (TaskCanceledException)
            {
                await DisplayAlert("IA", "La solicitud se canceló o tardó demasiado.", "OK");
                lblEstado.Text = "La solicitud se canceló o tardó demasiado.";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                lblEstado.Text = "Error al generar recetas.";
            }
            finally
            {
                ai.IsRunning = false;
                ai.IsVisible = false;
                btnGenerar.IsEnabled = true;
                _busy = false;
            }
        }

        // Botón "Preparar"
        private async void OnPrepararClicked(object sender, EventArgs e)
        {
            try
            {
                var recetaObj =
                    (sender as Button)?.CommandParameter
                    ?? (sender as Element)?.BindingContext;

                if (recetaObj == null)
                {
                    await DisplayAlert("Ups", "No se pudo encontrar la receta.", "OK");
                    return;
                }

                string? nombre =
                    recetaObj.GetType().GetProperty("Name")?.GetValue(recetaObj)?.ToString()
                    ?? recetaObj.GetType().GetProperty("Nombre")?.GetValue(recetaObj)?.ToString()
                    ?? "(receta)";

                await DisplayAlert("Preparar receta", $"Vas a preparar: {nombre}", "OK");
                // Aquí podrías navegar a un detalle o llamar _svcRecetas.PrepararRecetaAsync(...)
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await DisplayAlert("Error", "No se pudo abrir la receta.", "OK");
            }
        }
    }
}
