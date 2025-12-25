using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;
using WpfEventRecorder.Core;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;

namespace WpfEventRecorder.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ObservableCollection<RecordEntryViewModel> _entries = new();
    private readonly ObservableCollection<VisualTreeNode> _visualTreeRoot = new();
    private readonly ObservableCollection<PropertyItem> _selectedNodeProperties = new();
    private WindowInfo? _selectedWindow;

    public MainWindow()
    {
        InitializeComponent();

        EventsList.ItemsSource = _entries;
        VisualTreeView.ItemsSource = _visualTreeRoot;

        // Setup property list with grouping
        var propertyView = CollectionViewSource.GetDefaultView(_selectedNodeProperties);
        propertyView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
        PropertyList.ItemsSource = propertyView;

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
        var selectedCount = _entries.Count(e => e.IsSelectedForExport);
        var hasSelectedEntries = selectedCount > 0;

        StartButton.IsEnabled = !isRecording;
        StopButton.IsEnabled = isRecording;
        SaveButton.IsEnabled = hasEntries;
        ClearButton.IsEnabled = hasEntries && !isRecording;

        // Enable/disable export and selection buttons
        SaveCsvButton.IsEnabled = hasSelectedEntries;
        SaveExcelButton.IsEnabled = hasSelectedEntries;
        SelectAllButton.IsEnabled = hasEntries;
        DeselectAllButton.IsEnabled = hasEntries;

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
        SelectedCountText.Text = selectedCount.ToString();
    }

    private void UpdateSelectedCount()
    {
        var count = _entries.Count(e => e.IsSelectedForExport);
        SelectedCountText.Text = count.ToString();
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
        // Keep _selectedWindow for Visual Tree browsing after recording stops
        // Only hide the indicator but keep the window reference
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

            // Clear visual tree and properties
            _visualTreeRoot.Clear();
            _selectedNodeProperties.Clear();
            PropertyHeader.Text = "Select an element from Visual Tree";

            // Clear selected window
            _selectedWindow = null;
            TargetWindowBorder.Visibility = Visibility.Collapsed;

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

    private void CheckBox_Click(object sender, RoutedEventArgs e)
    {
        UpdateSelectedCount();
        UpdateUI();
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var entry in _entries)
        {
            entry.IsSelectedForExport = true;
        }
        SelectAllCheckBox.IsChecked = true;
        UpdateSelectedCount();
        UpdateUI();
    }

    private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var entry in _entries)
        {
            entry.IsSelectedForExport = false;
        }
        SelectAllCheckBox.IsChecked = false;
        UpdateSelectedCount();
        UpdateUI();
    }

    private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            var isChecked = checkBox.IsChecked ?? false;
            foreach (var entry in _entries)
            {
                entry.IsSelectedForExport = isChecked;
            }
            UpdateSelectedCount();
            UpdateUI();
        }
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            DefaultExt = ".json",
            Title = "Load Recording"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = File.ReadAllText(dialog.FileName);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<RecordEntry>? entries = null;

                // Try to parse as RecordingSession first (the format SaveButton uses)
                try
                {
                    var session = JsonSerializer.Deserialize<RecordingSession>(json, options);
                    if (session?.Entries != null && session.Entries.Count > 0)
                    {
                        entries = session.Entries;
                    }
                }
                catch
                {
                    // If that fails, try as simple List<RecordEntry>
                }

                // If session parsing didn't work, try as plain list
                if (entries == null)
                {
                    entries = JsonSerializer.Deserialize<List<RecordEntry>>(json, options);
                }

                if (entries != null && entries.Count > 0)
                {
                    // Ask if user wants to append or replace
                    MessageBoxResult result = MessageBoxResult.Yes;
                    if (_entries.Count > 0)
                    {
                        result = MessageBox.Show(
                            $"Found {entries.Count} entries in the file.\n\nDo you want to replace existing entries?\n\nYes = Replace existing\nNo = Append to existing\nCancel = Cancel load",
                            "Load Recording",
                            MessageBoxButton.YesNoCancel,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Cancel)
                            return;
                    }

                    if (result == MessageBoxResult.Yes)
                    {
                        _entries.Clear();
                        WpfRecorder.Clear();
                    }

                    foreach (var entry in entries)
                    {
                        _entries.Add(new RecordEntryViewModel(entry));
                    }

                    // Update header checkbox state
                    SelectAllCheckBox.IsChecked = _entries.All(x => x.IsSelectedForExport);

                    UpdateSelectedCount();
                    UpdateUI();

                    MessageBox.Show($"Loaded {entries.Count} entries from {dialog.FileName}",
                                    "Load Successful",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No entries found in the file.",
                                    "Load Recording",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recording: {ex.Message}",
                                "Load Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }

    private void SaveCsvButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedEntries = _entries.Where(e => e.IsSelectedForExport).Select(e => e.Entry).ToList();
        if (selectedEntries.Count == 0)
        {
            MessageBox.Show("No entries selected for export.",
                            "Export",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            DefaultExt = ".csv",
            FileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            Title = "Export to CSV"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                ExportToCsv(selectedEntries, dialog.FileName);
                MessageBox.Show($"Exported {selectedEntries.Count} entries to {dialog.FileName}",
                                "Export Successful",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}",
                                "Export Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }

    private void SaveExcelButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedEntries = _entries.Where(e => e.IsSelectedForExport).Select(e => e.Entry).ToList();
        if (selectedEntries.Count == 0)
        {
            MessageBox.Show("No entries selected for export.",
                            "Export",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Excel XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            DefaultExt = ".xml",
            FileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.xml",
            Title = "Export to Excel"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                ExportToExcel(selectedEntries, dialog.FileName);
                MessageBox.Show($"Exported {selectedEntries.Count} entries to {dialog.FileName}\n\nNote: Open with Microsoft Excel to view the spreadsheet.",
                                "Export Successful",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}",
                                "Export Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }

    private static void ExportToCsv(IEnumerable<RecordEntry> entries, string filePath)
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine("Timestamp,Type,ControlType,ControlName,AutomationId,Text,ContentText,OldValue,NewValue,WindowTitle,VisualTreePath,ScreenX,ScreenY,KeyCombination,Properties,Method,URL,StatusCode,Duration,CorrelationId");

        foreach (var entry in entries)
        {
            var timestamp = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var type = entry.EntryType.ToString();
            var controlType = EscapeCsv(entry.UIInfo?.ControlType ?? "");
            var controlName = EscapeCsv(entry.UIInfo?.ControlName ?? "");
            var automationId = EscapeCsv(entry.UIInfo?.AutomationId ?? "");
            var text = EscapeCsv(entry.UIInfo?.Text ?? "");
            var contentText = EscapeCsv(entry.UIInfo?.ContentText ?? "");
            var oldValue = EscapeCsv(entry.UIInfo?.OldValue ?? "");
            var newValue = EscapeCsv(entry.UIInfo?.NewValue ?? "");
            var windowTitle = EscapeCsv(entry.UIInfo?.WindowTitle ?? "");
            var visualTreePath = EscapeCsv(entry.UIInfo?.VisualTreePath ?? "");
            var screenX = entry.UIInfo?.ScreenPosition?.X.ToString() ?? "";
            var screenY = entry.UIInfo?.ScreenPosition?.Y.ToString() ?? "";
            var keyCombination = EscapeCsv(entry.UIInfo?.KeyCombination ?? "");
            var properties = EscapeCsv(FormatProperties(entry.UIInfo?.Properties));
            var method = EscapeCsv(entry.ApiInfo?.Method ?? "");
            var url = EscapeCsv(entry.ApiInfo?.Url ?? "");
            var statusCode = entry.ApiInfo?.StatusCode?.ToString() ?? "";
            var duration = entry.DurationMs?.ToString() ?? "";
            var correlationId = EscapeCsv(entry.CorrelationId ?? "");

            sb.AppendLine($"{timestamp},{type},{controlType},{controlName},{automationId},{text},{contentText},{oldValue},{newValue},{windowTitle},{visualTreePath},{screenX},{screenY},{keyCombination},{properties},{method},{url},{statusCode},{duration},{correlationId}");
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private static string FormatProperties(Dictionary<string, string>? properties)
    {
        if (properties == null || properties.Count == 0)
            return "";

        return string.Join("; ", properties.Select(p => $"{p.Key}={p.Value}"));
    }

    private static void ExportToExcel(IEnumerable<RecordEntry> entries, string filePath)
    {
        var entryList = entries.ToList();
        var sb = new StringBuilder();

        // Excel XML header
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine("          xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");

        // Styles
        sb.AppendLine("  <Styles>");
        sb.AppendLine("    <Style ss:ID=\"Header\">");
        sb.AppendLine("      <Font ss:Bold=\"1\"/>");
        sb.AppendLine("      <Interior ss:Color=\"#CCCCCC\" ss:Pattern=\"Solid\"/>");
        sb.AppendLine("    </Style>");
        sb.AppendLine("  </Styles>");

        // Worksheet
        sb.AppendLine("  <Worksheet ss:Name=\"Recorded Events\">");
        sb.AppendLine($"    <Table ss:ExpandedColumnCount=\"20\" ss:ExpandedRowCount=\"{entryList.Count + 1}\">");

        // Header row
        sb.AppendLine("      <Row>");
        WriteExcelCell(sb, "Timestamp", "Header");
        WriteExcelCell(sb, "Type", "Header");
        WriteExcelCell(sb, "ControlType", "Header");
        WriteExcelCell(sb, "ControlName", "Header");
        WriteExcelCell(sb, "AutomationId", "Header");
        WriteExcelCell(sb, "Text", "Header");
        WriteExcelCell(sb, "ContentText", "Header");
        WriteExcelCell(sb, "OldValue", "Header");
        WriteExcelCell(sb, "NewValue", "Header");
        WriteExcelCell(sb, "WindowTitle", "Header");
        WriteExcelCell(sb, "VisualTreePath", "Header");
        WriteExcelCell(sb, "ScreenX", "Header");
        WriteExcelCell(sb, "ScreenY", "Header");
        WriteExcelCell(sb, "KeyCombination", "Header");
        WriteExcelCell(sb, "Properties", "Header");
        WriteExcelCell(sb, "Method", "Header");
        WriteExcelCell(sb, "URL", "Header");
        WriteExcelCell(sb, "StatusCode", "Header");
        WriteExcelCell(sb, "Duration", "Header");
        WriteExcelCell(sb, "CorrelationId", "Header");
        sb.AppendLine("      </Row>");

        // Data rows
        foreach (var entry in entryList)
        {
            sb.AppendLine("      <Row>");
            WriteExcelCell(sb, entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            WriteExcelCell(sb, entry.EntryType.ToString());
            WriteExcelCell(sb, entry.UIInfo?.ControlType ?? "");
            WriteExcelCell(sb, entry.UIInfo?.ControlName ?? "");
            WriteExcelCell(sb, entry.UIInfo?.AutomationId ?? "");
            WriteExcelCell(sb, entry.UIInfo?.Text ?? "");
            WriteExcelCell(sb, entry.UIInfo?.ContentText ?? "");
            WriteExcelCell(sb, entry.UIInfo?.OldValue ?? "");
            WriteExcelCell(sb, entry.UIInfo?.NewValue ?? "");
            WriteExcelCell(sb, entry.UIInfo?.WindowTitle ?? "");
            WriteExcelCell(sb, entry.UIInfo?.VisualTreePath ?? "");
            WriteExcelCell(sb, entry.UIInfo?.ScreenPosition?.X.ToString() ?? "");
            WriteExcelCell(sb, entry.UIInfo?.ScreenPosition?.Y.ToString() ?? "");
            WriteExcelCell(sb, entry.UIInfo?.KeyCombination ?? "");
            WriteExcelCell(sb, FormatProperties(entry.UIInfo?.Properties));
            WriteExcelCell(sb, entry.ApiInfo?.Method ?? "");
            WriteExcelCell(sb, entry.ApiInfo?.Url ?? "");
            WriteExcelCell(sb, entry.ApiInfo?.StatusCode?.ToString() ?? "");
            WriteExcelCell(sb, entry.DurationMs?.ToString() ?? "");
            WriteExcelCell(sb, entry.CorrelationId ?? "");
            sb.AppendLine("      </Row>");
        }

        sb.AppendLine("    </Table>");
        sb.AppendLine("  </Worksheet>");
        sb.AppendLine("</Workbook>");

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private static void WriteExcelCell(StringBuilder sb, string value, string? style = null)
    {
        var escapedValue = EscapeXml(value);
        var styleAttr = style != null ? $" ss:StyleID=\"{style}\"" : "";
        sb.AppendLine($"        <Cell{styleAttr}><Data ss:Type=\"String\">{escapedValue}</Data></Cell>");
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    private static string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    #region Visual Tree Tab

    private void RefreshTree_Click(object sender, RoutedEventArgs e)
    {
        _visualTreeRoot.Clear();
        _selectedNodeProperties.Clear();
        PropertyHeader.Text = "Select an element from Visual Tree";

        if (_selectedWindow == null || _selectedWindow.WindowHandle == IntPtr.Zero)
        {
            MessageBox.Show("No target window selected. Start recording first to select a window.",
                            "Visual Tree",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
            return;
        }

        try
        {
            var rootNode = LiveTreeService.BuildVisualTree(_selectedWindow.WindowHandle);
            if (rootNode != null)
            {
                _visualTreeRoot.Add(rootNode);
                StatusBarText.Text = $"Visual tree refreshed - {CountNodes(rootNode)} elements found";
            }
            else
            {
                MessageBox.Show("Could not build visual tree. The target window may have been closed.",
                                "Visual Tree",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error building visual tree: {ex.Message}",
                            "Visual Tree Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }

    private static int CountNodes(VisualTreeNode node)
    {
        int count = 1;
        foreach (var child in node.Children)
        {
            count += CountNodes(child);
        }
        return count;
    }

    private void ExpandAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var node in _visualTreeRoot)
        {
            node.ExpandAll();
        }
    }

    private void CollapseAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var node in _visualTreeRoot)
        {
            node.CollapseAll();
        }
    }

    private void VisualTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selectedNodeProperties.Clear();

        if (e.NewValue is VisualTreeNode selectedNode)
        {
            PropertyHeader.Text = $"{selectedNode.ControlType} {selectedNode.DisplayText}";

            var properties = LiveTreeService.GetElementProperties(selectedNode);
            foreach (var prop in properties)
            {
                _selectedNodeProperties.Add(prop);
            }
        }
        else
        {
            PropertyHeader.Text = "Select an element from Visual Tree";
        }
    }

    #endregion
}

/// <summary>
/// View model for displaying RecordEntry in the list
/// </summary>
public class RecordEntryViewModel : INotifyPropertyChanged
{
    private bool _isSelectedForExport = true;

    public RecordEntry Entry { get; }

    public RecordEntryViewModel(RecordEntry entry)
    {
        Entry = entry;
    }

    /// <summary>
    /// Whether this entry is selected for export
    /// </summary>
    public bool IsSelectedForExport
    {
        get => _isSelectedForExport;
        set
        {
            if (_isSelectedForExport != value)
            {
                _isSelectedForExport = value;
                OnPropertyChanged();
            }
        }
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
