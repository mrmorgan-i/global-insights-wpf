using System.Windows.Controls;
using Global_Insights_Dashboard.ViewModels;

namespace Global_Insights_Dashboard.Views.Trivia;

/// <summary>
/// Interaction logic for TriviaView.xaml
/// </summary>
public partial class TriviaView : UserControl
{
    public TriviaView(TriviaViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
