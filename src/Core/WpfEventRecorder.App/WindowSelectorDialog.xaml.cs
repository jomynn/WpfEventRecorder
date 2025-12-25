using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;

namespace WpfEventRecorder.App;

/// <summary>
/// Dialog for selecting a window to record
/// </summary>
public partial class WindowSelectorDialog : Window
{
    private readonly ObservableCollection<WindowInfo> _windows = new();

    /// <summary>
    /// The selected window info
    /// </summary>
    public WindowInfo? SelectedWindow { get; private set; }

    public WindowSelectorDialog()
    {
        InitializeComponent();
        WindowList.ItemsSource = _windows;
        WindowList.SelectionChanged += WindowList_SelectionChanged;

        Loaded += (s, e) => RefreshWindowList();
    }

    private void RefreshWindowList()
    {
        _windows.Clear();

        // Check if window enumeration is supported on this platform
        if (!WindowEnumerator.IsSupported)
        {
            SelectedWindowText.Text = "(Window enumeration is only supported on Windows)";
            SelectedWindowText.Foreground = new SolidColorBrush(Colors.Red);
            return;
        }

        var wpfOnly = WpfOnlyCheckBox.IsChecked == true;
        var windows = WindowEnumerator.Refresh(wpfOnly);

        // Exclude our own window
        var currentProcessId = Environment.ProcessId;
        foreach (var window in windows)
        {
            if (window.ProcessId != currentProcessId)
            {
                _windows.Add(window);
            }
        }

        if (_windows.Count == 0)
        {
            SelectedWindowText.Text = wpfOnly
                ? "(No WPF applications found. Try unchecking 'Show WPF apps only')"
                : "(No windows found)";
        }
    }

    private void WindowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedWindow = WindowList.SelectedItem as WindowInfo;
        SelectButton.IsEnabled = selectedWindow != null;
        BringToFrontButton.IsEnabled = selectedWindow != null;

        if (selectedWindow != null)
        {
            SelectedWindowText.Text = $"{selectedWindow.WindowTitle}\n" +
                                      $"Process: {selectedWindow.ProcessName} (PID: {selectedWindow.ProcessId})\n" +
                                      $"WPF: {(selectedWindow.IsWpfApp ? "Yes" : "No")}";
            SelectedWindowText.Foreground = new SolidColorBrush(Colors.Black);
        }
        else
        {
            SelectedWindowText.Text = "(none selected)";
            SelectedWindowText.Foreground = new SolidColorBrush(Colors.Gray);
        }
    }

    private void WindowList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (WindowList.SelectedItem is WindowInfo)
        {
            SelectButton_Click(sender, e);
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshWindowList();
    }

    private void WpfOnlyCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        RefreshWindowList();
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedWindow = WindowList.SelectedItem as WindowInfo;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void BringToFrontButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowList.SelectedItem is WindowInfo selectedWindow)
        {
            var success = WindowEnumerator.BringToFront(selectedWindow);
            if (success)
            {
                // Refresh the list to update window titles (minimized status may have changed)
                RefreshWindowList();

                // Re-select the same window if it's still in the list
                foreach (var window in _windows)
                {
                    if (window.WindowHandle == selectedWindow.WindowHandle)
                    {
                        WindowList.SelectedItem = window;
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Could not bring the window to the foreground.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

/// <summary>
/// Converts boolean to Yes/No string
/// </summary>
public class BoolToYesNoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? "Yes" : "No";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to color (green for true, gray for false)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b
            ? new SolidColorBrush(Colors.Green)
            : new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
