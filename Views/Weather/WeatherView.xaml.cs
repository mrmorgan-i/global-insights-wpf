using System.Windows.Controls;
using Global_Insights_Dashboard.ViewModels;

namespace Global_Insights_Dashboard.Views.Weather
{
    /// <summary>
    /// Interaction logic for WeatherView.xaml
    /// </summary>
    public partial class WeatherView : UserControl
    {
        public WeatherView()
        {
            InitializeComponent();
        }

        public WeatherView(WeatherViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
