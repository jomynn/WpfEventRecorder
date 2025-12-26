using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfEventRecorder.Extension.Converters;

/// <summary>
/// Converts a boolean value to Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            var invert = parameter?.ToString()?.ToLower() == "invert";
            if (invert) boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            var result = visibility == Visibility.Visible;

            var invert = parameter?.ToString()?.ToLower() == "invert";
            if (invert) result = !result;

            return result;
        }

        return false;
    }
}
