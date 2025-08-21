using Frontend_Proyecto_Fridgeloop.Services;
using Frontend_Proyecto_Fridgeloop.Helpers;
using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class AgregarProductoPage : ContentPage
    {
        private readonly ProductService _svc = new();
        private CancellationTokenSource? _cts;

        // Mapeos 1:1 con los Items de los Pickers en XAML
        private readonly int[] _categoryIdByIndex = new[] { 1, 2, 3, 4, 99 };
        private readonly string[] _unidadByIndex = { "kg", "g", "ml", "L", "pz", "unid", "pack" };

        public AgregarProductoPage()
        {
            InitializeComponent();

            // valores por defecto (ajústalos si quieres)
            if (pkCategoria.SelectedIndex < 0) pkCategoria.SelectedIndex = 0;
            if (pkUnidad.SelectedIndex < 0) pkUnidad.SelectedIndex = 0;
        }

        private async void OnGuardarProductoClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Validaciones mínimas (como las que ya tienes)
                if (string.IsNullOrWhiteSpace(txtNombre?.Text))
                { await DisplayAlert("Faltan datos", "Nombre obligatorio.", "OK"); return; }
                if (pkCategoria?.SelectedIndex is null || pkCategoria.SelectedIndex < 0)
                { await DisplayAlert("Faltan datos", "Selecciona una categoría.", "OK"); return; }
                if (pkUnidad?.SelectedIndex is null || pkUnidad.SelectedIndex < 0)
                { await DisplayAlert("Faltan datos", "Selecciona una unidad.", "OK"); return; }

                // Mapeos
                var catId = _categoryIdByIndex[pkCategoria.SelectedIndex];
                var unit = _unidadByIndex[pkUnidad.SelectedIndex];

                // Opcionales
                decimal cantidad = 1m;
                if (!string.IsNullOrWhiteSpace(txtCantidad?.Text) && decimal.TryParse(txtCantidad.Text.Trim(), out var q) && q > 0)
                    cantidad = q;

                DateTime? exp = dpExpira?.Date;

                // (Opcional) Si tienes un Entry de precio, úsalo; si no, queda null
                decimal? precio = null;
                if (!string.IsNullOrWhiteSpace(txtPrecio?.Text) && decimal.TryParse(txtPrecio.Text.Trim(), out var p) && p >= 0)
                    precio = p;

                // Crea ítem local y agrégalo a la lista de compras
                var item = new ShoppingItem
                {
                    Nombre = txtNombre.Text!.Trim(),
                    IdCategoria = catId,
                    Unidad = unit,
                    Cantidad = cantidad,
                    PrecioUnitario = precio,
                    FechaExpiracion = exp
                };
                ShoppingList.Items.Add(item);

                await DisplayAlert("Agregado", $"{item.Nombre} ? a la lista de compras.", "OK");
                await Navigation.PopAsync(); // volvemos a ListaComprasPage
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

        // Escáner con MessagingCenter (tu lógica)
        private async void OnEscanearClicked(object sender, EventArgs e)
        {
            // Evita dobles suscripciones si tocan varias veces
            try { MessagingCenter.Unsubscribe<object, string>(this, "BarcodeScanned"); } catch { }

            MessagingCenter.Subscribe<object, string>(this, "BarcodeScanned", (sender2, code) =>
            {
                codigoEntry.Text = code;
                MessagingCenter.Unsubscribe<object, string>(this, "BarcodeScanned");
            });

            // Intenta crear ScanPage; si no existe, avisa y sal
            var scanType = Type.GetType("Frontend_Proyecto_Fridgeloop.Pages.ScanPage");
            if (scanType == null)
            {
                await DisplayAlert("Escaner no disponible",
                    "Aun no está implementada la pantalla de escaneo.",
                    "OK");
                try { MessagingCenter.Unsubscribe<object, string>(this, "BarcodeScanned"); } catch { }
                return;
            }

            try
            {
                if (Activator.CreateInstance(scanType) is Page scanPage)
                    await Navigation.PushAsync(scanPage);
                else
                    await DisplayAlert("Escaner no disponible", "No se pudo crear la pantalla de escaneo.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Escaner no disponible", ex.Message, "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try { MessagingCenter.Unsubscribe<object, string>(this, "BarcodeScanned"); } catch { }
            _cts?.Cancel();
        }
    }
}
