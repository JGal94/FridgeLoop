using Microsoft.Maui.Controls;

namespace Frontend_Proyecto_Fridgeloop.Pages
{
    public partial class RecetasSugeridasPage : ContentPage
    {
        public RecetasSugeridasPage()
        {
            InitializeComponent();
        }

        private void OnFiltroTiempo(object sender, EventArgs e)
        {
            // Filtro por tiempo (simulado)
        }

        private void OnFiltroCalorias(object sender, EventArgs e)
        {
            // Filtro por calorías (simulado)
        }

        private void OnFiltroVegano(object sender, EventArgs e)
        {
            // Filtro por estilo vegano (simulado)
        }

        private void OnFiltroKeto(object sender, EventArgs e)
        {
            // Filtro por estilo keto (simulado)
        }

        private async void OnVerRecetaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DetalleRecetaPage());
        }

        private async void OnVerHistorialClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistorialRecetasPage());
        }
    }
}
