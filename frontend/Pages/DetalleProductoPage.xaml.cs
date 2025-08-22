using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class DetalleProductoPage : ContentPage
    {
        private int? _idProducto;

        // Recibe el DTO y (opcionalmente) el id explícito para actualizar
        public DetalleProductoPage(ProductService.ProductoDto p, int? idProducto = null)
        {
            InitializeComponent();

            // 1) Resolver id (si viene explícito, lo usamos; si no, intentamos sacarlo del DTO)
            _idProducto = idProducto ?? TryGetIdFromDto(p);

            // 2) Nombre: prioriza Label lblNombre; si no existe, usa Entry txtNombre (si lo tienes)
            var lblNombre = this.FindByName<Label>("lblNombre");
            if (lblNombre != null)
            {
                lblNombre.Text = p?.Name ?? "(sin nombre)";
            }
            else
            {
                var entryNombre = this.FindByName<Entry>("txtNombre");
                if (entryNombre != null)
                    entryNombre.Text = p?.Name ?? "";
            }

            // 3) Cantidad (Entry txtCantidad)
            var txtCant = this.FindByName<Entry>("txtCantidad");
            if (txtCant != null)
            {
                var cantidad = p?.Quantity ?? 0m;
                txtCant.Text = cantidad > 0 ? cantidad.ToString("0.##") : "";
            }

            // 4) Fecha de expiración (DatePicker dpExpira)
            var dp = this.FindByName<DatePicker>("dpExpira");
            if (dp != null)
            {
                if (p?.ExpirationDate.HasValue == true)
                    dp.Date = p.ExpirationDate.Value.Date;
                else
                    dp.Date = DateTime.Today;
            }
        }

        // Click en "Actualizar inventario"
        private async void OnActualizarInventarioClicked(object sender, EventArgs e)
        {
            // Validar id
            if (_idProducto == null)
            {
                await DisplayAlert("Dato faltante",
                    "No se encontró el id del producto. Regresa al inventario y vuelve a abrir el detalle.",
                    "OK");
                return;
            }

            // Validar cantidad
            var txtCant = this.FindByName<Entry>("txtCantidad");
            if (txtCant == null || !decimal.TryParse(txtCant.Text?.Trim(), out var cant))
            {
                await DisplayAlert("Validación", "Cantidad inválida.", "OK");
                return;
            }

            // Tomar fecha
            DateTime? expira = this.FindByName<DatePicker>("dpExpira")?.Date;

            try
            {
                var svc = new ProductService();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

                // ProductService.ActualizarInventarioAsync retorna ResBase
                var res = await svc.ActualizarInventarioAsync(_idProducto.Value, cant, expira, cts.Token);

                if (res?.resultado == true)
                {
                    await DisplayAlert("Listo", "Inventario actualizado.", "OK");
                    await Navigation.PushAsync(new DashboardPage()); // o PopToRootAsync() si prefieres volver al Dashboard
                }
                else
                {
                    await DisplayAlert("Error", ErrorAmigable(res, "No se pudo actualizar el inventario."), "OK");
                }
            }
            catch (TaskCanceledException)
            {
                await DisplayAlert("Tiempo de espera", "La operación tardó demasiado.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // ==== Helpers ====

        // Intenta extraer un id desde propiedades comunes
        private static int? TryGetIdFromDto(ProductService.ProductoDto p)
        {
            try
            {
                var t = p?.GetType();
                foreach (var name in new[] { "IdProducto", "idProducto", "ProductId", "ProductID", "Id", "ID" })
                {
                    var prop = t?.GetProperty(name);
                    if (prop == null) continue;
                    var val = prop.GetValue(p);
                    if (val == null) continue;

                    if (val is int i) return i;
                    if (int.TryParse(val.ToString(), out var j)) return j;
                }
            }
            catch { /* ignorar */ }
            return null;
        }

        private static string ErrorAmigable(ProductService.ResBase? r, string fallback)
        {
            return r?.listaDeErrores?.FirstOrDefault()?.Message
                   ?? r?.mensaje
                   ?? fallback;
        }
    }
}
