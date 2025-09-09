using Newtonsoft.Json;

namespace Global_Insights_Dashboard.Models.DTOs;

/// <summary>
/// Open Trivia Database API response
/// </summary>
public class TriviaResponse
{
    [JsonProperty("response_code")]
    public int ResponseCode { get; set; }

    [JsonProperty("results")]
    public List<TriviaQuestion> Results { get; set; } = new();

    public bool IsSuccess => ResponseCode == 0;
    public string ErrorMessage => GetErrorMessage(ResponseCode);

    private static string GetErrorMessage(int code)
    {
        return code switch
        {
            0 => string.Empty,
            1 => "No results found. The API doesn't have enough questions for your query.",
            2 => "Invalid parameter. Contains an invalid parameter.",
            3 => "Token not found. Session token does not exist.",
            4 => "Token empty. Session token has returned all possible questions for the specified query.",
            _ => "Unknown error occurred."
        };
    }
}

/// <summary>
/// Individual trivia question
/// </summary>
public class TriviaQuestion
{
    [JsonProperty("category")]
    public string Category { get; set; } = string.Empty;

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonProperty("question")]
    public string Question { get; set; } = string.Empty;

    [JsonProperty("correct_answer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [JsonProperty("incorrect_answers")]
    public List<string> IncorrectAnswers { get; set; } = new();

    // Computed properties for UI
    public List<string> AllAnswers
    {
        get
        {
            var answers = new List<string>(IncorrectAnswers) { CorrectAnswer };
            return answers.OrderBy(x => Guid.NewGuid()).ToList(); // Shuffle answers
        }
    }

    public bool IsMultipleChoice => Type.Equals("multiple", StringComparison.OrdinalIgnoreCase);
    public bool IsTrueFalse => Type.Equals("boolean", StringComparison.OrdinalIgnoreCase);
    public string QuestionHtml => System.Net.WebUtility.HtmlDecode(Question);
    public string CorrectAnswerHtml => System.Net.WebUtility.HtmlDecode(CorrectAnswer);
    public List<string> AllAnswersHtml => AllAnswers.Where(x => x != null).Select(x => System.Net.WebUtility.HtmlDecode(x!)).ToList();
    public string DifficultyDisplay => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Difficulty);
    public string CategoryDisplay => Category.Replace("Entertainment: ", "").Replace("Science: ", "");
}

/// <summary>
/// Quiz configuration settings
/// </summary>
public class QuizConfiguration
{
    public int NumberOfQuestions { get; set; } = 10;
    public TriviaCategory? Category { get; set; }
    public TriviaDifficulty? Difficulty { get; set; }
    public TriviaType? Type { get; set; }

    public string GetCategoryId()
    {
        return Category?.Id.ToString() ?? string.Empty;
    }

    public string GetDifficultyName()
    {
        return Difficulty switch
        {
            TriviaDifficulty.Easy => "easy",
            TriviaDifficulty.Medium => "medium",
            TriviaDifficulty.Hard => "hard",
            _ => string.Empty
        };
    }

    public string GetTypeName()
    {
        return Type switch
        {
            TriviaType.MultipleChoice => "multiple",
            TriviaType.TrueFalse => "boolean",
            _ => string.Empty
        };
    }
}

/// <summary>
/// Quiz session data
/// </summary>
public class QuizSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public QuizConfiguration Configuration { get; set; } = new();
    public List<TriviaQuestion> Questions { get; set; } = new();
    public List<QuizAnswer> Answers { get; set; } = new();
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public int CurrentQuestionIndex { get; set; } = 0;

    public bool IsCompleted => CurrentQuestionIndex >= Questions.Count;
    public int CorrectAnswers => Answers.Count(a => a.IsCorrect);
    public int TotalQuestions => Questions.Count;
    public double ScorePercentage => TotalQuestions > 0 ? (double)CorrectAnswers / TotalQuestions * 100 : 0;
    public TimeSpan Duration => (EndTime ?? DateTime.Now) - StartTime;
    public string ScoreDisplay => $"{CorrectAnswers}/{TotalQuestions} ({ScorePercentage:F1}%)";
    public string DurationDisplay => Duration.ToString(@"mm\:ss");
    public TriviaQuestion? CurrentQuestion => CurrentQuestionIndex < Questions.Count ? Questions[CurrentQuestionIndex] : null;

    public string GetPerformanceFeedback()
    {
        return ScorePercentage switch
        {
            >= 90 => "Excellent! 🏆",
            >= 80 => "Great job! 🎉",
            >= 70 => "Good work! 👍",
            >= 60 => "Not bad! 👌",
            >= 50 => "Keep trying! 💪",
            _ => "Better luck next time! 🍀"
        };
    }
}

