using Microsoft.Maui.Controls;

using Frontend_Proyecto_Fridgeloop.Services;
using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class DetalleProductoPage : ContentPage
    {
        private readonly ProductService _svc = new();
        private readonly ProductService.ProductoDto _p;

        public DetalleProductoPage(ProductService.ProductoDto p)
        {
            InitializeComponent();
            _p = p;

            // Mostrar solo el nombre, y precargar cantidad/fecha si vienen
            lblNombre.Text = _p.Name;
            txtCantidad.Text = _p.Quantity?.ToString();
            if (_p.ExpirationDate.HasValue)
                dpExpira.Date = _p.ExpirationDate.Value;
        }

        private async void OnGuardarInventarioClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Si vino sin id, intentamos resolverlo por nombre
                if (_p.ProductID <= 0)
                {
                    using var ctsId = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    var resolved = await _svc.ResolverIdPorNombreAsync(_p.Name, ctsId.Token);
                    _p.ProductID = resolved;
                }

                if (_p.ProductID <= 0)
                {
                    await DisplayAlert("Dato faltante",
                        "No se encontró el id del producto. Regresa al inventario y vuelve a abrir el detalle.",
                        "OK");
                    return;
                }

                decimal? qty = null;
                if (decimal.TryParse(txtCantidad.Text?.Trim(), out var q)) qty = q;
                DateTime? exp = dpExpira.Date;

                var req = new ProductService.ReqActualizarInventario
                {
                    ProductID = _p.ProductID,
                    Quantity = qty,
                    ExpirationDate = exp
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var r = await _svc.ActualizarInventarioAsync(req, cts.Token);

                await DisplayAlert(r?.resultado == true ? "Ok" : "Error",
                    ProductService.FirstError(r, r?.resultado == true ? "Inventario actualizado." : "No se pudo actualizar el inventario."),
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }



    }
}