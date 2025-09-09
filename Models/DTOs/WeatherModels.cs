using Newtonsoft.Json;

namespace Global_Insights_Dashboard.Models.DTOs;

/// <summary>
/// Current weather data response from OpenWeatherMap API
/// </summary>
public class CurrentWeatherResponse
{
    [JsonProperty("coord")]
    public Coordinates? Coordinates { get; set; }

    [JsonProperty("weather")]
    public List<WeatherCondition> Weather { get; set; } = new();

    [JsonProperty("base")]
    public string? Base { get; set; }

    [JsonProperty("main")]
    public MainWeatherData? Main { get; set; }

    [JsonProperty("visibility")]
    public int Visibility { get; set; }

    [JsonProperty("wind")]
    public WindData? Wind { get; set; }

    [JsonProperty("clouds")]
    public CloudData? Clouds { get; set; }

    [JsonProperty("rain")]
    public PrecipitationData? Rain { get; set; }

    [JsonProperty("snow")]
    public PrecipitationData? Snow { get; set; }

    [JsonProperty("dt")]
    public long Timestamp { get; set; }

    [JsonProperty("sys")]
    public SystemData? System { get; set; }

    [JsonProperty("timezone")]
    public int Timezone { get; set; }

    [JsonProperty("id")]
    public int CityId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("cod")]
    public int Code { get; set; }

    // Computed properties for UI
    public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).DateTime;
    public string TemperatureDisplay => Main != null ? $"{Math.Round(Main.Temperature)}°" : "N/A";
    public string FeelsLikeDisplay => Main != null ? $"Feels like {Math.Round(Main.FeelsLike)}°" : "";
    public string WeatherDescription => Weather.FirstOrDefault()?.Description?.ToTitleCase() ?? "Unknown";
    public string WeatherIcon => Weather.FirstOrDefault()?.Icon ?? "01d";
}

/// <summary>
/// 5-day weather forecast response from OpenWeatherMap API
/// </summary>
public class WeatherForecastResponse
{
    [JsonProperty("cod")]
    public string? Code { get; set; }

    [JsonProperty("message")]
    public int Message { get; set; }

    [JsonProperty("cnt")]
    public int Count { get; set; }

    [JsonProperty("list")]
    public List<ForecastItem> Items { get; set; } = new();

    [JsonProperty("city")]
    public CityInfo? City { get; set; }

    // Computed property for daily forecast
    public List<DailyForecast> DailyForecast => 
        Items.GroupBy(item => DateTimeOffset.FromUnixTimeSeconds(item.Timestamp).Date)
             .Take(5)
             .Select(group => new DailyForecast
             {
                 Date = group.Key,
                 Items = group.ToList(),
                 MinTemperature = group.Min(x => x.Main?.TemperatureMin ?? 0),
                 MaxTemperature = group.Max(x => x.Main?.TemperatureMax ?? 0),
                 WeatherCondition = group.First().Weather.FirstOrDefault()?.Main ?? "Unknown",
                 WeatherDescription = group.First().Weather.FirstOrDefault()?.Description ?? "Unknown",
                 WeatherIcon = group.First().Weather.FirstOrDefault()?.Icon ?? "01d"
             })
             .ToList();
}

public class ForecastItem
{
    [JsonProperty("dt")]
    public long Timestamp { get; set; }

    [JsonProperty("main")]
    public MainWeatherData? Main { get; set; }

    [JsonProperty("weather")]
    public List<WeatherCondition> Weather { get; set; } = new();

    [JsonProperty("clouds")]
    public CloudData? Clouds { get; set; }

    [JsonProperty("wind")]
    public WindData? Wind { get; set; }

    [JsonProperty("visibility")]
    public int Visibility { get; set; }

    [JsonProperty("pop")]
    public double ProbabilityOfPrecipitation { get; set; }

    [JsonProperty("rain")]
    public PrecipitationData? Rain { get; set; }

    [JsonProperty("snow")]
    public PrecipitationData? Snow { get; set; }

    [JsonProperty("sys")]
    public ForecastSystemData? System { get; set; }

    [JsonProperty("dt_txt")]
    public string? DateTimeText { get; set; }

    public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).DateTime;
}

public class DailyForecast
{
    public DateTime Date { get; set; }
    public List<ForecastItem> Items { get; set; } = new();
    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
    public string WeatherCondition { get; set; } = string.Empty;
    public string WeatherDescription { get; set; } = string.Empty;
    public string WeatherIcon { get; set; } = string.Empty;

    public string DayName => Date.ToString("dddd");
    public string DateDisplay => Date.ToString("MMM dd");
    public string TemperatureRange => $"{Math.Round(MinTemperature)}° / {Math.Round(MaxTemperature)}°";
}

public class Coordinates
{
    [JsonProperty("lon")]
    public double Longitude { get; set; }

    [JsonProperty("lat")]
    public double Latitude { get; set; }
}

public class WeatherCondition
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("main")]
    public string Main { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("icon")]
    public string Icon { get; set; } = string.Empty;
}

public class MainWeatherData
{
    [JsonProperty("temp")]
    public double Temperature { get; set; }

    [JsonProperty("feels_like")]
    public double FeelsLike { get; set; }

    [JsonProperty("temp_min")]
    public double TemperatureMin { get; set; }

    [JsonProperty("temp_max")]
    public double TemperatureMax { get; set; }

    [JsonProperty("pressure")]
    public int Pressure { get; set; }

    [JsonProperty("humidity")]
    public int Humidity { get; set; }

    [JsonProperty("sea_level")]
    public int? SeaLevel { get; set; }

    [JsonProperty("grnd_level")]
    public int? GroundLevel { get; set; }
}

public class WindData
{
    [JsonProperty("speed")]
    public double Speed { get; set; }

    [JsonProperty("deg")]
    public int Degree { get; set; }

    [JsonProperty("gust")]
    public double? Gust { get; set; }

    public string Direction => GetWindDirection(Degree);

    private static string GetWindDirection(int degree)
    {
        var directions = new[] { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
        var index = (int)Math.Round(degree / 22.5) % 16;
        return directions[index];
    }
}

public class CloudData
{
    [JsonProperty("all")]
    public int Coverage { get; set; }
}

public class PrecipitationData
{
    [JsonProperty("1h")]
    public double? OneHour { get; set; }

    [JsonProperty("3h")]
    public double? ThreeHours { get; set; }
}

public class SystemData
{
    [JsonProperty("type")]
    public int? Type { get; set; }

    [JsonProperty("id")]
    public int? Id { get; set; }

    [JsonProperty("country")]
    public string? Country { get; set; }

    [JsonProperty("sunrise")]
    public long Sunrise { get; set; }

    [JsonProperty("sunset")]
    public long Sunset { get; set; }

    public DateTime SunriseTime => DateTimeOffset.FromUnixTimeSeconds(Sunrise).DateTime;
    public DateTime SunsetTime => DateTimeOffset.FromUnixTimeSeconds(Sunset).DateTime;
}

public class ForecastSystemData
{
    [JsonProperty("pod")]
    public string? PartOfDay { get; set; }
}

public class CityInfo
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("coord")]
    public Coordinates? Coordinates { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("population")]
    public int Population { get; set; }

    [JsonProperty("timezone")]
    public int Timezone { get; set; }

    [JsonProperty("sunrise")]
    public long Sunrise { get; set; }

    [JsonProperty("sunset")]
    public long Sunset { get; set; }
}

// Extension method for string formatting
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }
}
