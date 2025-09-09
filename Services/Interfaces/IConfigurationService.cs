using Global_Insights_Dashboard.Models.Configuration;

namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for managing application configuration and settings
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Get API configuration
    /// </summary>
    ApiConfiguration ApiConfig { get; }

    /// <summary>
    /// Get application settings
    /// </summary>
    AppSettings AppSettings { get; }

    /// <summary>
    /// Save application settings to persistent storage
    /// </summary>
    Task SaveSettingsAsync();

    /// <summary>
    /// Load application settings from persistent storage
    /// </summary>
    Task LoadSettingsAsync();

    /// <summary>
    /// Update application settings
    /// </summary>
    void UpdateSettings(Action<AppSettings> updateAction);

    /// <summary>
    /// Get the configuration directory path
    /// </summary>
    string GetConfigurationDirectory();

    /// <summary>
    /// Check if API keys are configured
    /// </summary>
    bool AreApiKeysConfigured();

    /// <summary>
    /// Validate API configuration
    /// </summary>
    (bool IsValid, string ErrorMessage) ValidateApiConfiguration();

    /// <summary>
    /// Update API key for a specific service
    /// </summary>
    void UpdateApiKey(string service, string apiKey);
}
