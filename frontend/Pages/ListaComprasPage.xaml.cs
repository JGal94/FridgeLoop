using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class ListaComprasPage : ContentPage
    {
        public ListaComprasPage()
        {
            InitializeComponent();
        }

        private async void OnEditarClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Editar", "Edición manual activada (simulado)", "OK");
        }

        private async void OnExportarClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Exportar", "Lista exportada o enviada a impresión (simulado)", "OK");
        }

        private async void OnSincronizarClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Sincronización", "Funcionalidad futura de integración con apps externas", "OK");
        }

        private async void OnVerHistorialClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistorialComprasPage());
        }
    }
}
