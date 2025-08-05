using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class AgregarProductoPage : ContentPage
    {
        public AgregarProductoPage()
        {
            InitializeComponent();
        }

        
        private async void OnEscanearClicked(object sender, EventArgs e)
        {
            // Escaneo aún no implementado
            await DisplayAlert("Escaneo", "Funcionalidad en desarrollo", "OK");
        }
         
        private async void OnAgregarClicked(object sender, EventArgs e)
        {
            // Simulación de confirmación
            await DisplayAlert("Producto agregado", "Tu producto fue añadido al inventario", "OK");
        }
        
    }
}
