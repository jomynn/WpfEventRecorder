using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;

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

            StartButton.IsEnabled = !isRecording;
            StopButton.IsEnabled = isRecording;
            SaveButton.IsEnabled = hasEntries;
            ClearButton.IsEnabled = hasEntries && !isRecording;

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
    }
}
