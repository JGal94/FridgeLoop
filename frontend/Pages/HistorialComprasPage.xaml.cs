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

                // ? Usa AND para aceptar solo respuestas exitosas con datos (o lista vacía válida)
                if (res?.resultado == true)
                {
                    cvCompras.ItemsSource = res.compras ?? new List<CompraService.CompraDto>();
                }
                else
                {
                    await DisplayAlert(
                        "Error",
                        CompraService.FirstError(res, "No se pudo obtener el historial."),
                        "OK"
                    );
                    cvCompras.ItemsSource = new List<CompraService.CompraDto>();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
                cvCompras.ItemsSource = new List<CompraService.CompraDto>();
            }
        }

        // ========= OPCIÓN A: si usas SelectionChanged en el CollectionView =========
        private async void cvCompras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var seleccionado = e.CurrentSelection?.FirstOrDefault();
            if (seleccionado == null) return;

            var idProp = seleccionado.GetType().GetProperty("idCompra");
            if (idProp == null) return;

            var id = (int)idProp.GetValue(seleccionado)!;

            await Navigation.PushAsync(new CompraDetallePage(id));

            // Limpia selección para permitir taps repetidos
            ((CollectionView)sender).SelectedItem = null;
        }

        // ========= OPCIÓN B: si usas TapGestureRecognizer en el Frame del ItemTemplate =========
        private async void Compra_Tapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is int idCompra)
            {
                await Navigation.PushAsync(new CompraDetallePage(idCompra));
            }
        }
    }
}
