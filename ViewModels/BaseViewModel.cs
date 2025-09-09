using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Global_Insights_Dashboard.ViewModels;

/// <summary>
/// Base view model class with common functionality
/// </summary>
public abstract partial class BaseViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private DateTime _lastUpdated = DateTime.MinValue;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    /// <summary>
    /// Indicates whether the view model has been initialized
    /// </summary>
    public bool IsInitialized { get; protected set; }

    /// <summary>
    /// Service name for identification
    /// </summary>
    public abstract string ServiceName { get; }

    /// <summary>
    /// Initialize the view model
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        if (IsInitialized) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Initializing...";
            
            await OnInitializeAsync();
            
            IsInitialized = true;
            StatusMessage = "Ready";
            LastUpdated = DateTime.Now;
        }
        catch (Exception ex)
        {
            HandleError("Initialization failed", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Override this method to implement service-specific initialization
    /// </summary>
    protected virtual Task OnInitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Refresh the data
    /// </summary>
    [RelayCommand]
    public virtual async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Refreshing...";
            ClearError();
            
            await OnRefreshAsync();
            
            LastUpdated = DateTime.Now;
            StatusMessage = $"Last updated: {LastUpdated:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            HandleError("Refresh failed", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Override this method to implement service-specific refresh logic
    /// </summary>
    protected virtual Task OnRefreshAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handle errors with consistent messaging
    /// </summary>
    protected void HandleError(string message, Exception? exception = null)
    {
        HasError = true;
        ErrorMessage = exception != null ? $"{message}: {exception.Message}" : message;
        StatusMessage = ErrorMessage; // Show the specific error message in status
        
        // Log the error (implement logging as needed)
        System.Diagnostics.Debug.WriteLine($"[{ServiceName}] Error: {ErrorMessage}");
        if (exception != null)
        {
            System.Diagnostics.Debug.WriteLine($"[{ServiceName}] Exception: {exception}");
        }
    }

    /// <summary>
    /// Clear error state
    /// </summary>
    [RelayCommand]
    public void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Show loading state with optional message
    /// </summary>
    protected void ShowLoading(string message = "Loading...")
    {
        IsLoading = true;
        StatusMessage = message;
        ClearError();
    }

    /// <summary>
    /// Hide loading state
    /// </summary>
    protected void HideLoading()
    {
        IsLoading = false;
    }

    /// <summary>
    /// Execute an async operation with error handling and loading state
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> operation, string? loadingMessage = null, string? successMessage = null)
    {
        try
        {
            ShowLoading(loadingMessage ?? "Processing...");
            
            await operation();
            
            if (!string.IsNullOrEmpty(successMessage))
            {
                StatusMessage = successMessage;
            }
            
            LastUpdated = DateTime.Now;
        }
        catch (Exception ex)
        {
            HandleError("Operation failed", ex);
        }
        finally
        {
            HideLoading();
        }
    }

    /// <summary>
    /// Execute an async operation with result and error handling
    /// </summary>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? loadingMessage = null, string? successMessage = null)
    {
        try
        {
            ShowLoading(loadingMessage ?? "Processing...");
            
            var result = await operation();
            
            if (!string.IsNullOrEmpty(successMessage))
            {
                StatusMessage = successMessage;
            }
            
            LastUpdated = DateTime.Now;
            return result;
        }
        catch (Exception ex)
        {
            HandleError("Operation failed", ex);
            return default;
        }
        finally
        {
            HideLoading();
        }
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose pattern implementation
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        // Override in derived classes to dispose resources
    }
}
