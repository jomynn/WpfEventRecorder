using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;
using WpfEventRecorder.Services;

namespace WpfEventRecorder.ToolWindows
{
    /// <summary>
    /// Interaction logic for RecorderToolWindowControl.xaml
    /// </summary>
    public partial class RecorderToolWindowControl : UserControl
    {
        private readonly ObservableCollection<RecordEntryViewModel> _entries;

        public RecorderToolWindowControl()
        {
            InitializeComponent();

            _entries = new ObservableCollection<RecordEntryViewModel>();
            EventsList.ItemsSource = _entries;

            // Subscribe to recording events
            RecordingHub.Instance.RecordingStateChanged += OnRecordingStateChanged;
            RecordingHub.Instance.EntryRecorded += OnEntryRecorded;

            UpdateUI();
        }

        private void OnRecordingStateChanged(object? sender, bool isRecording)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUI();
            });
        }

        private void OnEntryRecorded(object? sender, RecordEntry entry)
        {
            Dispatcher.Invoke(() =>
            {
                _entries.Add(new RecordEntryViewModel(entry));
                EventCountText.Text = _entries.Count.ToString();
                UpdateSelectedCount();
                UpdateUI();

                // Auto-scroll to latest entry
                if (_entries.Count > 0)
                {
                    EventsList.ScrollIntoView(_entries[_entries.Count - 1]);
                }
            });
        }

        private void UpdateUI()
        {
            var isRecording = RecordingHub.Instance.IsRecording;
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
                StatusText.Text = "Recording...";
            }
            else if (hasEntries)
            {
                StatusIndicator.Fill = new SolidColorBrush(Colors.Green);
                StatusText.Text = "Stopped";
            }
            else
            {
                StatusIndicator.Fill = new SolidColorBrush(Colors.Gray);
                StatusText.Text = "Ready";
            }

            EventCountText.Text = _entries.Count.ToString();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            RecordingHub.Instance.Start();
            UpdateUI();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            RecordingHub.Instance.Stop();
            UpdateUI();
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
                    RecordingHub.Instance.SaveToFile(dialog.FileName);
                    MessageBox.Show($"Recording saved to {dialog.FileName}",
                                    "Save Successful",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving recording: {ex.Message}",
                                    "Save Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
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
                RecordingHub.Instance.Clear();
                _entries.Clear();
                DetailsTextBox.Text = string.Empty;
                BodyTextBox.Text = string.Empty;
                UpdateUI();
            }
        }

        private void EventsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventsList.SelectedItem is RecordEntryViewModel viewModel)
            {
                DetailsTextBox.Text = viewModel.Details;

                if (viewModel.IsApiEvent)
                {
                    var body = viewModel.Entry.EntryType == RecordEntryType.ApiRequest
                        ? viewModel.RequestBody
                        : viewModel.ResponseBody;
                    BodyTextBox.Text = body ?? "(no body)";
                }
                else
                {
                    BodyTextBox.Text = "(N/A for UI events)";
                }
            }
            else
            {
                DetailsTextBox.Text = string.Empty;
                BodyTextBox.Text = string.Empty;
            }
        }

        private void UpdateSelectedCount()
        {
            var count = _entries.Count(e => e.IsSelectedForExport);
            SelectedCountText.Text = count.ToString();
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
            UpdateSelectedCount();
            UpdateUI();
        }

        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var entry in _entries)
            {
                entry.IsSelectedForExport = false;
            }
            UpdateSelectedCount();
            UpdateUI();
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
                    ExportService.ExportToCsv(selectedEntries, dialog.FileName);
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
                    ExportService.ExportToExcel(selectedEntries, dialog.FileName);
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
    }
}
