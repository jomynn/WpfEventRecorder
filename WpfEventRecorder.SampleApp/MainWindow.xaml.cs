using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WpfEventRecorder.Core;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.SampleApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient? _httpClient;

        public MainWindow()
        {
            InitializeComponent();

            // Create HTTP client with recording handler
            _httpClient = WpfRecorder.CreateHttpClient();

            // Subscribe to recording events
            WpfRecorder.EntryRecorded += OnEntryRecorded;
            WpfRecorder.RecordingStateChanged += OnRecordingStateChanged;

            UpdateRecordingUI();
        }

        private void OnEntryRecorded(object? sender, RecordEntry e)
        {
            Dispatcher.Invoke(() =>
            {
                EventCountText.Text = $"Events: {WpfRecorder.EntryCount}";
            });
        }

        private void OnRecordingStateChanged(object? sender, bool isRecording)
        {
            Dispatcher.Invoke(UpdateRecordingUI);
        }

        private void UpdateRecordingUI()
        {
            var isRecording = WpfRecorder.IsRecording;
            StartRecordingBtn.IsEnabled = !isRecording;
            StopRecordingBtn.IsEnabled = isRecording;
            RecordingStatus.Text = isRecording ? " (Recording...)" : " (Stopped)";
            RecordingStatus.Foreground = isRecording
                ? System.Windows.Media.Brushes.Red
                : System.Windows.Media.Brushes.Gray;
            EventCountText.Text = $"Events: {WpfRecorder.EntryCount}";
        }

        private void StartRecordingBtn_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.Start();
            StatusText.Text = "Recording started";
            UpdateRecordingUI();
        }

        private void StopRecordingBtn_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.Stop();
            StatusText.Text = $"Recording stopped - {WpfRecorder.EntryCount} events captured";
            UpdateRecordingUI();
        }

        private void SaveRecordingBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".json",
                FileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    WpfRecorder.SaveToFile(dialog.FileName);
                    StatusText.Text = $"Recording saved to {dialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving: {ex.Message}", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                WpfRecorder.RecordTextInput(
                    "TextBox",
                    textBox.Name,
                    null,
                    textBox.Text);
                StatusText.Text = $"Text changed in {textBox.Name}";
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
            {
                WpfRecorder.Hub.AddEntry(new RecordEntry
                {
                    EntryType = RecordEntryType.UISelectionChange,
                    UIInfo = new UIInfo
                    {
                        ControlType = "ComboBox",
                        ControlName = comboBox.Name,
                        NewValue = item.Content?.ToString(),
                        WindowTitle = Title
                    }
                });
                StatusText.Text = $"Selected: {item.Content}";
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio)
            {
                WpfRecorder.Hub.AddEntry(new RecordEntry
                {
                    EntryType = RecordEntryType.UIToggle,
                    UIInfo = new UIInfo
                    {
                        ControlType = "RadioButton",
                        ControlName = radio.Name,
                        NewValue = radio.Content?.ToString(),
                        WindowTitle = Title
                    }
                });
                StatusText.Text = $"Priority set to: {radio.Content}";
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                WpfRecorder.Hub.AddEntry(new RecordEntry
                {
                    EntryType = RecordEntryType.UIToggle,
                    UIInfo = new UIInfo
                    {
                        ControlType = "CheckBox",
                        ControlName = checkBox.Name,
                        NewValue = checkBox.IsChecked?.ToString(),
                        Text = checkBox.Content?.ToString(),
                        WindowTitle = Title
                    }
                });
                StatusText.Text = $"{checkBox.Content}: {(checkBox.IsChecked == true ? "Checked" : "Unchecked")}";
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.RecordClick("Button", "SubmitButton", "Submit");
            StatusText.Text = "Form submitted";
            MessageBox.Show("Form submitted successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.RecordClick("Button", "CancelButton", "Cancel");
            StatusText.Text = "Action cancelled";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.RecordClick("Button", "ResetButton", "Reset");

            NameTextBox.Clear();
            EmailTextBox.Clear();
            CategoryComboBox.SelectedIndex = -1;
            MediumPriorityRadio.IsChecked = true;
            NotifyCheckBox.IsChecked = false;
            AgreeCheckBox.IsChecked = false;

            StatusText.Text = "Form reset";
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.RecordClick("Button", "AddItemButton", "Add >>");

            if (AvailableItemsList.SelectedItem is ListBoxItem item)
            {
                AvailableItemsList.Items.Remove(item);
                SelectedItemsList.Items.Add(item);
                StatusText.Text = $"Added: {item.Content}";
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.RecordClick("Button", "RemoveItemButton", "<< Remove");

            if (SelectedItemsList.SelectedItem is ListBoxItem item)
            {
                SelectedItemsList.Items.Remove(item);
                AvailableItemsList.Items.Add(item);
                StatusText.Text = $"Removed: {item.Content}";
            }
        }

        private async void GetRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.RecordClick("Button", "GetRequestBtn", "GET Request");
            await MakeApiCallAsync("GET");
        }

        private async void PostRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            WpfRecorder.RecordClick("Button", "PostRequestBtn", "POST Request");
            await MakeApiCallAsync("POST");
        }

        private async Task MakeApiCallAsync(string method)
        {
            try
            {
                StatusText.Text = $"Making {method} request...";
                ApiResponseTextBox.Text = "Loading...";

                HttpResponseMessage response;

                if (method == "GET")
                {
                    response = await _httpClient!.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
                }
                else
                {
                    var content = new StringContent(
                        "{\"title\":\"Test Post\",\"body\":\"This is a test\",\"userId\":1}",
                        Encoding.UTF8,
                        "application/json");
                    response = await _httpClient!.PostAsync("https://jsonplaceholder.typicode.com/posts", content);
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                ApiResponseTextBox.Text = $"Status: {(int)response.StatusCode} {response.StatusCode}\n\n{responseBody}";
                StatusText.Text = $"{method} request completed - {response.StatusCode}";
            }
            catch (Exception ex)
            {
                ApiResponseTextBox.Text = $"Error: {ex.Message}";
                StatusText.Text = $"Request failed: {ex.Message}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _httpClient?.Dispose();
            base.OnClosed(e);
        }
    }
}
