using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using WpfEventRecorder.Core;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ObservableCollection<RecordEntryViewModel> _entries = new();
    private WindowInfo? _selectedWindow;

    public MainWindow()
    {
        InitializeComponent();

        EventsList.ItemsSource = _entries;

        // Subscribe to recording events
        WpfRecorder.RecordingStateChanged += OnRecordingStateChanged;
        WpfRecorder.EntryRecorded += OnEntryRecorded;

        UpdateUI();
    }

    private void OnRecordingStateChanged(object? sender, bool isRecording)
    {
        Dispatcher.Invoke(UpdateUI);
    }

    private void OnEntryRecorded(object? sender, RecordEntry entry)
    {
        Dispatcher.Invoke(() =>
        {
            _entries.Add(new RecordEntryViewModel(entry));
            EventCountText.Text = $" ({_entries.Count} events)";

            // Auto-scroll to latest
            if (_entries.Count > 0)
            {
                EventsList.ScrollIntoView(_entries[^1]);
            }
        });
    }

    private void UpdateUI()
    {
        var isRecording = WpfRecorder.IsRecording;
        var hasEntries = _entries.Count > 0;

        StartButton.IsEnabled = !isRecording;
        StopButton.IsEnabled = isRecording;
        SaveButton.IsEnabled = hasEntries;
        ClearButton.IsEnabled = hasEntries && !isRecording;

        if (isRecording)
        {
            StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
            StatusText.Text = "Recording";
            StatusBarText.Text = "Recording in progress...";
        }
        else if (hasEntries)
        {
            StatusIndicator.Fill = new SolidColorBrush(Colors.Green);
            StatusText.Text = "Stopped";
            StatusBarText.Text = $"Recording stopped - {_entries.Count} events captured";
        }
        else
        {
            StatusIndicator.Fill = new SolidColorBrush(Colors.Gray);
            StatusText.Text = "Ready";
            StatusBarText.Text = "Ready to record";
        }

        EventCountText.Text = $" ({_entries.Count} events)";
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        // Show window selector dialog
        var dialog = new WindowSelectorDialog { Owner = this };
        if (dialog.ShowDialog() == true && dialog.SelectedWindow != null)
        {
            _selectedWindow = dialog.SelectedWindow;

            // Update target window indicator
            TargetWindowText.Text = _selectedWindow.DisplayName;
            TargetWindowBorder.Visibility = Visibility.Visible;

            // Start recording with target window info
            WpfRecorder.Start(_selectedWindow, $"Session_{DateTime.Now:yyyyMMdd_HHmmss}");
            UpdateUI();
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        WpfRecorder.Stop();
        _selectedWindow = null;
        TargetWindowBorder.Visibility = Visibility.Collapsed;
        UpdateUI();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to clear all recorded events?",
            "Clear Recording",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            WpfRecorder.Clear();
            _entries.Clear();
            DetailsTextBox.Text = string.Empty;
            BodyTextBox.Text = string.Empty;
            UpdateUI();
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            DefaultExt = ".json",
            FileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.json",
            Title = "Save Recording"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                WpfRecorder.SaveToFile(dialog.FileName);
                MessageBox.Show($"Recording saved to:\n{dialog.FileName}",
                    "Save Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                StatusBarText.Text = $"Saved to {dialog.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving recording:\n{ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void EventsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EventsList.SelectedItem is RecordEntryViewModel vm)
        {
            DetailsTextBox.Text = vm.Details;
            BodyTextBox.Text = vm.IsApiEvent
                ? (vm.Entry.EntryType == RecordEntryType.ApiRequest ? vm.RequestBody : vm.ResponseBody) ?? "(no body)"
                : "(N/A for UI events)";
        }
        else
        {
            DetailsTextBox.Text = string.Empty;
            BodyTextBox.Text = string.Empty;
        }
    }
}

/// <summary>
/// View model for displaying RecordEntry in the list
/// </summary>
public class RecordEntryViewModel
{
    public RecordEntry Entry { get; }

    public RecordEntryViewModel(RecordEntry entry)
    {
        Entry = entry;
    }

    public string TimeString => Entry.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff");

    public string TypeString => Entry.EntryType.ToString();

    public bool IsApiEvent => Entry.EntryType is RecordEntryType.ApiRequest or RecordEntryType.ApiResponse;

    public string Summary
    {
        get
        {
            if (Entry.UIInfo != null)
            {
                var ui = Entry.UIInfo;
                return $"{ui.ControlType}: {ui.ControlName ?? ui.Text ?? "(unnamed)"}";
            }

            if (Entry.ApiInfo != null)
            {
                var api = Entry.ApiInfo;
                var status = api.StatusCode.HasValue ? $" [{api.StatusCode}]" : "";
                return $"{api.Method} {api.Path ?? api.Url}{status}";
            }

            return Entry.EntryType.ToString();
        }
    }

    public string Details
    {
        get
        {
            if (Entry.UIInfo != null)
            {
                var ui = Entry.UIInfo;
                return $"""
                    Type: {Entry.EntryType}
                    Control Type: {ui.ControlType}
                    Control Name: {ui.ControlName ?? "(none)"}
                    Automation ID: {ui.AutomationId ?? "(none)"}
                    Text: {ui.Text ?? "(none)"}
                    Old Value: {ui.OldValue ?? "(none)"}
                    New Value: {ui.NewValue ?? "(none)"}
                    Window: {ui.WindowTitle ?? "(none)"}
                    Timestamp: {Entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}
                    Correlation ID: {Entry.CorrelationId ?? "(none)"}
                    """;
            }

            if (Entry.ApiInfo != null)
            {
                var api = Entry.ApiInfo;
                return $"""
                    Type: {Entry.EntryType}
                    Method: {api.Method}
                    URL: {api.Url}
                    Path: {api.Path ?? "(none)"}
                    Status: {api.StatusCode?.ToString() ?? "(pending)"}
                    Success: {api.IsSuccess}
                    Duration: {Entry.DurationMs?.ToString() ?? "N/A"} ms
                    Request Content-Type: {api.RequestContentType ?? "(none)"}
                    Response Content-Type: {api.ResponseContentType ?? "(none)"}
                    Timestamp: {Entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}
                    Correlation ID: {Entry.CorrelationId ?? "(none)"}
                    Error: {api.ErrorMessage ?? "(none)"}
                    """;
            }

            return $"Type: {Entry.EntryType}\nTimestamp: {Entry.Timestamp}";
        }
    }

    public string? RequestBody => Entry.ApiInfo?.RequestBody;
    public string? ResponseBody => Entry.ApiInfo?.ResponseBody;
}
