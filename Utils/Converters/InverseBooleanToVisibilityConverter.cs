using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Global_Insights_Dashboard.Utils.Converters;

public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        // Handle nullable bool
        if (value is bool nullableBool2)
        {
            return nullableBool2 ? Visibility.Collapsed : Visibility.Visible;
        }

        // Handle integers (for counts)
        if (value is int intValue)
        {
            return intValue > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // Handle strings
        if (value is string stringValue)
        {
            return !string.IsNullOrEmpty(stringValue) ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Collapsed;
        }

        return true;
    }
}
