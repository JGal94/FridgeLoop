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
                // Requeridos mínimos
                if (string.IsNullOrWhiteSpace(txtNombre?.Text))
                { await DisplayAlert("Faltan datos", "Nombre obligatorio.", "OK"); return; }

                if (pkCategoria?.SelectedIndex is null || pkCategoria.SelectedIndex < 0)
                { await DisplayAlert("Faltan datos", "Selecciona una categoría.", "OK"); return; }

                if (pkUnidad?.SelectedIndex is null || pkUnidad.SelectedIndex < 0)
                { await DisplayAlert("Faltan datos", "Selecciona una unidad.", "OK"); return; }

                // Mapeos desde los Pickers
                var catId = _categoryIdByIndex[pkCategoria.SelectedIndex];
                var unit = _unidadByIndex[pkUnidad.SelectedIndex];

                // Opcionales
                decimal? qty = null;
                if (decimal.TryParse(txtCantidad?.Text?.Trim(), out var q)) qty = q;

                DateTime? exp = dpExpira?.Date;

                // DTO SIN UserID (el backend lo toma del token)
                var dto = new ProductService.ProductoDto
                {
                    Name = txtNombre.Text!.Trim(),
                    CategoryID = catId,
                    Unit = unit,         //  del Picker
                    Quantity = qty,
                    ExpirationDate = exp
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var r = await _svc.InsertarAsync(dto, cts.Token);

                if (r?.resultado == true)
                {
                    //AppEvents.InventoryDirty = true; // para refrescar Inventario al volver (si usas el banderín)
                    await DisplayAlert("OK", "Producto insertado.", "Cerrar");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", ProductService.FirstError(r, "No se pudo insertar."), "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepcion", ex.Message, "OK");
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
