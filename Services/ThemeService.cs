using System.Diagnostics.CodeAnalysis;
using System.Windows;
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;
using Global_Insights_Dashboard.Models.Configuration;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Implementation of theme service using MaterialDesignInXamlToolkit
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This is a Windows-only WPF application")]
public class ThemeService : IThemeService
{
    private readonly IConfigurationService _configurationService;
    private ThemeSettings _currentTheme;

    public event EventHandler<AppThemeChangedEventArgs>? ThemeChanged;

    public ThemeSettings CurrentTheme => _currentTheme;
    public bool IsDarkMode => _currentTheme.IsDarkMode;

    public ThemeService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _currentTheme = _configurationService.AppSettings.Theme;
        
        // Initialize theme on startup
        _ = LoadThemeAsync();
    }

    public void ToggleTheme()
    {
        if (_currentTheme.IsDarkMode)
        {
            SetLightTheme();
        }
        else
        {
            SetDarkTheme();
        }
    }

    public void SetDarkTheme()
    {
        var oldTheme = new ThemeSettings
        {
            IsDarkMode = _currentTheme.IsDarkMode,
            PrimaryColor = _currentTheme.PrimaryColor,
            SecondaryColor = _currentTheme.SecondaryColor
        };

        _currentTheme.IsDarkMode = true;
        ApplyThemeToMaterialDesign();
        
        OnThemeChanged(oldTheme, _currentTheme);
        _ = SaveThemeAsync();
    }

    public void SetLightTheme()
    {
        var oldTheme = new ThemeSettings
        {
            IsDarkMode = _currentTheme.IsDarkMode,
            PrimaryColor = _currentTheme.PrimaryColor,
            SecondaryColor = _currentTheme.SecondaryColor
        };

        _currentTheme.IsDarkMode = false;
        ApplyThemeToMaterialDesign();
        
        OnThemeChanged(oldTheme, _currentTheme);
        _ = SaveThemeAsync();
    }

    public void ApplyTheme(ThemeSettings themeSettings)
    {
        var oldTheme = new ThemeSettings
        {
            IsDarkMode = _currentTheme.IsDarkMode,
            PrimaryColor = _currentTheme.PrimaryColor,
            SecondaryColor = _currentTheme.SecondaryColor
        };

        _currentTheme.IsDarkMode = themeSettings.IsDarkMode;
        _currentTheme.PrimaryColor = themeSettings.PrimaryColor;
        _currentTheme.SecondaryColor = themeSettings.SecondaryColor;

        ApplyThemeToMaterialDesign();
        
        OnThemeChanged(oldTheme, _currentTheme);
        _ = SaveThemeAsync();
    }

    public List<ThemeColor> GetAvailablePrimaryColors()
    {
        return new List<ThemeColor>
        {
            new() { Name = "Red", DisplayName = "Red", HexValue = "#F44336", IsDefault = false },
            new() { Name = "Pink", DisplayName = "Pink", HexValue = "#E91E63", IsDefault = false },
            new() { Name = "Purple", DisplayName = "Purple", HexValue = "#9C27B0", IsDefault = false },
            new() { Name = "DeepPurple", DisplayName = "Deep Purple", HexValue = "#673AB7", IsDefault = true },
            new() { Name = "Indigo", DisplayName = "Indigo", HexValue = "#3F51B5", IsDefault = false },
            new() { Name = "Blue", DisplayName = "Blue", HexValue = "#2196F3", IsDefault = false },
            new() { Name = "LightBlue", DisplayName = "Light Blue", HexValue = "#03A9F4", IsDefault = false },
            new() { Name = "Cyan", DisplayName = "Cyan", HexValue = "#00BCD4", IsDefault = false },
            new() { Name = "Teal", DisplayName = "Teal", HexValue = "#009688", IsDefault = false },
            new() { Name = "Green", DisplayName = "Green", HexValue = "#4CAF50", IsDefault = false },
            new() { Name = "LightGreen", DisplayName = "Light Green", HexValue = "#8BC34A", IsDefault = false },
            new() { Name = "Lime", DisplayName = "Lime", HexValue = "#CDDC39", IsDefault = false },
            new() { Name = "Yellow", DisplayName = "Yellow", HexValue = "#FFEB3B", IsDefault = false },
            new() { Name = "Amber", DisplayName = "Amber", HexValue = "#FFC107", IsDefault = false },
            new() { Name = "Orange", DisplayName = "Orange", HexValue = "#FF9800", IsDefault = false },
            new() { Name = "DeepOrange", DisplayName = "Deep Orange", HexValue = "#FF5722", IsDefault = false },
            new() { Name = "Brown", DisplayName = "Brown", HexValue = "#795548", IsDefault = false },
            new() { Name = "Grey", DisplayName = "Grey", HexValue = "#9E9E9E", IsDefault = false },
            new() { Name = "BlueGrey", DisplayName = "Blue Grey", HexValue = "#607D8B", IsDefault = false }
        };
    }

    public List<ThemeColor> GetAvailableSecondaryColors()
    {
        return GetAvailablePrimaryColors(); // Secondary colors are same as primary
    }

    public void SetPrimaryColor(string colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName))
            return;

        var oldTheme = new ThemeSettings
        {
            IsDarkMode = _currentTheme.IsDarkMode,
            PrimaryColor = _currentTheme.PrimaryColor,
            SecondaryColor = _currentTheme.SecondaryColor
        };

        _currentTheme.PrimaryColor = colorName;
        ApplyThemeToMaterialDesign();
        
        OnThemeChanged(oldTheme, _currentTheme);
        _ = SaveThemeAsync();
    }

    public void SetSecondaryColor(string colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName))
            return;

        var oldTheme = new ThemeSettings
        {
            IsDarkMode = _currentTheme.IsDarkMode,
            PrimaryColor = _currentTheme.PrimaryColor,
            SecondaryColor = _currentTheme.SecondaryColor
        };

        _currentTheme.SecondaryColor = colorName;
        ApplyThemeToMaterialDesign();
        
        OnThemeChanged(oldTheme, _currentTheme);
        _ = SaveThemeAsync();
    }

    public void ResetToDefault()
    {
        var oldTheme = new ThemeSettings
        {
            IsDarkMode = _currentTheme.IsDarkMode,
            PrimaryColor = _currentTheme.PrimaryColor,
            SecondaryColor = _currentTheme.SecondaryColor
        };

        _currentTheme.IsDarkMode = false;
        _currentTheme.PrimaryColor = "DeepPurple";
        _currentTheme.SecondaryColor = "Lime";

        ApplyThemeToMaterialDesign();
        
        OnThemeChanged(oldTheme, _currentTheme);
        _ = SaveThemeAsync();
    }

    public async Task SaveThemeAsync()
    {
        _configurationService.UpdateSettings(settings => settings.Theme = _currentTheme);
        await Task.CompletedTask;
    }

    public async Task LoadThemeAsync()
    {
        _currentTheme = _configurationService.AppSettings.Theme;
        
        // Apply loaded theme
        Application.Current.Dispatcher.Invoke(() =>
        {
            ApplyThemeToMaterialDesign();
        });
        
        await Task.CompletedTask;
    }

    private void ApplyThemeToMaterialDesign()
    {
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            // Set base theme (Light/Dark)
            theme.SetBaseTheme(_currentTheme.IsDarkMode ? Theme.Dark : Theme.Light);

            // Set primary color
            if (TryGetMaterialDesignColor(_currentTheme.PrimaryColor, out var primaryColor))
            {
                theme.SetPrimaryColor(primaryColor);
            }

            // Set secondary color
            if (TryGetMaterialDesignColor(_currentTheme.SecondaryColor, out var secondaryColor))
            {
                theme.SetSecondaryColor(secondaryColor);
            }

            paletteHelper.SetTheme(theme);
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            System.Diagnostics.Debug.WriteLine($"Failed to apply theme: {ex.Message}");
        }
    }

    private static bool TryGetMaterialDesignColor(string colorName, out System.Windows.Media.Color color)
    {
        color = default;
        
        try
        {
            // Try to get the color from MaterialDesignColors
            var swatches = new SwatchesProvider().Swatches;
            var swatch = swatches.FirstOrDefault(s => s.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase));
            
            if (swatch != null)
            {
                color = swatch.ExemplarHue.Color;
                return true;
            }

            // Fallback colors
            color = colorName.ToLowerInvariant() switch
            {
                "deeppurple" => System.Windows.Media.Color.FromRgb(103, 58, 183),
                "lime" => System.Windows.Media.Color.FromRgb(205, 220, 57),
                "blue" => System.Windows.Media.Color.FromRgb(33, 150, 243),
                "green" => System.Windows.Media.Color.FromRgb(76, 175, 80),
                "red" => System.Windows.Media.Color.FromRgb(244, 67, 54),
                "orange" => System.Windows.Media.Color.FromRgb(255, 152, 0),
                _ => System.Windows.Media.Color.FromRgb(103, 58, 183) // Default to deep purple
            };

            return true;
        }
        catch
        {
            color = System.Windows.Media.Color.FromRgb(103, 58, 183); // Default fallback
            return true;
        }
    }

    private void OnThemeChanged(ThemeSettings oldTheme, ThemeSettings newTheme)
    {
        ThemeChanged?.Invoke(this, new AppThemeChangedEventArgs
        {
            OldTheme = oldTheme,
            NewTheme = newTheme
        });
    }
}
