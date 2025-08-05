using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class DetalleRecetaPage : ContentPage
    {
        public DetalleRecetaPage()
        {
            InitializeComponent();
        }

        private async void OnPrepararClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Preparar", "Ingredientes descontados del inventario", "OK");
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Receta Guardada", "Se agregó a tu historial", "OK");
        }
    }
}
