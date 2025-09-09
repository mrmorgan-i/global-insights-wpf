using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;
using Global_Insights_Dashboard.ViewModels;
using Global_Insights_Dashboard.Utils.Validation;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Global_Insights_Dashboard.ViewModels;

/// <summary>
/// ViewModel for the Finance service providing stock quotes and financial data
/// </summary>
public partial class FinanceViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;
    private readonly ICacheService _cacheService;
    private readonly IConfigurationService _configurationService;

    [ObservableProperty]
    private string _symbolInput = string.Empty;

    [ObservableProperty]
    private StockQuoteResponse? _currentQuote;

    [ObservableProperty]
    private ObservableCollection<string> _favoriteSymbols = new();

    [ObservableProperty]
    private bool _hasQuoteData = false;

    [ObservableProperty]
    private DateTime _lastUpdated = DateTime.MinValue;

    [ObservableProperty]
    private ISeries[] _priceSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _yAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private bool _showChart = false;

    public override string ServiceName => "Finance";

    // Popular stock symbols
    public List<StockSymbol> PopularStocks { get; } = new()
    {
        new("AAPL", "Apple Inc."),
        new("MSFT", "Microsoft Corporation"),
        new("GOOGL", "Alphabet Inc."),
        new("AMZN", "Amazon.com Inc."),
        new("TSLA", "Tesla Inc."),
        new("META", "Meta Platforms Inc."),
        new("NVDA", "NVIDIA Corporation"),
        new("NFLX", "Netflix Inc."),
        new("DIS", "The Walt Disney Company"),
        new("BA", "Boeing Company")
    };

    public FinanceViewModel(
        IFinanceService financeService,
        ICacheService cacheService,
        IConfigurationService configurationService)
    {
        _financeService = financeService;
        _cacheService = cacheService;
        _configurationService = configurationService;

        // Initialize chart axes
        InitializeChart();
    }

    protected override async Task OnInitializeAsync()
    {
        // Load user preferences
        var preferences = _configurationService.AppSettings.Preferences;
        FavoriteSymbols = new ObservableCollection<string>(preferences.FavoriteStockSymbols);

        // Debug API key status
        var validation = _configurationService.ValidateApiConfiguration();
        StatusMessage = validation.IsValid ? "API keys loaded successfully" : $"Config error: {validation.ErrorMessage}";
        
        Console.WriteLine($"[FinanceViewModel] Finance API Key present: {!string.IsNullOrEmpty(_configurationService.ApiConfig.Finance.ApiKey)}");

        // Load data for first favorite stock if available
        if (FavoriteSymbols.Any())
        {
            SymbolInput = FavoriteSymbols.First();
            await GetQuoteDataAsync();
        }
    }

    [RelayCommand]
    private async Task GetQuoteDataAsync()
    {
        // Validate stock symbol input
        var validation = CommonValidators.StockSymbol.Validate(SymbolInput?.Trim().ToUpperInvariant());
        if (!validation.IsValid)
        {
            HandleError(validation.ErrorMessage);
            return;
        }

        var symbol = SymbolInput?.Trim().ToUpperInvariant() ?? string.Empty;

        await ExecuteAsync(async () =>
        {
            try
            {
                var cacheKey = $"quote_{symbol}";
                var cachedQuote = await _cacheService.GetAsync<StockQuoteResponse>(cacheKey);

                StockQuoteResponse? quoteResponse = null;

                if (cachedQuote != null)
                {
                    quoteResponse = cachedQuote;
                    StatusMessage = "Loaded from cache";
                }
                else
                {
                    // Check if we have a valid API key
                    if (string.IsNullOrEmpty(_configurationService.ApiConfig.Finance.ApiKey))
                    {
                        StatusMessage = "Finance API key is not configured. Please check your configuration.";
                        return;
                    }

                    quoteResponse = await _financeService.GetStockQuoteAsync(symbol);
                    
                    if (quoteResponse != null)
                    {
                        await _cacheService.SetAsync(cacheKey, quoteResponse, TimeSpan.FromMinutes(5));
                        StatusMessage = $"Quote loaded for {symbol}";
                        LastUpdated = DateTime.Now;
                    }
                }

                if (quoteResponse != null)
                {
                    CurrentQuote = quoteResponse;
                    HasQuoteData = true;
                    SymbolInput = symbol; // Normalize the symbol display

                    // Load historical data for chart
                    await LoadHistoricalDataAsync(symbol);
                }
                else
                {
                    CurrentQuote = null;
                    HasQuoteData = false;
                    ShowChart = false;
                    StatusMessage = $"No data found for symbol {symbol}";
                }
            }
            catch (Exception ex)
            {
                HandleError($"Failed to load quote for {symbol}", ex);
                CurrentQuote = null;
                HasQuoteData = false;
                ShowChart = false;
            }
        });
    }

    [RelayCommand]
    private async Task LoadPopularStock(StockSymbol? stock)
    {
        if (stock != null)
        {
            SymbolInput = stock.Symbol;
            await GetQuoteDataAsync();
        }
    }

    [RelayCommand]
    private void AddToFavorites()
    {
        if (string.IsNullOrWhiteSpace(SymbolInput))
            return;

        var symbol = SymbolInput.Trim().ToUpperInvariant();
        if (!FavoriteSymbols.Contains(symbol))
        {
            FavoriteSymbols.Add(symbol);
            
            // Save to configuration
            _configurationService.UpdateSettings(settings =>
            {
                settings.Preferences.FavoriteStockSymbols = FavoriteSymbols.ToList();
            });
            
            StatusMessage = $"Added {symbol} to favorites";
        }
    }

    [RelayCommand]
    private void RemoveFromFavorites(string? symbol)
    {
        if (!string.IsNullOrEmpty(symbol) && FavoriteSymbols.Contains(symbol))
        {
            FavoriteSymbols.Remove(symbol);
            
            // Save to configuration
            _configurationService.UpdateSettings(settings =>
            {
                settings.Preferences.FavoriteStockSymbols = FavoriteSymbols.ToList();
            });

            StatusMessage = $"Removed {symbol} from favorites";
        }
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        if (CurrentQuote != null)
        {
            // Clear cache for current symbol
            var cacheKey = $"quote_{CurrentQuote.Quote?.Symbol}";
            await _cacheService.RemoveAsync(cacheKey);
            
            // Clear historical data cache
            var historicalCacheKey = $"historical_{CurrentQuote.Quote?.Symbol}";
            await _cacheService.RemoveAsync(historicalCacheKey);
            
            await GetQuoteDataAsync();
        }
    }

    private async Task LoadHistoricalDataAsync(string symbol)
    {
        try
        {
            var cacheKey = $"historical_{symbol}";
            var cachedData = await _cacheService.GetAsync<IntradayTimeSeriesResponse>(cacheKey);

            IntradayTimeSeriesResponse? timeSeriesData = null;

            if (cachedData != null)
            {
                timeSeriesData = cachedData;
            }
            else
            {
                timeSeriesData = await _financeService.GetIntradayDataAsync(symbol);
                
                if (timeSeriesData != null)
                {
                    await _cacheService.SetAsync(cacheKey, timeSeriesData, TimeSpan.FromHours(1));
                }
            }

            if (timeSeriesData?.TimeSeries5Min != null && timeSeriesData.TimeSeries5Min.Any())
            {
                UpdateChart(timeSeriesData);
                ShowChart = true;
            }
            else
            {
                ShowChart = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FinanceViewModel] Failed to load historical data: {ex.Message}");
            ShowChart = false;
        }
    }

    private void UpdateChart(IntradayTimeSeriesResponse timeSeriesData)
    {
        try
        {
            // Take last 30 data points
            var recentData = timeSeriesData.TimeSeries5Min?
                .OrderBy(kvp => DateTime.Parse(kvp.Key))
                .TakeLast(30)
                .ToList() ?? new List<KeyValuePair<string, TimeSeriesData>>();

            var values = recentData
                .Select((kvp, index) => new { Index = index, Price = (decimal)kvp.Value.Close })
                .ToList();

            PriceSeries = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Values = values.Select(v => v.Price),
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(SKColors.DeepSkyBlue),
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    GeometrySize = 4,
                    LineSmoothness = 0.8
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = recentData.Select(kvp => DateTime.Parse(kvp.Key).ToString("MM/dd")).ToArray(),
                    LabelsRotation = -45,
                    TextSize = 12
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Price ($)",
                    NameTextSize = 14,
                    TextSize = 12,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FinanceViewModel] Failed to update chart: {ex.Message}");
            ShowChart = false;
        }
    }

    private void InitializeChart()
    {
        XAxes = new Axis[] { new Axis() };
        YAxes = new Axis[] { new Axis { Name = "Price ($)" } };
        PriceSeries = Array.Empty<ISeries>();
    }
}

/// <summary>
/// Helper class for popular stock symbols
/// </summary>
public record StockSymbol(string Symbol, string Name);
