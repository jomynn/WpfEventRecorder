using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfEventRecorder.Extension.Converters;

/// <summary>
/// Converts an event type to a color.
/// </summary>
public class EventTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string eventType)
        {
            return eventType switch
            {
                "Input" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),      // Green
                "Command" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),   // Blue
                "ApiCall" => new SolidColorBrush(Color.FromRgb(156, 39, 176)),   // Purple
                "Navigation" => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                "Window" => new SolidColorBrush(Color.FromRgb(96, 125, 139)),    // Grey-Blue
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))           // Grey
            };
        }

        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
