using System.Globalization;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class CompraDetallePage : ContentPage   // ? partial y mismo nombre
    {
        private readonly CompraService _svc = new();
        private readonly int _idCompra;

        public class VM
        {
            public CompraService.CompraDetalleDto? compra { get; set; }
        }

        public CompraDetallePage(int idCompra)
        {
            InitializeComponent();            // ? ahora compila si el XAML está bien
            _idCompra = idCompra;
            BindingContext = new VM();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Cargar();
        }

        private async Task Cargar()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
                var res = await _svc.ObtenerCompraPorIdAsync(_idCompra, cts.Token);

                if (res?.resultado == true && res.compra != null)
                    BindingContext = new VM { compra = res.compra };
                else
                {
                    await DisplayAlert("Atención",
                        CompraService.FirstError(res, "No se pudo cargar el detalle."),
                        "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                await Navigation.PopAsync();
            }
        }
    }
    public class SubtotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;

            var t = value.GetType();
            var cantidad = (decimal)(t.GetProperty("cantidad")?.GetValue(value) ?? 0m);
            var precio = (decimal?)(t.GetProperty("precioUnitario")?.GetValue(value));
            var subtotal = cantidad * (precio ?? 0m);

            return subtotal.ToString("N2", culture);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
