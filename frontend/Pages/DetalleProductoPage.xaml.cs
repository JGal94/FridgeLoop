using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class DetalleProductoPage : ContentPage
    {
        public DetalleProductoPage()
        {
            InitializeComponent();
        }

        private async void OnEditarClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Editar", "Función de edición pendiente", "OK");
        }

        private async void OnEliminarClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Eliminar", "Producto eliminado (simulado)", "OK");
        }

        private async void OnMoverClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Mover", "Función de mover pendiente", "OK");
        }

        private async void OnConsumirClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Consumir", "Consumo parcial registrado", "OK");
        }
    }
}
