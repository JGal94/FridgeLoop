using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;


namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class HistorialComprasPage : ContentPage
    {
        private readonly CompraService _svc = new();

        public HistorialComprasPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarAsync();
        }

        private async Task CargarAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
                var res = await _svc.ObtenerComprasAsync(page, pageSize, cts.Token);

                if (res?.resultado == true || (res?.compras?.Any() ?? false))
                {
                    cvCompras.ItemsSource = res!.compras!;
                }
                else
                {
                    // Muestra el mensaje exacto (si vino del backend)
                    await DisplayAlert("Error",
                        CompraService.FirstError(res, "No se pudo obtener el historial."),
                        "OK");
                    cvCompras.ItemsSource = new List<CompraService.CompraDto>();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
        }
    }
}
