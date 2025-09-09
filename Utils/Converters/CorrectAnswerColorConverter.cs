using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Global_Insights_Dashboard.Utils.Converters;

public class CorrectAnswerColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCorrect)
        {
            return isCorrect ? Brushes.Green : Brushes.Red;
        }
        
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
