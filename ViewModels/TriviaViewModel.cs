using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;
using Global_Insights_Dashboard.Utils.Validation;
using Global_Insights_Dashboard.ViewModels;

namespace Global_Insights_Dashboard.ViewModels;

/// <summary>
/// ViewModel for the Trivia service providing quiz functionality
/// </summary>
public partial class TriviaViewModel : BaseViewModel
{
    private readonly ITriviaService _triviaService;
    private readonly ICacheService _cacheService;
    private readonly IConfigurationService _configurationService;

    private TriviaResponse? _currentTriviaSet;
    private int _currentQuestionIndex = 0;
    private List<string> _userAnswers = new();
    private DateTime _quizStartTime;

    [ObservableProperty]
    private int _numberOfQuestions = 10;

    [ObservableProperty]
    private string _selectedCategory = "any";

    [ObservableProperty]
    private string _selectedDifficulty = "any";

    [ObservableProperty]
    private string _selectedType = "any";

    [ObservableProperty]
    private TriviaQuestion? _currentQuestion;

    [ObservableProperty]
    private ObservableCollection<string> _currentAnswerOptions = new();

    [ObservableProperty]
    private string? _selectedAnswer;

    [ObservableProperty]
    private bool _isQuizActive = false;

    [ObservableProperty]
    private bool _isQuizComplete = false;

    [ObservableProperty]
    private int _currentQuestionNumber = 0;

    [ObservableProperty]
    private int _totalQuestions = 0;

    [ObservableProperty]
    private int _correctAnswers = 0;

    [ObservableProperty]
    private TimeSpan _elapsedTime = TimeSpan.Zero;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private bool _showResults = false;

    [ObservableProperty]
    private bool _showAnswerFeedback = false;

    [ObservableProperty]
    private bool _isAnswerCorrect = false;

    [ObservableProperty]
    private string _feedbackMessage = string.Empty;

    [ObservableProperty]
    private string _correctAnswerText = string.Empty;

    public override string ServiceName => "Trivia";

    // Computed property for score percentage
    public double ScorePercentage => TotalQuestions > 0 ? (double)CorrectAnswers / TotalQuestions * 100 : 0;

    // Available categories
    public Dictionary<string, string> AvailableCategories { get; } = new()
    {
        { "any", "Any Category" },
        { "9", "General Knowledge" },
        { "10", "Entertainment: Books" },
        { "11", "Entertainment: Film" },
        { "12", "Entertainment: Music" },
        { "13", "Entertainment: Musicals & Theatres" },
        { "14", "Entertainment: Television" },
        { "15", "Entertainment: Video Games" },
        { "16", "Entertainment: Board Games" },
        { "17", "Science & Nature" },
        { "18", "Science: Computers" },
        { "19", "Science: Mathematics" },
        { "20", "Mythology" },
        { "21", "Sports" },
        { "22", "Geography" },
        { "23", "History" },
        { "24", "Politics" },
        { "25", "Art" },
        { "26", "Celebrities" },
        { "27", "Animals" },
        { "28", "Vehicles" }
    };

    // Difficulty levels
    public Dictionary<string, string> AvailableDifficulties { get; } = new()
    {
        { "any", "Any Difficulty" },
        { "easy", "Easy" },
        { "medium", "Medium" },
        { "hard", "Hard" }
    };

    // Question types
    public Dictionary<string, string> AvailableTypes { get; } = new()
    {
        { "any", "Any Type" },
        { "multiple", "Multiple Choice" },
        { "boolean", "True/False" }
    };

    public TriviaViewModel(
        ITriviaService triviaService,
        ICacheService cacheService,
        IConfigurationService configurationService)
    {
        _triviaService = triviaService;
        _cacheService = cacheService;
        _configurationService = configurationService;
    }

