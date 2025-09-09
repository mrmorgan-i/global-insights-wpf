using System.Windows.Controls;
using Global_Insights_Dashboard.ViewModels;

namespace Global_Insights_Dashboard.Views.News;

/// <summary>
/// Interaction logic for NewsView.xaml
/// </summary>
public partial class NewsView : UserControl
{
    public NewsView(NewsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
