using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Global_Insights_Dashboard.Services.Interfaces;
using Global_Insights_Dashboard.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Global_Insights_Dashboard.ViewModels;

/// <summary>
/// Main view model for the application window and navigation
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IThemeService _themeService;
    private readonly IConfigurationService _configurationService;

    [ObservableProperty]
    private string _currentServiceName = "Weather";

    [ObservableProperty]
    private string _applicationTitle = "Global Insights Dashboard";

    [ObservableProperty]
    private bool _isNavigationOpen = false;

    [ObservableProperty]
    private ObservableCollection<ServiceMenuItem> _services = new();

    [ObservableProperty]
    private string _statusBarText = "Ready";

    [ObservableProperty]
    private bool _isDarkMode = false;

    [ObservableProperty]
    private string _currentTime = string.Empty;

    public override string ServiceName => "Main";

    public MainViewModel(
        INavigationService navigationService,
        IThemeService themeService,
        IConfigurationService configurationService)
    {
        _navigationService = navigationService;
        _themeService = themeService;
        _configurationService = configurationService;

        // Subscribe to navigation events
        _navigationService.NavigationChanged += OnNavigationChanged;
        
        // Subscribe to theme events
        _themeService.ThemeChanged += OnThemeChanged;

        // Initialize properties
        IsDarkMode = _themeService.IsDarkMode;
        
        // Initialize services
        InitializeServices();
        
        // Start time update
        StartTimeUpdate();
    }

    protected override async Task OnInitializeAsync()
    {
        // Load theme and navigate to last used service
        await _themeService.LoadThemeAsync();
        
        var lastService = _configurationService.AppSettings.Preferences.LastActiveService;
        if (_navigationService.IsServiceAvailable(lastService))
        {
            _navigationService.NavigateTo(lastService);
        }
        else
        {
            _navigationService.NavigateTo("Weather");
        }
    }

    [RelayCommand]
    private void NavigateToService(string serviceName)
    {
        if (!string.IsNullOrEmpty(serviceName) && _navigationService.IsServiceAvailable(serviceName))
        {
            _navigationService.NavigateTo(serviceName);
            IsNavigationOpen = false;
        }
    }

    [RelayCommand]
    private void ToggleNavigation()
    {
        IsNavigationOpen = !IsNavigationOpen;
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
    }

    [RelayCommand]
    private void GoHome()
    {
        _navigationService.NavigateTo("Weather");
        IsNavigationOpen = false;
    }

    [RelayCommand]
    private async Task RefreshCurrentService()
    {
        StatusBarText = "Refreshing...";
        
        try
        {
            // Trigger refresh on current service
            await RefreshAsync();
            StatusBarText = $"Last refreshed: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusBarText = $"Refresh failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        try
        {
            // Get settings view model from DI container
            var settingsViewModel = App.ServiceProvider?.GetService<SettingsViewModel>() ?? 
                throw new InvalidOperationException("Settings view model not available");
            
            var settingsWindow = new Views.Settings.SettingsView(settingsViewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            
            settingsWindow.ShowDialog();
            StatusBarText = "Settings dialog closed";
        }
        catch (Exception ex)
        {
            StatusBarText = $"Failed to open settings: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowAbout()
    {
        // TODO: Implement about dialog
        StatusBarText = "Global Insights Dashboard v1.0 - A comprehensive information hub";
    }

    [RelayCommand]
    private void ExitApplication()
    {
        // Save current service before exit
        _configurationService.UpdateSettings(settings => 
            settings.Preferences.LastActiveService = CurrentServiceName);
        
        System.Windows.Application.Current.Shutdown();
    }

    private void InitializeServices()
    {
        Services.Clear();
        
        Services.Add(new ServiceMenuItem
        {
            Name = "Weather",
            DisplayName = "Weather",
            Icon = "🌤️",
            Description = "Current weather and 5-day forecast",
            IsActive = true
        });

        Services.Add(new ServiceMenuItem
        {
            Name = "News",
            DisplayName = "News",
            Icon = "📰",
            Description = "Latest headlines and news articles",
            IsActive = false
        });

        Services.Add(new ServiceMenuItem
        {
            Name = "Finance",
            DisplayName = "Finance",
            Icon = "📈",
            Description = "Stock quotes and market data",
            IsActive = false
        });

        Services.Add(new ServiceMenuItem
        {
            Name = "Trivia",
            DisplayName = "Trivia",
            Icon = "🧠",
            Description = "Interactive knowledge quizzes",
            IsActive = false
        });
    }

    private void OnNavigationChanged(object? sender, NavigationEventArgs e)
    {
        CurrentServiceName = e.ToService;
        
        // Update active service in menu
        foreach (var service in Services)
        {
            service.IsActive = service.Name == e.ToService;
        }

        StatusBarText = $"Navigated to {e.ToService}";
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        IsDarkMode = e.NewTheme.IsDarkMode;
        StatusBarText = $"Theme changed to {(IsDarkMode ? "Dark" : "Light")} mode";
    }

    private void StartTimeUpdate()
    {
        var timer = new System.Timers.Timer(1000); // Update every second
        timer.Elapsed += (_, _) =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            });
        };
        timer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _navigationService.NavigationChanged -= OnNavigationChanged;
            _themeService.ThemeChanged -= OnThemeChanged;
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Service menu item for navigation
/// </summary>
public partial class ServiceMenuItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _isActive = false;

    [ObservableProperty]
    private bool _isEnabled = true;
}
