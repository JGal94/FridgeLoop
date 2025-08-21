using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;              // SecureStorage
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
                // (Opcional) ver si hay token en Android/iOS físico
                var tok = await SecureStorage.GetAsync("auth_token");
                System.Diagnostics.Debug.WriteLine($"[Inventario] token? {(string.IsNullOrWhiteSpace(tok) ? "NO" : "SÍ")}");

                lblPagina.Text = $"Página {_page}";

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

                // <-- Tu servicio devuelve una LISTA directa
                var lista = await _svc.ObtenerListaAsync(_page, _pageSize, _q, cts.Token);

                if (lista != null)
                {
                    // Bind directo
                    cvInventario.ItemsSource = lista; // List<ProductService.ProductoDto>
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo obtener el inventario.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
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
                // Debounce
                await Task.Delay(400, _ctsSearch.Token);

                _q = string.IsNullOrWhiteSpace(txtBuscar?.Text)
                    ? null
                    : txtBuscar.Text.Trim();

                _page = 1;
                await CargarAsync();
            }
            catch (TaskCanceledException)
            {
                // ignorar si se canceló por tecleo continuo
            }
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
            _page++;   // Si luego agregas total páginas desde backend, aquí puedes limitar
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
