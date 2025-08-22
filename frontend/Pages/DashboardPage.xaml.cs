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
        private async Task CargarProductosPorVencerAsync()
        {
            // Estados iniciales
            lblPVTitle.Text = "Productos por vencer";
            lblPVSubtitle.Text = "Cargando…";
            btnVerNotificaciones.IsEnabled = false;

            _cts?.Cancel();
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            try
            {
                // mismos parámetros que usamos en NotificacionesPage
                var list = await _noti.ObtenerNotificacionesAsync(
                    dias: 7, incluirVencidos: true, maxDiasVencidos: 7,
                    page: 1, pageSize: 50, ct: _cts.Token);

                var total = list?.Count ?? 0;

                if (total <= 0)
                {
                    lblPVSubtitle.Text = "No hay productos por vencer en la próxima semana.";
                    btnVerNotificaciones.IsEnabled = false;
                    return;
                }

                var vencidos = list!.Count(n => n.Tipo == "danger");
                var porVencer = total - vencidos;

                // Título fijo y subtítulo con conteo
                lblPVTitle.Text = "Productos por vencer";
                lblPVSubtitle.Text = porVencer > 0
                    ? $"{porVencer} producto(s) vencen esta semana" + (vencidos > 0 ? $" • {vencidos} vencido(s)" : "")
                    : $"{vencidos} producto(s) vencido(s)";

                btnVerNotificaciones.IsEnabled = true;
            }
            catch (TaskCanceledException) { /* ignorar */ }
            catch (Exception ex)
            {
                lblPVSubtitle.Text = "No se pudieron cargar las notificaciones.";
                System.Diagnostics.Debug.WriteLine($"[Dashboard] {ex}");
                btnVerNotificaciones.IsEnabled = true; // deja ver la lista por si acaso
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
