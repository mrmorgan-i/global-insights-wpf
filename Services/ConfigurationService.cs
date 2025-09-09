using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Global_Insights_Dashboard.Models.Configuration;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Implementation of configuration service for managing application settings
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly string _settingsFilePath;
    private readonly string _configDirectory;

    public ApiConfiguration ApiConfig { get; private set; }
    public AppSettings AppSettings { get; private set; }

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
        _configDirectory = GetConfigurationDirectory();
        _settingsFilePath = Path.Combine(_configDirectory, "settings.json");

        // Initialize configuration
        ApiConfig = new ApiConfiguration();
        
        // Load from Microsoft.Extensions.Configuration
        var configSection = _configuration.GetSection("ApiConfiguration");
        if (configSection.Exists())
        {
            var tempConfig = configSection.Get<ApiConfiguration>();
            if (tempConfig != null)
            {
                ApiConfig = tempConfig;
            }
        }
        
        // Load user settings
        AppSettings = new AppSettings();
        _ = LoadSettingsAsync();
    }

    public string GetConfigurationDirectory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appDataPath, "GlobalInsightsDashboard");
        
        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }
        
        return configDir;
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                // Create default settings file
                await SaveSettingsAsync();
                return;
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            var loadedSettings = JsonConvert.DeserializeObject<AppSettings>(json);
            
            if (loadedSettings != null)
            {
                AppSettings = loadedSettings;
            }
        }
        catch
        {
            // Log error and use default settings
            AppSettings = new AppSettings();
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            var json = JsonConvert.SerializeObject(AppSettings, Formatting.Indented);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch
        {
            // Nothing to do really
        }
    }

    public void UpdateSettings(Action<AppSettings> updateAction)
    {
        updateAction(AppSettings);
        _ = SaveSettingsAsync();
    }

    public bool AreApiKeysConfigured()
    {
        return !string.IsNullOrWhiteSpace(ApiConfig.Weather.ApiKey) &&
               !string.IsNullOrWhiteSpace(ApiConfig.News.ApiKey) &&
               !string.IsNullOrWhiteSpace(ApiConfig.Finance.ApiKey);
        // Trivia API doesn't require a key
    }

    public (bool IsValid, string ErrorMessage) ValidateApiConfiguration()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ApiConfig.Weather.ApiKey))
        {
            errors.Add("Weather API key is missing");
        }

        if (string.IsNullOrWhiteSpace(ApiConfig.News.ApiKey))
        {
            errors.Add("News API key is missing");
        }

        if (string.IsNullOrWhiteSpace(ApiConfig.Finance.ApiKey))
        {
            errors.Add("Finance API key is missing");
        }

        if (string.IsNullOrWhiteSpace(ApiConfig.Weather.BaseUrl))
        {
            errors.Add("Weather API base URL is missing");
        }

        if (string.IsNullOrWhiteSpace(ApiConfig.News.BaseUrl))
        {
            errors.Add("News API base URL is missing");
        }

        if (string.IsNullOrWhiteSpace(ApiConfig.Finance.BaseUrl))
        {
            errors.Add("Finance API base URL is missing");
        }

        if (string.IsNullOrWhiteSpace(ApiConfig.Trivia.BaseUrl))
        {
            errors.Add("Trivia API base URL is missing");
        }

        var isValid = errors.Count == 0;
        var errorMessage = isValid ? string.Empty : string.Join(", ", errors);

        return (isValid, errorMessage);
    }

    public void UpdateApiKey(string service, string apiKey)
    {
        switch (service.ToLowerInvariant())
        {
            case "weather":
                ApiConfig.Weather.ApiKey = apiKey;
                break;
            case "news":
                ApiConfig.News.ApiKey = apiKey;
                break;
            case "finance":
                ApiConfig.Finance.ApiKey = apiKey;
                break;
            default:
                throw new ArgumentException($"Unknown service: {service}", nameof(service));
        }

        // Save the updated configuration
        _ = SaveSettingsAsync();
    }

}
