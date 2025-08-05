using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class HistorialRecetasPage : ContentPage
    {
        public HistorialRecetasPage()
        {
            InitializeComponent();
        }

        private async void OnRepetirRecetaClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Repetir receta", "Has preparado nuevamente la receta.", "OK");
        }
    }
}
