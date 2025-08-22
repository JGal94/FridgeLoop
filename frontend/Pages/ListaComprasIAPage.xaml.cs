using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Maui.Controls;
using Frontend_Proyecto_Fridgeloop.Services;
using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class ListaComprasIAPage : ContentPage
    {
        private readonly CompraIAService _svc = new();
        private CancellationTokenSource? _cts;
        private List<CompraIAService.ProductoRecomendadoDto> _ultimoResultado = new();

        public ListaComprasIAPage()
        {
            InitializeComponent();
            // mostrar lo que ya hubiera en memoria (si hubiera)
            RefrescarVistaPrev();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try { _cts?.Cancel(); } catch { }
        }

        private async void OnGenerarClicked(object sender, EventArgs e)
        {
            if (ai.IsRunning) return;
            ai.IsVisible = ai.IsRunning = true;
            _cts?.Cancel();
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));

            try
            {
                var res = await _svc.ObtenerPrediccionAsync(_cts.Token);
                if (res?.resultado == true && res.productos != null && res.productos.Any())
                {
                    _ultimoResultado = res.productos;

                    // Convertimos a texto con viñetas para la vista previa
                    var lineas = _ultimoResultado.Select(p =>
                    {
                        var nombre = string.IsNullOrWhiteSpace(p.nombre) ? "Producto" : p.nombre!.Trim();
                        var unidad = string.IsNullOrWhiteSpace(p.unidad) ? "unid" : p.unidad!.Trim();
                        var qty = p.cantidadRecomendada;
                        var qtyTxt = qty % 1 == 0 ? ((int)qty).ToString() : qty.ToString("0.##");
                        return $"• {nombre} ({qtyTxt} {unidad})";
                    });

                    txtLista.Text = string.Join(Environment.NewLine, lineas);
                }
                else
                {
                    var msg = CompraIAService.FirstError(res, "No se obtuvieron sugerencias.");
                    await DisplayAlert("IA", msg, "OK");
                }
            }
            catch (TaskCanceledException) { /* silencio */ }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                ai.IsVisible = ai.IsRunning = false;
            }
        }

        private async void OnAgregarALaListaClicked(object sender, EventArgs e)
        {
            if (_ultimoResultado == null || !_ultimoResultado.Any())
            {
                await DisplayAlert("Lista", "No hay sugerencias para agregar. Genera primero con IA.", "OK");
                return;
            }

            if (sender is Button btn) btn.IsEnabled = false;
            try
            {
                foreach (var s in _ultimoResultado)
                {
                    var idCat = CategoryMapper.Map(s.nombre);
                    // ? mapeo seguro (1..46; default 46)

                    ShoppingList.Items.Add(new ShoppingItem
                    {
                        Nombre = s.nombre?.Trim() ?? "Producto",
                        IdCategoria = idCat,
                        Unidad = string.IsNullOrWhiteSpace(s.unidad) ? "unid" : s.unidad!.Trim(),
                        Cantidad = s.cantidadRecomendada <= 0 ? 1 : s.cantidadRecomendada,
                        PrecioUnitario = null,
                        FechaExpiracion = null
                    });
                }

                await NavigateToListaComprasAsync(); // como ya lo dejaste
            }
            finally
            {
                if (sender is Button btn2) btn2.IsEnabled = true;
            }
        }


        private async Task NavigateToListaComprasAsync()
        {
            // Si ya existe una ListaComprasPage en la pila, volver a ella
            var stack = Navigation?.NavigationStack;
            if (stack != null)
            {
                var target = stack.FirstOrDefault(p => p is ListaComprasPage);
                if (target != null)
                {
                    while (Navigation.NavigationStack.Last() != target)
                        await Navigation.PopAsync(animated: false);
                    return; // OnAppearing de ListaComprasPage recalcula el total
                }
            }

            // Si no existe, crea una nueva instancia
            await Navigation.PushAsync(new ListaComprasPage());
        }


        private void RefrescarVistaPrev()
        {
            // Muestra la lista global de sesión en formato con viñetas
            txtLista.Text = ShoppingList.AsBulletedText();
        }
    }
}
