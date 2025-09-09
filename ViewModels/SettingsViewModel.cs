using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Global_Insights_Dashboard.Services.Interfaces;
using Global_Insights_Dashboard.Utils.Validation;

namespace Global_Insights_Dashboard.ViewModels;

/// <summary>
/// ViewModel for the Settings dialog
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;
    private readonly IConfigurationService _configurationService;
    private readonly ICacheService _cacheService;

    public event EventHandler? SaveRequested;

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private string _selectedPrimaryColor = "DeepPurple";

    [ObservableProperty]
    private string _weatherApiKey = string.Empty;

    [ObservableProperty]
    private string _newsApiKey = string.Empty;

    [ObservableProperty]
    private string _financeApiKey = string.Empty;

    [ObservableProperty]
    private string _defaultService = "Weather";

    [ObservableProperty]
    private string _defaultCity = "Wichita, US";

    [ObservableProperty]
    private bool _isAutoRefreshEnabled = true;

    [ObservableProperty]
    private int _refreshIntervalMinutes = 15;

    [ObservableProperty]
    private bool _isOfflineModeEnabled = true;

    public override string ServiceName => "Settings";

    public ObservableCollection<KeyValuePair<string, string>> AvailableColors { get; } = new()
    {
        new("DeepPurple", "Deep Purple"),
        new("Blue", "Blue"),
        new("Indigo", "Indigo"),
        new("Teal", "Teal"),
        new("Green", "Green"),
        new("Orange", "Orange"),
        new("Red", "Red"),
        new("Pink", "Pink")
    };

    public ObservableCollection<string> AvailableServices { get; } = new()
    {
        "Weather", "News", "Finance", "Trivia"
    };

    public SettingsViewModel(
        IThemeService themeService,
        IConfigurationService configurationService,
        ICacheService cacheService)
    {
        _themeService = themeService;
        _configurationService = configurationService;
        _cacheService = cacheService;

        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        // Load theme settings
        IsDarkMode = _themeService.IsDarkMode;
        SelectedPrimaryColor = _configurationService.AppSettings.Theme.PrimaryColor;

        // Load API keys (will be masked in the UI)
        WeatherApiKey = _configurationService.ApiConfig.Weather.ApiKey ?? string.Empty;
        NewsApiKey = _configurationService.ApiConfig.News.ApiKey ?? string.Empty;
        FinanceApiKey = _configurationService.ApiConfig.Finance.ApiKey ?? string.Empty;

        // Load preferences
        var preferences = _configurationService.AppSettings.Preferences;
        DefaultService = preferences.LastActiveService;
        DefaultCity = $"{preferences.LastWeatherCity}, {preferences.LastWeatherCountry}";
        IsAutoRefreshEnabled = _configurationService.AppSettings.Refresh.AutoRefreshEnabled;
        RefreshIntervalMinutes = _configurationService.AppSettings.Refresh.RefreshIntervalMinutes;
        IsOfflineModeEnabled = _configurationService.AppSettings.Cache.EnableOfflineMode;
    }

    [RelayCommand]
    private async Task SaveWeatherApiKey(PasswordBox passwordBox)
    {
        if (passwordBox?.Password != null)
        {
            var validation = SettingsValidators.RequiredValidator("Weather API Key").Validate(passwordBox.Password);
            if (!validation.IsValid)
            {
                HandleError(validation.ErrorMessage);
                return;
            }

            await SaveApiKey("Weather", passwordBox.Password);
            WeatherApiKey = passwordBox.Password;
            StatusMessage = "Weather API key saved successfully";
        }
    }

    [RelayCommand]
    private async Task SaveNewsApiKey(PasswordBox passwordBox)
    {
        if (passwordBox?.Password != null)
        {
            var validation = SettingsValidators.RequiredValidator("News API Key").Validate(passwordBox.Password);
            if (!validation.IsValid)
            {
                HandleError(validation.ErrorMessage);
                return;
            }

            await SaveApiKey("News", passwordBox.Password);
            NewsApiKey = passwordBox.Password;
            StatusMessage = "News API key saved successfully";
        }
    }

    [RelayCommand]
    private async Task SaveFinanceApiKey(PasswordBox passwordBox)
    {
        if (passwordBox?.Password != null)
        {
            var validation = SettingsValidators.RequiredValidator("Finance API Key").Validate(passwordBox.Password);
            if (!validation.IsValid)
            {
                HandleError(validation.ErrorMessage);
                return;
            }

            await SaveApiKey("Finance", passwordBox.Password);
            FinanceApiKey = passwordBox.Password;
            StatusMessage = "Finance API key saved successfully";
        }
    }

    private async Task SaveApiKey(string service, string apiKey)
    {
        try
        {
            // Update the configuration
            switch (service)
            {
                case "Weather":
                    _configurationService.UpdateApiKey("Weather", apiKey);
                    break;
                case "News":
                    _configurationService.UpdateApiKey("News", apiKey);
                    break;
                case "Finance":
                    _configurationService.UpdateApiKey("Finance", apiKey);
                    break;
            }

            await Task.CompletedTask; // API key saving is synchronous
        }
        catch (Exception ex)
        {
            HandleError($"Failed to save {service} API key", ex);
        }
    }

    [RelayCommand]
    private async Task ClearCache()
    {
        try
        {
            await _cacheService.ClearAsync();
            StatusMessage = "Cache cleared successfully";
        }
        catch (Exception ex)
        {
            HandleError("Failed to clear cache", ex);
        }
    }

    [RelayCommand]
    private async Task SaveAllSettings()
    {
        try
        {
            SaveRequested?.Invoke(this, EventArgs.Empty);

            // Save theme settings
            if (IsDarkMode != _themeService.IsDarkMode)
            {
                _themeService.ToggleTheme();
            }

            // Save all settings
            _configurationService.UpdateSettings(settings =>
            {
                settings.Theme.PrimaryColor = SelectedPrimaryColor;
                settings.Preferences.LastActiveService = DefaultService;
                
                // Parse default city
                var cityParts = DefaultCity.Split(',');
                if (cityParts.Length >= 2)
                {
                    settings.Preferences.LastWeatherCity = cityParts[0].Trim();
                    settings.Preferences.LastWeatherCountry = cityParts[1].Trim();
                }
                else
                {
                    settings.Preferences.LastWeatherCity = DefaultCity.Trim();
                    settings.Preferences.LastWeatherCountry = "US";
                }

                settings.Refresh.AutoRefreshEnabled = IsAutoRefreshEnabled;
                settings.Refresh.RefreshIntervalMinutes = RefreshIntervalMinutes;
                settings.Cache.EnableOfflineMode = IsOfflineModeEnabled;
            });

            StatusMessage = "All settings saved successfully";
            
            // Close dialog after a brief delay
            await Task.Delay(1500);
            CloseDialog();
        }
        catch (Exception ex)
        {
            HandleError("Failed to save settings", ex);
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset all settings to their default values? This action cannot be undone.",
            "Reset Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Reset to default values
            IsDarkMode = false;
            SelectedPrimaryColor = "DeepPurple";
            DefaultService = "Weather";
            DefaultCity = "Wichita, US";
            IsAutoRefreshEnabled = true;
            RefreshIntervalMinutes = 15;
            IsOfflineModeEnabled = true;

            StatusMessage = "Settings reset to defaults";
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseDialog();
    }

    private void CloseDialog()
    {
        // Find the parent window and close it
        if (Application.Current.MainWindow?.OwnedWindows?.Count > 0)
        {
            var settingsWindow = Application.Current.MainWindow.OwnedWindows
                .OfType<Views.Settings.SettingsView>()
                .FirstOrDefault();
            
            settingsWindow?.Close();
        }
    }
}

/// <summary>
/// Extension class for additional validators
/// </summary>
public static class SettingsValidators
{
    public static InputValidator RequiredValidator(string fieldName) => new InputValidator()
        .AddRule(new RequiredValidationRule(fieldName));
}
