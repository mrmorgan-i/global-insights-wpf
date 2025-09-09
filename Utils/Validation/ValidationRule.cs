using System.Text.RegularExpressions;

namespace Global_Insights_Dashboard.Utils.Validation;

/// <summary>
/// Base class for input validation rules
/// </summary>
public abstract class ValidationRule
{
    public abstract ValidationResult Validate(string? input);
}

/// <summary>
/// Validation result containing success state and error message
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Error(string message) => new() { IsValid = false, ErrorMessage = message };
}

/// <summary>
/// Validates that input is not null or empty
/// </summary>
public class RequiredValidationRule : ValidationRule
{
    private readonly string _fieldName;

    public RequiredValidationRule(string fieldName = "Field")
    {
        _fieldName = fieldName;
    }

    public override ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Error($"{_fieldName} is required");

        return ValidationResult.Success();
    }
}

/// <summary>
/// Validates input length constraints
/// </summary>
public class LengthValidationRule : ValidationRule
{
    private readonly int _minLength;
    private readonly int _maxLength;
    private readonly string _fieldName;

    public LengthValidationRule(int minLength, int maxLength, string fieldName = "Field")
    {
        _minLength = minLength;
        _maxLength = maxLength;
        _fieldName = fieldName;
    }

    public override ValidationResult Validate(string? input)
    {
        if (input == null) return ValidationResult.Success(); // Let RequiredValidationRule handle null

        if (input.Length < _minLength)
            return ValidationResult.Error($"{_fieldName} must be at least {_minLength} characters");

        if (input.Length > _maxLength)
            return ValidationResult.Error($"{_fieldName} must be no more than {_maxLength} characters");

        return ValidationResult.Success();
    }
}

/// <summary>
/// Validates input against a regular expression pattern
/// </summary>
public class RegexValidationRule : ValidationRule
{
    private readonly Regex _regex;
    private readonly string _errorMessage;

    public RegexValidationRule(string pattern, string errorMessage, RegexOptions options = RegexOptions.None)
    {
        _regex = new Regex(pattern, options);
        _errorMessage = errorMessage;
    }

    public override ValidationResult Validate(string? input)
    {
        if (input == null) return ValidationResult.Success(); // Let RequiredValidationRule handle null

        if (!_regex.IsMatch(input))
            return ValidationResult.Error(_errorMessage);

        return ValidationResult.Success();
    }
}

/// <summary>
/// Validates numeric input within a specified range
/// </summary>
public class NumericRangeValidationRule : ValidationRule
{
    private readonly int _min;
    private readonly int _max;
    private readonly string _fieldName;

    public NumericRangeValidationRule(int min, int max, string fieldName = "Value")
    {
        _min = min;
        _max = max;
        _fieldName = fieldName;
    }

    public override ValidationResult Validate(string? input)
    {
        if (input == null) return ValidationResult.Success(); // Let RequiredValidationRule handle null

        if (!int.TryParse(input, out int value))
            return ValidationResult.Error($"{_fieldName} must be a valid number");

        if (value < _min || value > _max)
            return ValidationResult.Error($"{_fieldName} must be between {_min} and {_max}");

        return ValidationResult.Success();
    }
}

/// <summary>
/// Composite validator that runs multiple validation rules
/// </summary>
public class InputValidator
{
    private readonly List<ValidationRule> _rules = new();

    public InputValidator AddRule(ValidationRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    public ValidationResult Validate(string? input)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Validate(input);
            if (!result.IsValid)
                return result;
        }

        return ValidationResult.Success();
    }
}

/// <summary>
/// Pre-built validators for common use cases
/// </summary>
public static class CommonValidators
{
    /// <summary>
    /// Validator for city names (required, 1-100 characters, letters/spaces/hyphens/apostrophes only)
    /// </summary>
    public static InputValidator CityName => new InputValidator()
        .AddRule(new RequiredValidationRule("City name"))
        .AddRule(new LengthValidationRule(1, 100, "City name"))
        .AddRule(new RegexValidationRule(@"^[a-zA-Z\s\-'\.]+$", "City name can only contain letters, spaces, hyphens, apostrophes, and periods"));

    /// <summary>
    /// Validator for country codes (optional, 2-3 characters, letters only)
    /// </summary>
    public static InputValidator CountryCode => new InputValidator()
        .AddRule(new LengthValidationRule(0, 3, "Country code"))
        .AddRule(new RegexValidationRule(@"^[a-zA-Z]*$", "Country code can only contain letters"));

    /// <summary>
    /// Validator for stock symbols (required, 1-5 characters, uppercase letters only)
    /// </summary>
    public static InputValidator StockSymbol => new InputValidator()
        .AddRule(new RequiredValidationRule("Stock symbol"))
        .AddRule(new LengthValidationRule(1, 5, "Stock symbol"))
        .AddRule(new RegexValidationRule(@"^[A-Z]+$", "Stock symbol must contain only uppercase letters"));

    /// <summary>
    /// Validator for trivia question count (required, 1-50)
    /// </summary>
    public static InputValidator TriviaQuestionCount => new InputValidator()
        .AddRule(new RequiredValidationRule("Number of questions"))
        .AddRule(new NumericRangeValidationRule(1, 50, "Number of questions"));
}
