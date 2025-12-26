using System.Globalization;
using System.Windows.Data;

namespace WpfEventRecorder.Extension.Converters;

/// <summary>
/// Converts a TimeSpan to a formatted string.
/// </summary>
public class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return timeSpan.ToString(@"hh\:mm\:ss");
            }

            return timeSpan.ToString(@"mm\:ss\.f");
        }

        return "00:00.0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && TimeSpan.TryParse(str, out var result))
        {
            return result;
        }

        return TimeSpan.Zero;
    }
}
