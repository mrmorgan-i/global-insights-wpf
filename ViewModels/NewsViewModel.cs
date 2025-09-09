using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;
using Global_Insights_Dashboard.ViewModels;

namespace Global_Insights_Dashboard.ViewModels;

/// <summary>
/// ViewModel for the News service providing news headlines and search functionality
/// </summary>
public partial class NewsViewModel : BaseViewModel
{
    private readonly INewsService _newsService;
    private readonly ICacheService _cacheService;
    private readonly IConfigurationService _configurationService;

    [ObservableProperty]
    private ObservableCollection<NewsArticle> _articles = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCountry = "us";

    [ObservableProperty]
    private string _selectedCategory = "general";

    [ObservableProperty]
    private bool _hasArticles = false;

    [ObservableProperty]
    private DateTime _lastUpdated = DateTime.MinValue;

    public override string ServiceName => "News";

    // Available countries for news
    public Dictionary<string, string> AvailableCountries { get; } = new()
    {
        { "us", "United States" },
        { "gb", "United Kingdom" },
        { "ca", "Canada" },
        { "au", "Australia" },
        { "de", "Germany" },
        { "fr", "France" },
        { "it", "Italy" },
        { "jp", "Japan" },
        { "cn", "China" },
        { "in", "India" },
        { "br", "Brazil" },
        { "mx", "Mexico" },
        { "ru", "Russia" },
        { "kr", "South Korea" },
        { "nl", "Netherlands" }
    };

    // Available categories
    public Dictionary<string, string> AvailableCategories { get; } = new()
    {
        { "general", "General" },
        { "business", "Business" },
        { "entertainment", "Entertainment" },
        { "health", "Health" },
        { "science", "Science" },
        { "sports", "Sports" },
        { "technology", "Technology" }
    };

    public NewsViewModel(
        INewsService newsService,
        ICacheService cacheService,
        IConfigurationService configurationService)
    {
        _newsService = newsService;
        _cacheService = cacheService;
        _configurationService = configurationService;
    }

    protected override async Task OnInitializeAsync()
    {
        // Load user preferences
        var preferences = _configurationService.AppSettings.Preferences;
        SelectedCountry = preferences.NewsCountry;
        SelectedCategory = preferences.NewsCategory;

        // Check API configuration
        var validation = _configurationService.ValidateApiConfiguration();
        if (!validation.IsValid)
        {
            StatusMessage = $"Configuration error: {validation.ErrorMessage}";
        }

        // Load initial news
        await LoadNewsAsync();
    }

    [RelayCommand]
    private async Task LoadNewsAsync()
    {
        await ExecuteAsync(async () =>
        {
            try
            {
                var cacheKey = $"news_{SelectedCountry}_{SelectedCategory}";
                var cachedNews = await _cacheService.GetAsync<NewsResponse>(cacheKey);

                NewsResponse? newsResponse = null;

                if (cachedNews != null)
                {
                    newsResponse = cachedNews;
                    StatusMessage = "Loaded from cache";
                }
                else
                {
                    // Check if we have a valid API key
                    if (string.IsNullOrEmpty(_configurationService.ApiConfig.News.ApiKey))
                    {
                        StatusMessage = "News API key is not configured. Please check your configuration.";
                        return;
                    }

                    newsResponse = await _newsService.GetTopHeadlinesAsync(SelectedCountry, SelectedCategory);
                    
                    if (newsResponse?.Articles?.Any() == true)
                    {
                        await _cacheService.SetAsync(cacheKey, newsResponse, TimeSpan.FromMinutes(15));
                        StatusMessage = $"Loaded {newsResponse.Articles.Count} articles";
                        LastUpdated = DateTime.Now;
                    }
                }

                if (newsResponse?.Articles?.Any() == true)
                {
                    Articles.Clear();
                    foreach (var article in newsResponse.Articles.Take(50)) // Limit to 50 articles
                    {
                        Articles.Add(article);
                    }
                    HasArticles = true;
                }
                else
                {
                    Articles.Clear();
                    HasArticles = false;
                    StatusMessage = "No articles found";
                }

                // Save user preferences
                _configurationService.UpdateSettings(settings =>
                {
                    settings.Preferences.NewsCountry = SelectedCountry;
                    settings.Preferences.NewsCategory = SelectedCategory;
                });
            }
            catch (Exception ex)
            {
                HandleError("Failed to load news", ex);
                StatusMessage = $"Error: {ex.Message}";
            }
        });
    }

    [RelayCommand]
    private async Task SearchNewsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadNewsAsync();
            return;
        }

        await ExecuteAsync(async () =>
        {
            try
            {
                var cacheKey = $"news_search_{SearchQuery}";
                var cachedNews = await _cacheService.GetAsync<NewsResponse>(cacheKey);

                NewsResponse? newsResponse = null;

                if (cachedNews != null)
                {
                    newsResponse = cachedNews;
                    StatusMessage = "Search results from cache";
                }
                else
                {
                    // Check if we have a valid API key
                    if (string.IsNullOrEmpty(_configurationService.ApiConfig.News.ApiKey))
                    {
                        StatusMessage = "News API key is not configured. Please check your configuration.";
                        return;
                    }

                    newsResponse = await _newsService.SearchNewsAsync(SearchQuery);
                    
                    if (newsResponse?.Articles?.Any() == true)
                    {
                        await _cacheService.SetAsync(cacheKey, newsResponse, TimeSpan.FromMinutes(15));
                        StatusMessage = $"Found {newsResponse.Articles.Count} articles";
                        LastUpdated = DateTime.Now;
                    }
                }

                if (newsResponse?.Articles?.Any() == true)
                {
                    Articles.Clear();
                    foreach (var article in newsResponse.Articles.Take(50))
                    {
                        Articles.Add(article);
                    }
                    HasArticles = true;
                }
                else
                {
                    Articles.Clear();
                    HasArticles = false;
                    StatusMessage = "No articles found for your search";
                }
            }
            catch (Exception ex)
            {
                HandleError("Failed to search news", ex);
                StatusMessage = $"Search error: {ex.Message}";
            }
        });
    }

    [RelayCommand]
    private void OpenArticle(NewsArticle? article)
    {
        if (article?.Url != null)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = article.Url,
                    UseShellExecute = true
                });
                StatusMessage = "Article opened in browser";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to open article: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task RefreshNewsAsync()
    {
        // Clear cache for current selection
        var cacheKey = $"news_{SelectedCountry}_{SelectedCategory}";
        await _cacheService.RemoveAsync(cacheKey);
        
        await LoadNewsAsync();
    }

    // Property change handlers
    partial void OnSelectedCountryChanged(string value)
    {
        if (!IsLoading)
        {
            _ = LoadNewsAsync();
        }
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        if (!IsLoading)
        {
            _ = LoadNewsAsync();
        }
    }
}
