using Newtonsoft.Json;

namespace Global_Insights_Dashboard.Models.DTOs;

/// <summary>
/// News API response for headlines and search results
/// </summary>
public class NewsResponse
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("totalResults")]
    public int TotalResults { get; set; }

    [JsonProperty("articles")]
    public List<NewsArticle> Articles { get; set; } = new();

    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }

    public bool IsSuccess => Status == "ok";
    public bool HasError => !string.IsNullOrEmpty(Code) || !string.IsNullOrEmpty(Message);
}

/// <summary>
/// Individual news article from NewsAPI
/// </summary>
public class NewsArticle
{
    [JsonProperty("source")]
    public NewsSource? Source { get; set; }

    [JsonProperty("author")]
    public string? Author { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("urlToImage")]
    public string? ImageUrl { get; set; }

    [JsonProperty("publishedAt")]
    public DateTime PublishedAt { get; set; }

    [JsonProperty("content")]
    public string? Content { get; set; }

    // Computed properties for UI
    public string PublishedTimeAgo => GetTimeAgo(PublishedAt);
    public string PublishedDisplay => PublishedAt.ToString("MMM dd, yyyy HH:mm");
    public string SourceDisplay => Source?.Name ?? "Unknown Source";
    public string AuthorDisplay => string.IsNullOrEmpty(Author) ? SourceDisplay : Author;
    public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
    public string TruncatedDescription => TruncateText(Description, 150);
    public string TruncatedTitle => TruncateText(Title, 80);

    private static string GetTimeAgo(DateTime publishedAt)
    {
        var timeSpan = DateTime.UtcNow - publishedAt;

        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";

        return "Just now";
    }

    private static string TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (text.Length <= maxLength) return text;
        
        return text.Substring(0, maxLength).Trim() + "...";
    }
}

/// <summary>
/// News source information
/// </summary>
public class NewsSource
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// News category options for filtering
/// </summary>
public enum NewsCategory
{
    General,
    Business,
    Entertainment,
    Health,
    Science,
    Sports,
    Technology
}

/// <summary>
/// Country options for news filtering
/// </summary>
public enum NewsCountry
{
    US,
    GB,
    CA,
    AU,
    IN,
    DE,
    FR
}

/// <summary>
/// News search request parameters
/// </summary>
public class NewsSearchRequest
{
    public string? Query { get; set; }
    public NewsCountry? Country { get; set; }
    public NewsCategory? Category { get; set; }
    public int PageSize { get; set; } = 20;
    public int Page { get; set; } = 1;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Sources { get; set; }
    public string? Language { get; set; } = "en";

    public string GetCountryCode()
    {
        return Country switch
        {
            NewsCountry.US => "us",
            NewsCountry.GB => "gb",
            NewsCountry.CA => "ca",
            NewsCountry.AU => "au",
            NewsCountry.IN => "in",
            NewsCountry.DE => "de",
            NewsCountry.FR => "fr",
            _ => "us"
        };
    }

    public string GetCategoryName()
    {
        return Category switch
        {
            NewsCategory.General => "general",
            NewsCategory.Business => "business",
            NewsCategory.Entertainment => "entertainment",
            NewsCategory.Health => "health",
            NewsCategory.Science => "science",
            NewsCategory.Sports => "sports",
            NewsCategory.Technology => "technology",
            _ => "general"
        };
    }
}

/// <summary>
/// Country information for news filtering
/// </summary>
public class CountryInfo
{
    public NewsCountry Country { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;

    public static List<CountryInfo> GetAvailableCountries()
    {
        return new List<CountryInfo>
        {
            new() { Country = NewsCountry.US, Code = "us", Name = "United States", Flag = "🇺🇸" },
            new() { Country = NewsCountry.GB, Code = "gb", Name = "United Kingdom", Flag = "🇬🇧" },
            new() { Country = NewsCountry.CA, Code = "ca", Name = "Canada", Flag = "🇨🇦" },
            new() { Country = NewsCountry.AU, Code = "au", Name = "Australia", Flag = "🇦🇺" },
            new() { Country = NewsCountry.IN, Code = "in", Name = "India", Flag = "🇮🇳" },
            new() { Country = NewsCountry.DE, Code = "de", Name = "Germany", Flag = "🇩🇪" },
            new() { Country = NewsCountry.FR, Code = "fr", Name = "France", Flag = "🇫🇷" }
        };
    }
}

/// <summary>
/// Category information for news filtering
/// </summary>
public class CategoryInfo
{
    public NewsCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public static List<CategoryInfo> GetAvailableCategories()
    {
        return new List<CategoryInfo>
        {
            new() { Category = NewsCategory.General, Name = "General", Icon = "📰", Description = "General news and current events" },
            new() { Category = NewsCategory.Business, Name = "Business", Icon = "💼", Description = "Business and financial news" },
            new() { Category = NewsCategory.Entertainment, Name = "Entertainment", Icon = "🎬", Description = "Entertainment and celebrity news" },
            new() { Category = NewsCategory.Health, Name = "Health", Icon = "🏥", Description = "Health and medical news" },
            new() { Category = NewsCategory.Science, Name = "Science", Icon = "🔬", Description = "Science and research news" },
            new() { Category = NewsCategory.Sports, Name = "Sports", Icon = "⚽", Description = "Sports news and updates" },
            new() { Category = NewsCategory.Technology, Name = "Technology", Icon = "💻", Description = "Technology and digital news" }
        };
    }
}
