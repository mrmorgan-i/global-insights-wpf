using System.Windows.Controls;

namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for handling navigation between different views/services
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Event raised when the current view changes
    /// </summary>
    event EventHandler<NavigationEventArgs>? NavigationChanged;

    /// <summary>
    /// Get the current active service name
    /// </summary>
    string CurrentService { get; }

    /// <summary>
    /// Navigate to a specific service
    /// </summary>
    /// <param name="serviceName">Name of the service to navigate to</param>
    void NavigateTo(string serviceName);

    /// <summary>
    /// Navigate to a specific view
    /// </summary>
    /// <param name="view">UserControl to navigate to</param>
    /// <param name="serviceName">Name of the service</param>
    void NavigateTo(UserControl view, string serviceName);

    /// <summary>
    /// Get available services for navigation
    /// </summary>
    /// <returns>List of available service names</returns>
    List<string> GetAvailableServices();

    /// <summary>
    /// Check if a service is available
    /// </summary>
    /// <param name="serviceName">Service name to check</param>
    /// <returns>True if service exists</returns>
    bool IsServiceAvailable(string serviceName);

    /// <summary>
    /// Go back to the previous service
    /// </summary>
    /// <returns>True if navigation occurred</returns>
    bool GoBack();

    /// <summary>
    /// Get navigation history
    /// </summary>
    /// <returns>List of previously visited services</returns>
    List<string> GetNavigationHistory();

    /// <summary>
    /// Clear navigation history
    /// </summary>
    void ClearHistory();
}

/// <summary>
/// Navigation event arguments
/// </summary>
public class NavigationEventArgs : EventArgs
{
    public string FromService { get; set; } = string.Empty;
    public string ToService { get; set; } = string.Empty;
    public UserControl? View { get; set; }
    public DateTime NavigationTime { get; set; } = DateTime.Now;
}
