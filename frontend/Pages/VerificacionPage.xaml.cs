using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class VerificacionPage : ContentPage
    {
        public VerificacionPage()
        {
            InitializeComponent();
        }

        private async void OnActivateClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync(); // vuelve al menu inicial
        }
    }
}
