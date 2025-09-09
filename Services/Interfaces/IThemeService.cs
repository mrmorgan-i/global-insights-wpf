using Global_Insights_Dashboard.Models.Configuration;

namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for managing application themes and appearance
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Event raised when theme changes
    /// </summary>
    event EventHandler<AppThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Get current theme settings
    /// </summary>
    ThemeSettings CurrentTheme { get; }

    /// <summary>
    /// Check if dark mode is currently active
    /// </summary>
    bool IsDarkMode { get; }

    /// <summary>
    /// Toggle between light and dark themes
    /// </summary>
    void ToggleTheme();

    /// <summary>
    /// Set theme to dark mode
    /// </summary>
    void SetDarkTheme();

    /// <summary>
    /// Set theme to light mode
    /// </summary>
    void SetLightTheme();

    /// <summary>
    /// Apply theme settings
    /// </summary>
    /// <param name="themeSettings">Theme settings to apply</param>
    void ApplyTheme(ThemeSettings themeSettings);

    /// <summary>
    /// Get available primary colors
    /// </summary>
    /// <returns>List of available primary colors</returns>
    List<ThemeColor> GetAvailablePrimaryColors();

    /// <summary>
    /// Get available secondary colors
    /// </summary>
    /// <returns>List of available secondary colors</returns>
    List<ThemeColor> GetAvailableSecondaryColors();

    /// <summary>
    /// Set primary color
    /// </summary>
    /// <param name="colorName">Primary color name</param>
    void SetPrimaryColor(string colorName);

    /// <summary>
    /// Set secondary color
    /// </summary>
    /// <param name="colorName">Secondary color name</param>
    void SetSecondaryColor(string colorName);

    /// <summary>
    /// Reset theme to default settings
    /// </summary>
    void ResetToDefault();

    /// <summary>
    /// Save current theme settings
    /// </summary>
    Task SaveThemeAsync();

    /// <summary>
    /// Load theme settings
    /// </summary>
    Task LoadThemeAsync();
}

/// <summary>
/// Theme changed event arguments
/// </summary>
public class AppThemeChangedEventArgs : EventArgs
{
    public ThemeSettings OldTheme { get; set; } = new();
    public ThemeSettings NewTheme { get; set; } = new();
    public DateTime ChangedTime { get; set; } = DateTime.Now;
}

/// <summary>
/// Theme color information
/// </summary>
public class ThemeColor
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string HexValue { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