/// <summary>
/// Individual quiz answer
/// </summary>
public class QuizAnswer
{
    public int QuestionIndex { get; set; }
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public DateTime AnsweredAt { get; set; } = DateTime.Now;
    public TimeSpan TimeSpent { get; set; }

    public bool IsCorrect => UserAnswer.Equals(CorrectAnswer, StringComparison.OrdinalIgnoreCase);
    public string Result => IsCorrect ? "✅ Correct" : "❌ Incorrect";
    public string UserAnswerHtml => System.Net.WebUtility.HtmlDecode(UserAnswer);
    public string CorrectAnswerHtml => System.Net.WebUtility.HtmlDecode(CorrectAnswer);
}

/// <summary>
/// Trivia categories available from Open Trivia Database
/// </summary>
public class TriviaCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public static List<TriviaCategory> GetAvailableCategories()
    {
        return new List<TriviaCategory>
        {
            new() { Id = 9, Name = "General Knowledge", Icon = "🧠" },
            new() { Id = 10, Name = "Entertainment: Books", Icon = "📚" },
            new() { Id = 11, Name = "Entertainment: Film", Icon = "🎬" },
            new() { Id = 12, Name = "Entertainment: Music", Icon = "🎵" },
            new() { Id = 13, Name = "Entertainment: Musicals & Theatres", Icon = "🎭" },
            new() { Id = 14, Name = "Entertainment: Television", Icon = "📺" },
            new() { Id = 15, Name = "Entertainment: Video Games", Icon = "🎮" },
            new() { Id = 16, Name = "Entertainment: Board Games", Icon = "🎲" },
            new() { Id = 17, Name = "Science & Nature", Icon = "🔬" },
            new() { Id = 18, Name = "Science: Computers", Icon = "💻" },
            new() { Id = 19, Name = "Science: Mathematics", Icon = "🔢" },
            new() { Id = 20, Name = "Mythology", Icon = "⚡" },
            new() { Id = 21, Name = "Sports", Icon = "⚽" },
            new() { Id = 22, Name = "Geography", Icon = "🌍" },
            new() { Id = 23, Name = "History", Icon = "📜" },
            new() { Id = 24, Name = "Politics", Icon = "🏛️" },
            new() { Id = 25, Name = "Art", Icon = "🎨" },
            new() { Id = 26, Name = "Celebrities", Icon = "🌟" },
            new() { Id = 27, Name = "Animals", Icon = "🐾" },
            new() { Id = 28, Name = "Vehicles", Icon = "🚗" },
            new() { Id = 29, Name = "Entertainment: Comics", Icon = "💭" },
            new() { Id = 30, Name = "Science: Gadgets", Icon = "📱" },
            new() { Id = 31, Name = "Entertainment: Japanese Anime & Manga", Icon = "🍥" },
            new() { Id = 32, Name = "Entertainment: Cartoon & Animations", Icon = "🎪" }
        };
    }
}

/// <summary>
/// Difficulty levels for trivia questions
/// </summary>
public enum TriviaDifficulty
{
    Easy,
    Medium,
    Hard
}

/// <summary>
/// Question types for trivia
/// </summary>
public enum TriviaType
{
    MultipleChoice,
    TrueFalse
}

/// <summary>
/// Quiz statistics for tracking performance
/// </summary>
public class QuizStatistics
{
    public int TotalQuizzesCompleted { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public double OverallAccuracy => TotalQuestionsAnswered > 0 ? (double)TotalCorrectAnswers / TotalQuestionsAnswered * 100 : 0;
    public Dictionary<string, int> CategoryStats { get; set; } = new();
    public Dictionary<string, int> DifficultyStats { get; set; } = new();
    public TimeSpan TotalTimeSpent { get; set; }
    public DateTime LastQuizDate { get; set; } = DateTime.MinValue;

    public string AccuracyDisplay => $"{OverallAccuracy:F1}%";
    public string TotalTimeDisplay => TotalTimeSpent.ToString(@"hh\:mm\:ss");
    
    public void UpdateStatistics(QuizSession session)
    {
        if (!session.IsCompleted) return;

        TotalQuizzesCompleted++;
        TotalQuestionsAnswered += session.TotalQuestions;
        TotalCorrectAnswers += session.CorrectAnswers;
        TotalTimeSpent = TotalTimeSpent.Add(session.Duration);
        LastQuizDate = session.EndTime ?? DateTime.Now;

        // Update category stats
        foreach (var question in session.Questions)
        {
            var category = question.CategoryDisplay;
            CategoryStats[category] = CategoryStats.GetValueOrDefault(category, 0) + 1;
        }

        // Update difficulty stats
        foreach (var question in session.Questions)
        {
            var difficulty = question.DifficultyDisplay;
            DifficultyStats[difficulty] = DifficultyStats.GetValueOrDefault(difficulty, 0) + 1;
        }
    }
}
