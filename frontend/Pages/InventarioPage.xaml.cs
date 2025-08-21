using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class InventarioPage : ContentPage
    {
        private readonly ProductService _svc = new();
        private int _page = 1;
        private const int _pageSize = 20;
        private string? _q;
        private CancellationTokenSource? _ctsSearch;

        public InventarioPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarAsync();
        }

        private async Task CargarAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                lblPagina.Text = $"Página {_page}";
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

                var lista = await _svc.ObtenerListaAsync(_page, _pageSize, _q, cts.Token);
                if (lista != null)
                {
                    cvInventario.ItemsSource = lista; // ProductoDto (Name, Unit, Quantity, ExpirationDate)
                }
                else
                {
                    // Ayuda de depuración: JSON crudo
                    var raw = await _svc.ObtenerRawAsync(_page, _pageSize, _q, cts.Token);
                    await DisplayAlert("Error", "No se pudo obtener el inventario.\n\nDetalle:\n" + raw, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }

        private async void OnRefreshRequested(object sender, EventArgs e)
        {
            _page = 1;
            await CargarAsync();
            if (sender is RefreshView rv) rv.IsRefreshing = false;
        }

        private async void OnBuscarTextChanged(object sender, TextChangedEventArgs e)
        {
            _ctsSearch?.Cancel();
            _ctsSearch = new CancellationTokenSource();
            try
            {
                await Task.Delay(400, _ctsSearch.Token); // debounce
                _q = string.IsNullOrWhiteSpace(txtBuscar?.Text) ? null : txtBuscar.Text.Trim();
                _page = 1;
                await CargarAsync();
            }
            catch (TaskCanceledException) { }
        }

        private async void OnPrevClicked(object sender, EventArgs e)
        {
            if (_page > 1)
            {
                _page--;
                await CargarAsync();
            }
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            _page++; // si luego tu API devuelve "total", aquí puedes limitar
            await CargarAsync();
        }

        private async void OnIrDetalleClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is ProductService.ProductoDto p)
            {
                await Navigation.PushAsync(new DetalleProductoPage(p));
            }
        }
    }
}