    protected override Task OnInitializeAsync()
    {
        StatusMessage = "Ready to start a quiz! Select your preferences and click Start Quiz.";
        UpdateProgressText();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartQuizAsync()
    {
        // Validate number of questions
        var validation = CommonValidators.TriviaQuestionCount.Validate(NumberOfQuestions.ToString());
        if (!validation.IsValid)
        {
            HandleError(validation.ErrorMessage);
            return;
        }

        await ExecuteAsync(async () =>
        {
            try
            {
                StatusMessage = "Loading quiz questions...";
                
                TriviaDifficulty? difficulty = SelectedDifficulty switch
                {
                    "easy" => TriviaDifficulty.Easy,
                    "medium" => TriviaDifficulty.Medium,
                    "hard" => TriviaDifficulty.Hard,
                    _ => null
                };

                TriviaType? type = SelectedType switch
                {
                    "multiple" => TriviaType.MultipleChoice,
                    "boolean" => TriviaType.TrueFalse,
                    _ => null
                };

                TriviaCategory? category = null;
                if (SelectedCategory != "any" && int.TryParse(SelectedCategory, out int categoryId))
                {
                    category = TriviaCategory.GetAvailableCategories().FirstOrDefault(c => c.Id == categoryId);
                }

                var config = new QuizConfiguration
                {
                    NumberOfQuestions = NumberOfQuestions,
                    Category = category,
                    Difficulty = difficulty,
                    Type = type
                };

                _currentTriviaSet = await _triviaService.GetTriviaQuestionsAsync(config);

                if (_currentTriviaSet?.Results?.Any() == true)
                {
                    _currentQuestionIndex = 0;
                    _userAnswers = new List<string>();
                    _quizStartTime = DateTime.Now;
                    
                    TotalQuestions = _currentTriviaSet.Results.Count;
                    CurrentQuestionNumber = 1;
                    CorrectAnswers = 0;
                    IsQuizActive = true;
                    IsQuizComplete = false;
                    ShowResults = false;

                    LoadCurrentQuestion();
                    UpdateProgressText();
                    StatusMessage = $"Quiz started! Question 1 of {TotalQuestions}";
                }
                else
                {
                    StatusMessage = "Failed to load quiz questions. Please try again.";
                }
            }
            catch (Exception ex)
            {
                HandleError("Failed to start quiz", ex);
                StatusMessage = $"Error starting quiz: {ex.Message}";
            }
        });
    }

    [RelayCommand]
    private void SelectAnswer(string? answer)
    {
        SelectedAnswer = answer;
    }

    [RelayCommand]
    private void SubmitAnswer()
    {
        if (string.IsNullOrEmpty(SelectedAnswer) || _currentTriviaSet?.Results == null || CurrentQuestion == null)
            return;

        _userAnswers.Add(SelectedAnswer);

        // Check if answer is correct (both answers are already decoded)
        IsAnswerCorrect = string.Equals(CurrentQuestion.CorrectAnswer, SelectedAnswer, StringComparison.OrdinalIgnoreCase);
        
        if (IsAnswerCorrect)
        {
            CorrectAnswers++;
            FeedbackMessage = "🎉 Correct!";
        }
        else
        {
            FeedbackMessage = "❌ Incorrect";
            CorrectAnswerText = $"The correct answer was: {CurrentQuestion.CorrectAnswer}";
        }

        // Show feedback and hide submit button
        ShowAnswerFeedback = true;
        
        // Update progress
        OnPropertyChanged(nameof(ScorePercentage));
        UpdateProgressText();
        StatusMessage = $"Question {CurrentQuestionNumber} of {TotalQuestions} - {(IsAnswerCorrect ? "Correct!" : "Incorrect")}";
    }

    [RelayCommand]
    private void NextQuestion()
    {
        // Hide feedback
        ShowAnswerFeedback = false;
        FeedbackMessage = string.Empty;
        CorrectAnswerText = string.Empty;

        // Move to next question or finish quiz
        _currentQuestionIndex++;
        
        if (_currentQuestionIndex < _currentTriviaSet!.Results.Count)
        {
            CurrentQuestionNumber = _currentQuestionIndex + 1;
            LoadCurrentQuestion();
            UpdateProgressText();
            StatusMessage = $"Question {CurrentQuestionNumber} of {TotalQuestions}";
        }
        else
        {
            CompleteQuiz();
        }
    }

    [RelayCommand]
    private void RestartQuiz()
    {
        IsQuizActive = false;
        IsQuizComplete = false;
        ShowResults = false;
        ShowAnswerFeedback = false;
        IsAnswerCorrect = false;
        FeedbackMessage = string.Empty;
        CorrectAnswerText = string.Empty;
        CurrentQuestion = null;
        CurrentAnswerOptions.Clear();
        SelectedAnswer = null;
        _currentTriviaSet = null;
        _userAnswers.Clear();
        _currentQuestionIndex = 0;
        CurrentQuestionNumber = 0;
        TotalQuestions = 0;
        CorrectAnswers = 0;
        ElapsedTime = TimeSpan.Zero;
        
        UpdateProgressText();
        StatusMessage = "Ready to start a new quiz!";
    }

    private void LoadCurrentQuestion()
    {
        if (_currentTriviaSet?.Results == null || _currentQuestionIndex >= _currentTriviaSet.Results.Count)
            return;

        var question = _currentTriviaSet.Results[_currentQuestionIndex];
        
        // Create a new question with decoded text for display
        CurrentQuestion = new TriviaQuestion
        {
            Category = DecodeText(question.Category),
            Type = question.Type,
            Difficulty = question.Difficulty,
            Question = DecodeText(question.Question),
            CorrectAnswer = DecodeText(question.CorrectAnswer),
            IncorrectAnswers = question.IncorrectAnswers.Select(DecodeText).ToList()
        };

        // Prepare answer options with all answers decoded
        var options = new List<string> { CurrentQuestion.CorrectAnswer };
        options.AddRange(CurrentQuestion.IncorrectAnswers);
        
        // Shuffle the options
        var random = new Random();
        options = options.OrderBy(x => random.Next()).ToList();

        // Add shuffled options to display collection
        CurrentAnswerOptions.Clear();
        foreach (var option in options)
        {
            CurrentAnswerOptions.Add(option);
        }

        SelectedAnswer = null;
    }

    private static string DecodeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        // First decode HTML entities, then URL decode
        var htmlDecoded = System.Net.WebUtility.HtmlDecode(text);
        var urlDecoded = System.Net.WebUtility.UrlDecode(htmlDecoded);
        return urlDecoded;
    }

