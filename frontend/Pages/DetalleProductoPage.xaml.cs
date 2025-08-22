using System;
using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class DetalleProductoPage : ContentPage
    {
        private int? _idProducto;

        // Constructor que recibe el DTO y, opcionalmente, el idProducto
        public DetalleProductoPage(ProductService.ProductoDto p, int? idProducto = null)
        {
            InitializeComponent();

            _idProducto = idProducto;

            // Mostrar siempre el nombre
            txtNombre.Text = p?.Name ?? "(sin nombre)";

            // Precargar cantidad y fecha si vienen
            if (p?.Quantity != null)
                txtCantidad.Text = p.Quantity.Value.ToString("0.##");

            dpExpira.Date = p?.ExpirationDate ?? DateTime.Today;
        }

        private async void OnActualizarInventarioClicked(object sender, EventArgs e)
        {
            // valida id
            if (!_idProducto.HasValue)
            {
                await DisplayAlert("Dato faltante",
                    "No se encontró el id del producto. Regresa al inventario y vuelve a abrir el detalle.",
                    "OK");
                return;
            }

            // valida cantidad
            if (!decimal.TryParse(txtCantidad.Text?.Trim(), out var cant))
            {
                await DisplayAlert("Validación", "Cantidad inválida.", "OK");
                return;
            }

            DateTime? expira = dpExpira.Date;

            try
            {
                var svc = new ProductService();
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(20));

                // IMPORTANTE: ActualizarInventarioAsync devuelve un objeto de respuesta (NO bool)
                var r = await svc.ActualizarInventarioAsync(
                    _idProducto.Value,  // idProducto
                    cant,               // cantidad
                    expira,             // fechaExpiracion
                    cts.Token);         // ct

                var ok = r?.resultado == true;

                if (ok)
                {
                    await DisplayAlert("Listo", "Inventario actualizado.", "OK");
                    await Navigation.PushAsync(new DashboardPage());
                }
                else
                {
                    // Si tienes ProductService.FirstError, úsalo; si no, arma un mensaje básico.
                    string msg;
                    try
                    {
                        msg = ProductService.FirstError(r, "No se pudo actualizar el inventario.");
                    }
                    catch
                    {
                        msg = r?.mensaje ?? "No se pudo actualizar el inventario.";
                    }

                    await DisplayAlert("Error", msg, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
