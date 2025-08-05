using Frontend_Proyecto_Fridgeloop.Pages;

namespace Frontend_Proyecto_Fridgeloop
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new StartPage())
            {
                BarBackgroundColor = Color.FromArgb("#EAF4FF"), 
                BarTextColor = Color.FromArgb("#003366")         
            };

        }
    }
}
