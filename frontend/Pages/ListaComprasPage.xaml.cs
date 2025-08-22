using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Helpers;
using Frontend_Proyecto_Fridgeloop.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class ListaComprasPage : ContentPage
    {

        // Exponemos la fuente que ya usas (si prefieres, puedes enlazar directo a ShoppingList.Items)
        public ObservableCollection<ShoppingItem> ItemsSource => ShoppingList.Items;

        private decimal _totalCompra;
        public decimal TotalCompra
        {
            get => _totalCompra;
            private set
            {
                if (_totalCompra != value)
                {
                    _totalCompra = value;
                    OnPropertyChanged(nameof(TotalCompra));
                }
            }
        }

        private readonly CompraService _compras = new();

        private static readonly HashSet<string> _porPieza = new(StringComparer.OrdinalIgnoreCase)
{ "pz", "unid", "pack" };

        public ListaComprasPage()
        {
            InitializeComponent();
            BindingContext = this;                  // ?? habilita {Binding TotalCompra}
            cvLista.ItemsSource = ShoppingList.Items;
            ItemsSource.CollectionChanged += Items_CollectionChanged;
            RecalcTotal();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Por si cambió algo al volver de otra página
            RecalcTotal();
        }

        private async void OnAgregarProductoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AgregarProductoPage());
        }

        private async void OnEditarItem(object sender, EventArgs e)
        {
            if ((sender as SwipeItem)?.CommandParameter is not ShoppingItem item) return;

            try
            {
                // Editar cantidad
                var qtyStr = await DisplayPromptAsync("Editar cantidad",
                    $"Cantidad para {item.Nombre} ({item.Unidad})",
                    initialValue: item.Cantidad.ToString(" "),
                    keyboard: Keyboard.Numeric);
                if (qtyStr == null) return; // cancelado

                if (!decimal.TryParse(qtyStr.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var nuevaCantidad) || nuevaCantidad <= 0)
                {
                    await DisplayAlert("Dato inválido", "Ingresa una cantidad válida mayor a 0.", "OK");
                    return;
                }

                // Editar precio (opcional)
                var precioStr = await DisplayPromptAsync("Editar precio (opcional)",
                    $"Precio unitario para {item.Nombre} ( )",
                    initialValue: item.PrecioUnitario?.ToString(" "),
                    keyboard: Keyboard.Numeric);

                decimal? nuevoPrecio = item.PrecioUnitario;
                if (precioStr != null && precioStr.Trim().Length > 0)
                {
                    if (decimal.TryParse(precioStr.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var p) && p >= 0)
                        nuevoPrecio = p;
                    else
                    {
                        await DisplayAlert("Dato inválido", "El precio debe ser un número mayor o igual a 0.", "OK");
                        return;
                    }
                }
                else
                {
                    nuevoPrecio = null; // limpiar
                }

                // Aplicar cambios
                item.Cantidad = nuevaCantidad;
                item.PrecioUnitario = nuevoPrecio;

                // Forzar refresco visual del item (si tu plantilla no reacciona automáticamente):
                var idx = ShoppingList.Items.IndexOf(item);
                if (idx >= 0)
                {
                    ShoppingList.Items.RemoveAt(idx);
                    ShoppingList.Items.Insert(idx, item);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
        }

        private async void OnEliminarItem(object sender, EventArgs e)
        {
            if ((sender as SwipeItem)?.CommandParameter is not ShoppingItem item) return;

            var confirmar = await DisplayAlert("Eliminar",
                $"¿Quitar \"{item.Nombre}\" de la lista?", "Sí", "No");
            if (!confirmar) return;

            ShoppingList.Items.Remove(item);
        }

        private async void OnRealizarCompraClicked(object sender, EventArgs e)
        {
            if (ShoppingList.Items.Count == 0)
            {
                await DisplayAlert("Lista vacía", "No hay productos para comprar.", "OK");
                return;
            }
            // Blindaje: asegurar IDs válidos 1..46 (si algo viejo quedó mal seteado)
            var validIds = new HashSet<int>(Enumerable.Range(1, 46));
            foreach (var it in ShoppingList.Items)
            {
                if (!validIds.Contains(it.IdCategoria))
                    it.IdCategoria = 46; // Otros
            }


            var confirmar = await DisplayAlert("Confirmar",
                "¿Deseas registrar la compra y mover los productos al inventario?",
                "Sí", "No");
            if (!confirmar) return;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                // POST /api/compra/registrar con la lista local
                var res = await _compras.RegistrarCompraAsync(DateTime.UtcNow, ShoppingList.Items, cts.Token);

                if (res?.resultado == true)
                {
                    ShoppingList.Clear(); // vaciar lista local
                    await DisplayAlert("Compra registrada",
                        $"{res.mensaje ?? "Productos agregados al inventario."} Total: {res.total:0.##}",
                        "OK");
                }
                else
                {
                    await DisplayAlert("Error",
                        CompraService.FirstError(res, "No se pudo registrar la compra."),
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
        }

        private async void OnVerHistorialClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await Navigation.PushAsync(new HistorialComprasPage());
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnSincronizarClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Próximamente", "Sincronización aún no implementada.", "OK");
        }
        

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RecalcTotal();
        }

        private void RecalcTotal()
        {
            decimal total = 0m;

            foreach (var i in ItemsSource)
            {
                var precio = i.PrecioUnitario ?? 0m;
                var cantidad = i.Cantidad > 0 ? i.Cantidad : 1m; // usa 1 si viene 0 o nulo
                total += precio * cantidad;
            }

            TotalCompra = total;

            
            lblTotal.Text = $"CRC {TotalCompra:N0}";
        }

    }
}
