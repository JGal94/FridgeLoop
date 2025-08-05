using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
        }
        
        private async void GoToInventario(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InventarioPage());
        }

        private async void GoToRecetas(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RecetasSugeridasPage());
        }

        private async void GoToCompras(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListaComprasPage());
        }

        private async void GoToGastos(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PresupuestoMensualPage());
        }
        private async void OnNotificacionesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NotificacionesPage());
        }
        private async void OnPerfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PerfilPage());
        }
        private async void OnConfiguracionClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConfiguracionPage());
        }

    }
}
