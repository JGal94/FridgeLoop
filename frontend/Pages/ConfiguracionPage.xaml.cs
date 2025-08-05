using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class ConfiguracionPage : ContentPage
    {
        public ConfiguracionPage()
        {
            InitializeComponent();
        }

        private async void OnAvanzadoClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Ajustes avanzados", "Esta función estará disponible próximamente.", "OK");
        }
    }
}
