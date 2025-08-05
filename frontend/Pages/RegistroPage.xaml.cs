using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class RegistroPage : ContentPage
    {
        public RegistroPage()
        {
            InitializeComponent();
        }

        
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            // Simulación de redirección a pantalla de verificación

            await Navigation.PushAsync(new VerificacionPage()); 

        }
        
    }
}
