using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class NotificacionesPage : ContentPage
    {
        private readonly NotificacionesService _svc = new();
        private CancellationTokenSource? _cts;

        public NotificacionesPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try { _cts?.Cancel(); } catch { /* ignorar */ }
        }

        private async Task CargarAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            try
            {
                var items = await _svc.ObtenerNotificacionesAsync(
                    dias: 7,
                    incluirVencidos: true,
                    maxDiasVencidos: 7,
                    page: 1,
                    pageSize: 100,
                    ct: _cts.Token
                );

                cvNotificaciones.ItemsSource = items;
            }
            catch (TaskCanceledException) { /* ignorar */ }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notificaciones] error: {ex}");
                await DisplayAlert("Error", "No se pudieron cargar las notificaciones.", "OK");
            }
        }

        // ==== Selección por CollectionView ====
        private async void OnNotificacionSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var col = (CollectionView)sender;
                var noti = e.CurrentSelection?.FirstOrDefault() as NotificacionesService.NotificacionItem;

                // limpiar la selección visual para que no quede marcada
                col.SelectedItem = null;

                if (noti == null) return;

                await NavegarADetalleAsync(noti);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        // ==== TapGesture en el item ====
        private async void OnItemTapped(object sender, TappedEventArgs e)
        {
            try
            {
                var noti = e.Parameter as NotificacionesService.NotificacionItem
                           ?? (sender as Element)?.BindingContext as NotificacionesService.NotificacionItem;

                if (noti == null) return;

                await NavegarADetalleAsync(noti);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        // ==== Ir a la pantalla de Detalle con Id explícito ====
        private async Task NavegarADetalleAsync(NotificacionesService.NotificacionItem noti)
        {
            // Armamos el DTO con la información disponible en la notificación
            var p = new ProductService.ProductoDto
            {
                Name = noti.Titulo ?? "(sin nombre)",
                Unit = noti.Unidad ?? "",
                Quantity = noti.Cantidad,
                ExpirationDate = noti.FechaExpira
            };

            // Pasamos el idProducto como segundo argumento (sin nombre de parámetro)
            await Navigation.PushAsync(new DetalleProductoPage(p, noti.IdProducto));
        }
    }
}
