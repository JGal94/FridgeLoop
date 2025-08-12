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

            MessagingCenter.Subscribe<ScanPage, string>(this, "BarcodeScanned", (sender2, code) =>
            {
                // Mostrar el código en un Entry, por ejemplo
                codigoEntry.Text = code;
                MessagingCenter.Unsubscribe<ScanPage, string>(this, "BarcodeScanned");
            });

            await Navigation.PushAsync(new ScanPage());
        }
         
        private async void OnAgregarClicked(object sender, EventArgs e)
        {
            // Simulación de confirmación
            await DisplayAlert("Producto agregado", "Tu producto fue añadido al inventario", "OK");
        }
        
    }
}