    private void CompleteQuiz()
    {
        IsQuizActive = false;
        IsQuizComplete = true;
        ShowResults = true;
        ShowAnswerFeedback = false; // Hide any lingering feedback
        ElapsedTime = DateTime.Now - _quizStartTime;
        
        // Notify UI that percentage has changed
        OnPropertyChanged(nameof(ScorePercentage));
        
        var percentage = ScorePercentage;
        StatusMessage = $"Quiz completed! You scored {CorrectAnswers}/{TotalQuestions} ({percentage:F1}%)";
        
        UpdateProgressText();
    }

    private void UpdateProgressText()
    {
        if (IsQuizActive)
        {
            var answeredQuestions = _userAnswers.Count;
            if (answeredQuestions > 0)
            {
                ProgressText = $"Question {CurrentQuestionNumber} of {TotalQuestions} - Score: {CorrectAnswers}/{answeredQuestions}";
            }
            else
            {
                ProgressText = $"Question {CurrentQuestionNumber} of {TotalQuestions}";
            }
        }
        else if (IsQuizComplete)
        {
            var percentage = ScorePercentage;
            ProgressText = $"Final Score: {CorrectAnswers}/{TotalQuestions} ({percentage:F1}%)";
        }
        else
        {
            ProgressText = "Quiz not started";
        }
    }

    // Property change handlers
    partial void OnSelectedAnswerChanged(string? value)
    {
        // Enable submit button when answer is selected
        OnPropertyChanged(nameof(CanSubmitAnswer));
    }

    public bool CanSubmitAnswer => !string.IsNullOrEmpty(SelectedAnswer) && IsQuizActive;
}
