using System;
using System.Net.Http;
using System.Windows.Input;
using WpfEventRecorder.Core;
using WpfEventRecorder.Core.Attributes;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.SampleApp.Services;

namespace WpfEventRecorder.SampleApp.ViewModels
{
    /// <summary>
    /// Main window view model
    /// </summary>
    [RecordViewModel("MainWindow")]
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private CustomerListViewModel _customerListViewModel;
        private bool _isRecording;
        private int _eventCount;
        private string _statusMessage = "Ready";

        /// <summary>
        /// Customer list view model
        /// </summary>
        public CustomerListViewModel CustomerListViewModel
        {
            get => _customerListViewModel;
            set => SetProperty(ref _customerListViewModel, value);
        }

        /// <summary>
        /// Whether recording is active
        /// </summary>
        [IgnoreRecording]
        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty(ref _isRecording, value);
        }

        /// <summary>
        /// Number of recorded events
        /// </summary>
        [IgnoreRecording]
        public int EventCount
        {
            get => _eventCount;
            set => SetProperty(ref _eventCount, value);
        }

        /// <summary>
        /// Status message
        /// </summary>
        [IgnoreRecording]
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Commands
        public ICommand StartRecordingCommand { get; }
        public ICommand StopRecordingCommand { get; }
        public ICommand ClearRecordingCommand { get; }
        public ICommand SaveRecordingCommand { get; }

        /// <summary>
        /// Creates a new main view model
        /// </summary>
        public MainViewModel()
        {
            // Create HTTP client with recording support
            _httpClient = WpfRecorder.CreateHttpClient();

            // Create customer service and view model
            var customerService = new CustomerService(_httpClient);
            CustomerListViewModel = new CustomerListViewModel(customerService);

            // Initialize recording commands
            StartRecordingCommand = new RelayCommand(StartRecording, () => !IsRecording);
            StopRecordingCommand = new RelayCommand(StopRecording, () => IsRecording);
            ClearRecordingCommand = new RelayCommand(ClearRecording);
            SaveRecordingCommand = new RelayCommand(SaveRecording, () => EventCount > 0);

            // Subscribe to recording events
            WpfRecorder.EntryRecorded += OnEntryRecorded;
            WpfRecorder.RecordingStateChanged += OnRecordingStateChanged;

            // Initialize state
            UpdateRecordingState();
        }

        /// <summary>
        /// Starts recording
        /// </summary>
        [RecordCommand("Start Recording")]
        public void StartRecording()
        {
            WpfRecorder.Start("SampleAppSession");
            StatusMessage = "Recording started";
            UpdateRecordingState();
        }

        /// <summary>
        /// Stops recording
        /// </summary>
        [RecordCommand("Stop Recording")]
        public void StopRecording()
        {
            WpfRecorder.Stop();
            StatusMessage = $"Recording stopped - {EventCount} events captured";
            UpdateRecordingState();
        }

        /// <summary>
        /// Clears all recorded events
        /// </summary>
        [RecordCommand("Clear Recording")]
        public void ClearRecording()
        {
            WpfRecorder.Clear();
            EventCount = 0;
            StatusMessage = "Recording cleared";
        }

        /// <summary>
        /// Saves the recording to a file
        /// </summary>
        [RecordCommand("Save Recording")]
        public void SaveRecording()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
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
                    StatusMessage = $"Recording saved to {dialog.FileName}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error saving: {ex.Message}";
                }
            }
        }

        private void OnEntryRecorded(object sender, RecordEntry e)
        {
            EventCount = WpfRecorder.EntryCount;
        }

        private void OnRecordingStateChanged(object sender, bool isRecording)
        {
            UpdateRecordingState();
        }

        private void UpdateRecordingState()
        {
            IsRecording = WpfRecorder.IsRecording;
            EventCount = WpfRecorder.EntryCount;

            ((RelayCommand)StartRecordingCommand).RaiseCanExecuteChanged();
            ((RelayCommand)StopRecordingCommand).RaiseCanExecuteChanged();
            ((RelayCommand)SaveRecordingCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Cleanup()
        {
            WpfRecorder.EntryRecorded -= OnEntryRecorded;
            WpfRecorder.RecordingStateChanged -= OnRecordingStateChanged;
            _httpClient?.Dispose();
        }
    }
}
