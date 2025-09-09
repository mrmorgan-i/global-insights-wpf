using System.Windows;
using System.Windows.Controls;
using Global_Insights_Dashboard.ViewModels;

namespace Global_Insights_Dashboard.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Set current API keys in password boxes
            SetPasswordBoxValues(viewModel);

            // Subscribe to save events to read password box values
            viewModel.SaveRequested += OnSaveRequested;
        }

        private void SetPasswordBoxValues(SettingsViewModel viewModel)
        {
            // Set masked API keys (show only last 4 characters)
            WeatherApiKeyBox.Password = MaskApiKey(viewModel.WeatherApiKey);
            NewsApiKeyBox.Password = MaskApiKey(viewModel.NewsApiKey);
            FinanceApiKeyBox.Password = MaskApiKey(viewModel.FinanceApiKey);
        }

        private string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 4)
                return apiKey;

            return new string('*', apiKey.Length - 4) + apiKey.Substring(apiKey.Length - 4);
        }

        private void OnSaveRequested(object? sender, System.EventArgs e)
        {
            var viewModel = DataContext as SettingsViewModel;
            if (viewModel != null)
            {
                // Get actual values from password boxes if they were changed
                if (!WeatherApiKeyBox.Password.Contains('*'))
                    viewModel.WeatherApiKey = WeatherApiKeyBox.Password;
                
                if (!NewsApiKeyBox.Password.Contains('*'))
                    viewModel.NewsApiKey = NewsApiKeyBox.Password;
                
                if (!FinanceApiKeyBox.Password.Contains('*'))
                    viewModel.FinanceApiKey = FinanceApiKeyBox.Password;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.SaveRequested -= OnSaveRequested;
            }
            base.OnClosed(e);
        }
    }
}
