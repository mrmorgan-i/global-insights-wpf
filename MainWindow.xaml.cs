using System.Windows;
using System.Windows.Controls;
using Global_Insights_Dashboard.ViewModels;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly INavigationService _navigationService;

        public MainWindow(MainViewModel viewModel, INavigationService navigationService)
        {
            InitializeComponent();
            
            _viewModel = viewModel;
            _navigationService = navigationService;
            
            DataContext = _viewModel;
            
            // Subscribe to navigation events
            _navigationService.NavigationChanged += OnNavigationChanged;
            
            // Initialize the view model
            Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize application: {ex.Message}", 
                              "Initialization Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private void OnNavigationChanged(object? sender, NavigationEventArgs e)
        {
            // Update the content area with the new view
            if (e.View != null)
            {
                ContentArea.Content = e.View;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events
            _navigationService.NavigationChanged -= OnNavigationChanged;
            
            // Dispose view model if it implements IDisposable
            if (_viewModel is IDisposable disposableViewModel)
            {
                disposableViewModel.Dispose();
            }
            
            base.OnClosed(e);
        }
    }
}