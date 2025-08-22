using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
   public partial class DashboardPage : ContentPage
    {
        private readonly NotificacionesService _noti = new();
        private CancellationTokenSource? _cts;

        public DashboardPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarProductosPorVencerAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try { _cts?.Cancel(); } catch { }
        }

        /// <summary>
        /// Carga el resumen de productos por vencer para la tarjeta del dashboard.
        /// </summary>

        private bool _canTap; // controla la navegación

        private async Task CargarProductosPorVencerAsync()
        {
            lblPVTitle.Text = "Productos por vencer";
            lblPVSubtitle.Text = "Cargando…";

            // Deshabilitar interacción mientras carga
            _canTap = false;
            cardPV.InputTransparent = true; // bloquea toques
            cardPV.Opacity = 0.6;

            _cts?.Cancel();
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            try
            {
                var list = await _noti.ObtenerNotificacionesAsync(
                    dias: 7, incluirVencidos: true, maxDiasVencidos: 7,
                    page: 1, pageSize: 50, ct: _cts.Token);

                var total = list?.Count ?? 0;

                if (total <= 0)
                {
                    lblPVSubtitle.Text = "No hay productos por vencer en la próxima semana.";
                    _canTap = false;
                    cardPV.InputTransparent = true;
                    cardPV.Opacity = 0.6;
                    return;
                }

                var vencidos = list!.Count(n => n.Tipo == "danger");
                var porVencer = total - vencidos;

                lblPVSubtitle.Text = porVencer > 0
                    ? $"{porVencer} producto(s) vencen esta semana" + (vencidos > 0 ? $" • {vencidos} vencido(s)" : "")
                    : $"{vencidos} producto(s) vencido(s)";

                // Habilitar interacción
                _canTap = true;
                cardPV.InputTransparent = false;
                cardPV.Opacity = 1.0;
            }
            catch (TaskCanceledException)
            {
                lblPVSubtitle.Text = "Tiempo de espera agotado.";
                _canTap = false;
                cardPV.InputTransparent = true;
                cardPV.Opacity = 0.6;
            }
            catch (Exception ex)
            {
                lblPVSubtitle.Text = "No se pudieron cargar las notificaciones.";
                System.Diagnostics.Debug.WriteLine($"[Dashboard] {ex}");

                // Permitir navegar aún con error (si quieres)
                _canTap = true;
                cardPV.InputTransparent = false;
                cardPV.Opacity = 1.0;
            }
        }

        private bool _isNavigating;

        private async void OnFrameTapped(object sender, TappedEventArgs e)
        {
            if (!_canTap || _isNavigating) return;

            try
            {
                _isNavigating = true;
                await Navigation.PushAsync(new NotificacionesPage());
            }
            finally
            {
                _isNavigating = false;
            }
        }




        // ====== Botones del encabezado ======

        private async void OnNotificacionesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NotificacionesPage());
        }

        private async void OnPerfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PerfilPage());
        }

        private async void OnConfiguracionClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConfiguracionPage());
        }

        // Botón dentro de la tarjeta
        private async void OnVerTodasNotiClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NotificacionesPage());
        }
        

        


        // ====== Accesos rápidos ======

        private async void GoToInventario(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InventarioPage());
        }

        private async void GoToCompras(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListaComprasPage());
        }

        private async void GoToRecetas(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RecetasSugeridasPage());
        }

        private async void GoToGastos(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PresupuestoMensualPage());
        }
    }
}
