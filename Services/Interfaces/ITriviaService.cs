using Global_Insights_Dashboard.Models.DTOs;

namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for trivia data retrieval from Open Trivia Database API
/// </summary>
public interface ITriviaService
{
    /// <summary>
    /// Get trivia questions based on configuration
    /// </summary>
    /// <param name="configuration">Quiz configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trivia response with questions</returns>
    Task<TriviaResponse?> GetTriviaQuestionsAsync(QuizConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get random trivia questions
    /// </summary>
    /// <param name="amount">Number of questions (1-50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trivia response with questions</returns>
    Task<TriviaResponse?> GetRandomQuestionsAsync(int amount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trivia questions by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="amount">Number of questions</param>
    /// <param name="difficulty">Difficulty level</param>
    /// <param name="type">Question type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trivia response with questions</returns>
    Task<TriviaResponse?> GetQuestionsByCategoryAsync(int categoryId, int amount = 10, TriviaDifficulty? difficulty = null, TriviaType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available trivia categories
    /// </summary>
    /// <returns>List of available categories</returns>
    List<TriviaCategory> GetAvailableCategories();

    /// <summary>
    /// Test API connectivity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if API is accessible</returns>
    Task<bool> TestApiConnectivityAsync(CancellationToken cancellationToken = default);
}
