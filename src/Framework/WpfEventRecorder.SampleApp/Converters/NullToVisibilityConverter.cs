using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfEventRecorder.SampleApp.Converters
{
    /// <summary>
    /// Converts null/empty string to Visibility
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts null to Collapsed, non-null to Visible
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Inverts a boolean value
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Inverts the boolean value
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return value;
        }

        /// <summary>
        /// Inverts the boolean value back
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return value;
        }
    }

    /// <summary>
    /// Converts boolean to Visibility
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts true to Visible, false to Collapsed
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts Visibility to boolean
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }
}
