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
            try { _cts?.Cancel(); } catch { }
        }

        private async Task CargarAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                var items = await _svc.ObtenerNotificacionesAsync(
                    dias: 7, incluirVencidos: true, maxDiasVencidos: 7,
                    page: 1, pageSize: 50, ct: _cts.Token);

                cvNotificaciones.ItemsSource = items;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", NotificacionesService.FirstError(null, ex.Message), "OK");
            }
        }

        private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection == null || e.CurrentSelection.Count == 0) return;
                var noti = e.CurrentSelection[0] as NotificacionesService.NotificacionItem;
                ((CollectionView)sender).SelectedItem = null;
                if (noti == null) return;

                // Si quieres ir al detalle del producto, arma el DTO mínimo:
                var p = new ProductService.ProductoDto
                {
                    ProductID = noti.IdProducto,
                    Name = noti.Titulo,
                    Unit = noti.Unidad,
                    Quantity = noti.Cantidad,
                    ExpirationDate = noti.FechaExpira
                };

                await Navigation.PushAsync(new DetalleProductoPage(p));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
