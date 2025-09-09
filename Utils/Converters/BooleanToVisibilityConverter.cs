using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Global_Insights_Dashboard.Utils.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        // Handle nullable bool
        if (value is bool nullableBool2)
        {
            return nullableBool2 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Handle integers (for counts)
        if (value is int intValue)
        {
            return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Handle strings
        if (value is string stringValue)
        {
            return !string.IsNullOrEmpty(stringValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }

        return false;
    }
}
