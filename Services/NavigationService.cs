using System.Collections.Generic;
using System.Windows.Controls;
using Global_Insights_Dashboard.Services.Interfaces;
using Global_Insights_Dashboard.ViewModels;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Implementation of navigation service for managing view navigation
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Dictionary<string, Func<UserControl>> _serviceFactories;
    private readonly List<string> _navigationHistory;
    private string _currentService = string.Empty;

    public event EventHandler<NavigationEventArgs>? NavigationChanged;

    public string CurrentService => _currentService;

    public NavigationService()
    {
        _serviceFactories = new Dictionary<string, Func<UserControl>>(StringComparer.OrdinalIgnoreCase);
        _navigationHistory = new List<string>();
        
        RegisterDefaultServices();
    }

    public void NavigateTo(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be empty", nameof(serviceName));

        if (!_serviceFactories.ContainsKey(serviceName))
            throw new ArgumentException($"Service '{serviceName}' is not registered", nameof(serviceName));

        var previousService = _currentService;
        
        // Create the view using the factory
        var view = _serviceFactories[serviceName]();
        
        // Update current service
        _currentService = serviceName;
        
        // Add to history if it's different from current
        if (!string.Equals(previousService, serviceName, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(previousService))
            {
                _navigationHistory.Add(previousService);
            }
        }

        // Raise navigation event
        NavigationChanged?.Invoke(this, new NavigationEventArgs
        {
            FromService = previousService,
            ToService = serviceName,
            View = view
        });
    }

    public void NavigateTo(UserControl view, string serviceName)
    {
        if (view == null)
            throw new ArgumentNullException(nameof(view));
        
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be empty", nameof(serviceName));

        var previousService = _currentService;
        _currentService = serviceName;

        // Add to history if it's different from current
        if (!string.Equals(previousService, serviceName, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(previousService))
            {
                _navigationHistory.Add(previousService);
            }
        }

        // Raise navigation event
        NavigationChanged?.Invoke(this, new NavigationEventArgs
        {
            FromService = previousService,
            ToService = serviceName,
            View = view
        });
    }

    public List<string> GetAvailableServices()
    {
        return _serviceFactories.Keys.ToList();
    }

    public bool IsServiceAvailable(string serviceName)
    {
        return !string.IsNullOrWhiteSpace(serviceName) && 
               _serviceFactories.ContainsKey(serviceName);
    }

    public bool GoBack()
    {
        if (_navigationHistory.Count == 0)
            return false;

        var previousService = _navigationHistory.Last();
        _navigationHistory.RemoveAt(_navigationHistory.Count - 1);

        NavigateTo(previousService);
        return true;
    }

    public List<string> GetNavigationHistory()
    {
        return new List<string>(_navigationHistory);
    }

    public void ClearHistory()
    {
        _navigationHistory.Clear();
    }

    /// <summary>
    /// Register a service view factory
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="viewFactory">Factory function to create the view</param>
    public void RegisterService(string serviceName, Func<UserControl> viewFactory)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be empty", nameof(serviceName));
        
        if (viewFactory == null)
            throw new ArgumentNullException(nameof(viewFactory));

        _serviceFactories[serviceName] = viewFactory;
    }

    /// <summary>
    /// Unregister a service
    /// </summary>
    /// <param name="serviceName">Name of the service to unregister</param>
    /// <returns>True if service was removed</returns>
    public bool UnregisterService(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return false;

        return _serviceFactories.Remove(serviceName);
    }

    private void RegisterDefaultServices()
    {
        // Register service view factories
        // Note: These will be properly injected when the views are implemented
        _serviceFactories["Weather"] = () => 
        {
            var viewModel = App.GetService<WeatherViewModel>();
            var view = new Global_Insights_Dashboard.Views.Weather.WeatherView(viewModel!);
            return view;
        };
        
        _serviceFactories["News"] = () => 
        {
            var viewModel = App.GetService<NewsViewModel>();
            var view = new Global_Insights_Dashboard.Views.News.NewsView(viewModel!);
            return view;
        };
        
        _serviceFactories["Finance"] = () => 
        {
            var viewModel = App.GetService<FinanceViewModel>();
            var view = new Global_Insights_Dashboard.Views.Finance.FinanceView(viewModel!);
            return view;
        };
        
        _serviceFactories["Trivia"] = () => 
        {
            var viewModel = App.GetService<TriviaViewModel>();
            var view = new Global_Insights_Dashboard.Views.Trivia.TriviaView(viewModel!);
            return view;
        };
    }
}
