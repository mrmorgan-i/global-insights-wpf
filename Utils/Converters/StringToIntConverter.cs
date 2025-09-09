using System.Globalization;
using System.Windows.Data;
using Global_Insights_Dashboard.Utils.Validation;

namespace Global_Insights_Dashboard.Utils.Converters;

public class StringToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue.ToString();
        }
        
        return "10"; // Default fallback
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            // Use our validation framework
            var validation = CommonValidators.TriviaQuestionCount.Validate(stringValue);
            
            if (validation.IsValid && int.TryParse(stringValue, out int result))
            {
                return result;
            }
            
            // For invalid input, return the default value
            // The validation error will be shown elsewhere in the UI
            return string.IsNullOrWhiteSpace(stringValue) ? 10 : 10;
        }
        
        return 10; // Default fallback
    }
}
