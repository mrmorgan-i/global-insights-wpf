using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;
using Global_Insights_Dashboard.Utils.Validation;

namespace Global_Insights_Dashboard.ViewModels;

/// <summary>
/// ViewModel for the Weather service
/// </summary>
public partial class WeatherViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private readonly ICacheService _cacheService;
    private readonly IConfigurationService _configurationService;

    [ObservableProperty]
    private string _cityInput = string.Empty;

    [ObservableProperty]
    private string _countryInput = string.Empty;

    [ObservableProperty]
    private CurrentWeatherResponse? _currentWeather;

    [ObservableProperty]
    private WeatherForecastResponse? _forecast;

    [ObservableProperty]
    private ObservableCollection<DailyForecast> _dailyForecasts = new();

    [ObservableProperty]
    private bool _hasWeatherData = false;

    [ObservableProperty]
    private string _searchPlaceholder = "Enter city name (e.g., London, New York)";

    public override string ServiceName => "Weather";

    public WeatherViewModel(
        IWeatherService weatherService,
        ICacheService cacheService,
        IConfigurationService configurationService)
    {
        _weatherService = weatherService;
        _cacheService = cacheService;
        _configurationService = configurationService;
    }

    protected override async Task OnInitializeAsync()
    {
        // Load last searched city from settings
        var preferences = _configurationService.AppSettings.Preferences;
        CityInput = preferences.LastWeatherCity;
        CountryInput = preferences.LastWeatherCountry;

        // Check API configuration
        var validation = _configurationService.ValidateApiConfiguration();
        if (!validation.IsValid)
        {
            StatusMessage = $"Configuration error: {validation.ErrorMessage}";
        }

        // Load weather data for the saved city
        if (!string.IsNullOrEmpty(CityInput))
        {
            await GetWeatherDataAsync();
        }
    }

    [RelayCommand]
    private async Task SearchWeather()
    {
        // Validate city name
        var cityValidation = CommonValidators.CityName.Validate(CityInput);
        if (!cityValidation.IsValid)
        {
            HandleError(cityValidation.ErrorMessage);
            return;
        }

        // Validate country code if provided
        if (!string.IsNullOrWhiteSpace(CountryInput))
        {
            var countryValidation = CommonValidators.CountryCode.Validate(CountryInput);
            if (!countryValidation.IsValid)
            {
                HandleError(countryValidation.ErrorMessage);
                return;
            }
        }

        await GetWeatherDataAsync();
    }

    [RelayCommand]
    private async Task SearchWeatherByLocation(string location)
    {
        var parts = location.Split(',');
        CityInput = parts[0].Trim();
        CountryInput = parts.Length > 1 ? parts[1].Trim() : string.Empty;
        
        await GetWeatherDataAsync();
    }

    protected override async Task OnRefreshAsync()
    {
        if (!string.IsNullOrEmpty(CityInput))
        {
            await GetWeatherDataAsync(forceRefresh: true);
        }
    }

    private async Task GetWeatherDataAsync(bool forceRefresh = false)
    {
        try
        {
            ShowLoading("Getting weather data...");

            var cacheKey = $"weather_{CityInput}_{CountryInput}".ToLowerInvariant();
            
            // Try cache first unless forcing refresh
            if (!forceRefresh)
            {
                var cachedWeather = await _cacheService.GetAsync<CurrentWeatherResponse>(cacheKey + "_current");
                var cachedForecast = await _cacheService.GetAsync<WeatherForecastResponse>(cacheKey + "_forecast");
                
                if (cachedWeather != null && cachedForecast != null)
                {
                    UpdateWeatherData(cachedWeather, cachedForecast);
                    StatusMessage = "Weather data loaded from cache";
                    return;
                }
            }

            // Get current weather and forecast
            var currentTask = _weatherService.GetCurrentWeatherAsync(CityInput, CountryInput);
            var forecastTask = _weatherService.GetForecastAsync(CityInput, CountryInput);

            await Task.WhenAll(currentTask, forecastTask);

            var currentWeather = await currentTask;
            var forecast = await forecastTask;

            if (currentWeather == null)
            {
                HandleError("Unable to get weather data. Please check the city name and try again.");
                return;
            }

            if (forecast == null)
            {
                HandleError("Unable to get forecast data. Please check the city name and try again.");
                return;
            }

            // Cache the results
            await _cacheService.SetAsync(cacheKey + "_current", currentWeather, TimeSpan.FromMinutes(30));
            await _cacheService.SetAsync(cacheKey + "_forecast", forecast, TimeSpan.FromMinutes(30));

            // Update UI
            UpdateWeatherData(currentWeather, forecast);

            // Save search to preferences
            _configurationService.UpdateSettings(settings =>
            {
                settings.Preferences.LastWeatherCity = CityInput;
                settings.Preferences.LastWeatherCountry = CountryInput;
            });

            StatusMessage = $"Weather updated for {currentWeather.Name}";
        }
        catch (Exception ex)
        {
            HandleError("Failed to get weather data", ex);
            
            // Try to load from cache as fallback
            var cacheKey = $"weather_{CityInput}_{CountryInput}".ToLowerInvariant();
            var cachedWeather = await _cacheService.GetAsync<CurrentWeatherResponse>(cacheKey + "_current");
            var cachedForecast = await _cacheService.GetAsync<WeatherForecastResponse>(cacheKey + "_forecast");
            
            if (cachedWeather != null && cachedForecast != null)
            {
                UpdateWeatherData(cachedWeather, cachedForecast);
                StatusMessage = "Showing cached weather data (offline mode)";
            }
        }
        finally
        {
            HideLoading();
        }
    }

    private void UpdateWeatherData(CurrentWeatherResponse current, WeatherForecastResponse forecast)
    {
        CurrentWeather = current;
        Forecast = forecast;
        
        // Update daily forecasts
        DailyForecasts.Clear();
        foreach (var daily in forecast.DailyForecast)
        {
            DailyForecasts.Add(daily);
        }

        HasWeatherData = true;
        LastUpdated = DateTime.Now;
    }

    [RelayCommand]
    private async Task UseCurrentLocation()
    {
        // For demo purposes, use a default location
        // In a real app, you'd use geolocation services
        CityInput = "London";
        CountryInput = "GB";
        await GetWeatherDataAsync();
    }

    [RelayCommand]
    private async Task SearchPopularCity(string cityCountry)
    {
        await SearchWeatherByLocation(cityCountry);
    }

    [RelayCommand]
    private void ClearSearch()
    {
        CityInput = string.Empty;
        CountryInput = string.Empty;
        CurrentWeather = null;
        Forecast = null;
        DailyForecasts.Clear();
        HasWeatherData = false;
        StatusMessage = "Ready";
    }

    // Popular cities for quick access
    public List<string> PopularCities => new()
    {
        "Wichita, US",
        "New York, US",
        "London, GB",
        "Tokyo, JP",
        "Paris, FR",
        "Sydney, AU",
        "Toronto, CA",
        "Berlin, DE"
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up resources if needed
        }
        base.Dispose(disposing);
    }
}
