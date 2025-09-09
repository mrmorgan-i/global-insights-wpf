using System.Net.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Global_Insights_Dashboard.Models.Configuration;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Implementation of trivia service using Open Trivia Database API
/// </summary>
public class TriviaService : ITriviaService
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguration _apiConfig;

    public TriviaService(HttpClient httpClient, IOptions<ApiConfiguration> apiConfig)
    {
        _httpClient = httpClient;
        _apiConfig = apiConfig.Value;
    }

    public async Task<TriviaResponse?> GetTriviaQuestionsAsync(QuizConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration.NumberOfQuestions < 1 || configuration.NumberOfQuestions > 50)
            throw new ArgumentException("Number of questions must be between 1 and 50", nameof(configuration));

        var url = BuildQuestionsUrl(configuration);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var triviaData = JsonConvert.DeserializeObject<TriviaResponse>(response);
            
            return triviaData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve trivia questions: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException("Trivia API request timed out", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse trivia data: {ex.Message}", ex);
        }
    }

    public async Task<TriviaResponse?> GetRandomQuestionsAsync(int amount = 10, CancellationToken cancellationToken = default)
    {
        if (amount < 1 || amount > 50)
            throw new ArgumentException("Amount must be between 1 and 50", nameof(amount));

        var configuration = new QuizConfiguration
        {
            NumberOfQuestions = amount
        };

        return await GetTriviaQuestionsAsync(configuration, cancellationToken);
    }

    public async Task<TriviaResponse?> GetQuestionsByCategoryAsync(int categoryId, int amount = 10, TriviaDifficulty? difficulty = null, TriviaType? type = null, CancellationToken cancellationToken = default)
    {
        if (amount < 1 || amount > 50)
            throw new ArgumentException("Amount must be between 1 and 50", nameof(amount));

        var configuration = new QuizConfiguration
        {
            NumberOfQuestions = amount,
            Category = TriviaCategory.GetAvailableCategories().FirstOrDefault(c => c.Id == categoryId),
            Difficulty = difficulty,
            Type = type
        };

        return await GetTriviaQuestionsAsync(configuration, cancellationToken);
    }

    public List<TriviaCategory> GetAvailableCategories()
    {
        return TriviaCategory.GetAvailableCategories();
    }

    public async Task<bool> TestApiConnectivityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Test with a simple request
            var result = await GetRandomQuestionsAsync(1, cancellationToken);
            return result != null && result.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    private string BuildQuestionsUrl(QuizConfiguration configuration)
    {
        var parameters = new List<string>
        {
            $"amount={configuration.NumberOfQuestions}"
        };

        if (configuration.Category != null)
        {
            parameters.Add($"category={configuration.Category.Id}");
        }

        var difficultyName = configuration.GetDifficultyName();
        if (!string.IsNullOrEmpty(difficultyName))
        {
            parameters.Add($"difficulty={difficultyName}");
        }

        var typeName = configuration.GetTypeName();
        if (!string.IsNullOrEmpty(typeName))
        {
            parameters.Add($"type={typeName}");
        }

        // Add encoding parameter for better character handling
        parameters.Add("encode=url3986");

        return $"{_apiConfig.Trivia.BaseUrl}?{string.Join("&", parameters)}";
    }
}
