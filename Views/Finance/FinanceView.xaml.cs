using System.Windows.Controls;
using Global_Insights_Dashboard.ViewModels;

namespace Global_Insights_Dashboard.Views.Finance;

/// <summary>
/// Interaction logic for FinanceView.xaml
/// </summary>
public partial class FinanceView : UserControl
{
    public FinanceView(FinanceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
