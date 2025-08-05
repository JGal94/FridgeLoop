using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        
           private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Simulación de navegación tras iniciar sesión

            await Navigation.PushAsync(new DashboardPage()); // página principal futura
        }
        
    }
}
