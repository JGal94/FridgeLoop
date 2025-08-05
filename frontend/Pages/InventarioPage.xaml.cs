using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class InventarioPage : ContentPage
    {
        public InventarioPage()
        {
            InitializeComponent();
        }

        private void OnFiltroVencerClicked(object sender, EventArgs e)
        {
            // Lógica futura
        }

        private void OnFiltroUsadosClicked(object sender, EventArgs e)
        {
            // Lógica futura
        }

        private async void OnAgregarProductoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AgregarProductoPage());
        }
    }
}
