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
            // Evita doble tap sin usar IsBusy global
            if (sender is Button b) b.IsEnabled = false;

            try
            {
                // Validaciones mínimas
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
                if (!string.IsNullOrWhiteSpace(txtCantidad?.Text) &&
                    decimal.TryParse(txtCantidad.Text.Trim(), out var q) && q > 0) cantidad = q;

                DateTime? exp = dpExpira?.Date;

                decimal? precio = null;
                if (!string.IsNullOrWhiteSpace(txtPrecio?.Text) &&
                    decimal.TryParse(txtPrecio.Text.Trim(), out var p) && p >= 0) precio = p;

                // Crear y agregar a la lista observable
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

                await DisplayAlert("Agregado", $"{item.Nombre} a la lista de compras.", "OK");

                // Regresar a la lista (no hay spinner activo)
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
            finally
            {
                if (sender is Button b2) b2.IsEnabled = true;
            }
        }


        // Escáner con MessagingCenter (tu lógica)
        // Escanear y suscripción correcta
        private async void OnEscanearClicked(object sender, EventArgs e)
        {
            // Evita dobles suscripciones
            try { MessagingCenter.Unsubscribe<ScanPage, string>(this, "BarcodeScanned"); } catch { }

            // ?? Suscríbete al TIPO correcto: ScanPage
            MessagingCenter.Subscribe<ScanPage, string>(this, "BarcodeScanned", (sender2, code) =>
            {
                // Rellena el Entry con el código
                codigoEntry.Text = code;

                // (Opcional) dispara la búsqueda automáticamente:
                // OnBuscarPorCodigoClicked(this, EventArgs.Empty);

                // Ya no necesitamos seguir suscritos
                MessagingCenter.Unsubscribe<ScanPage, string>(this, "BarcodeScanned");
            });

            // Navega de forma directa (sin reflection)
            await Navigation.PushAsync(new ScanPage());
        }

        // Agregar este método a tu clase AgregarProductoPage

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            // Verificar si hay cambios sin guardar
            bool hasChanges = !string.IsNullOrWhiteSpace(txtNombre?.Text) ||
                             !string.IsNullOrWhiteSpace(txtCantidad?.Text) ||
                             !string.IsNullOrWhiteSpace(txtPrecio?.Text) ||
                             !string.IsNullOrWhiteSpace(codigoEntry?.Text) ||
                             pkCategoria.SelectedIndex > 0 ||
                             pkUnidad.SelectedIndex > 0;

            if (hasChanges)
            {
                bool confirm = await DisplayAlert(
                    "¿Cancelar?",
                    "Se perderán los cambios no guardados. ¿Continuar?",
                    "Sí, cancelar",
                    "No");

                if (!confirm) return;
            }

            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // ? NO te desuscribas aquí; si lo haces, pierdes el mensaje.
            // try { MessagingCenter.Unsubscribe<ScanPage, string>(this, "BarcodeScanned"); } catch { }

            // Esto sí: cancelar llamadas pendientes
            _cts?.Cancel();
        }


        private async void OnBuscarPorCodigoClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;

            var codigo = codigoEntry?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                await DisplayAlert("Falta el código", "Escanea o escribe un código de barras.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                var dto = await _svc.ObtenerPorCodigoAsync(codigo, _cts.Token);
                if (dto == null)
                {
                    await DisplayAlert("No encontrado", "No se halló un producto para ese código.", "OK");
                    return;
                }

                // Mapea a tus controles actuales
                txtNombre.Text = dto.Name;

                var catIndex = Array.IndexOf(_categoryIdByIndex, dto.CategoryID);
                if (catIndex >= 0) pkCategoria.SelectedIndex = catIndex;

                var uIndex = Array.FindIndex(_unidadByIndex, u =>
                    string.Equals(u, dto.Unit, StringComparison.OrdinalIgnoreCase));
                if (uIndex >= 0) pkUnidad.SelectedIndex = uIndex;

                txtCantidad.Text = (dto.Quantity ?? 1m).ToString();
                if (dto.ExpirationDate.HasValue) dpExpira.Date = dto.ExpirationDate.Value;
            }
            catch (TaskCanceledException)
            {
                await DisplayAlert("Tiempo agotado", "La solicitud tardó demasiado.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
