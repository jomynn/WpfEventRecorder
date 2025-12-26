using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfEventRecorder.Core.Communication;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.ToolWindows;

/// <summary>
/// ViewModel for the Recording Dashboard.
/// </summary>
public partial class RecordingDashboardViewModel : ObservableObject
{
    private readonly WpfEventRecorderPackage _package;
    private readonly DispatcherTimer _durationTimer;

    [ObservableProperty]
    private RecordingState _currentState = RecordingState.Idle;

    [ObservableProperty]
    private ObservableCollection<EventDisplayItem> _events = new();

    [ObservableProperty]
    private ObservableCollection<EventDisplayItem> _filteredEvents = new();

    [ObservableProperty]
    private EventDisplayItem? _selectedEvent;

    [ObservableProperty]
    private string _selectedEventDetails = "";

    [ObservableProperty]
    private TimeSpan _duration = TimeSpan.Zero;

    [ObservableProperty]
    private ObservableCollection<string> _eventTypeFilters = new() { "All", "Input", "Command", "ApiCall", "Navigation", "Window" };

    [ObservableProperty]
    private string _selectedEventTypeFilter = "All";

    public int EventCount => Events.Count;
    public bool HasEvents => Events.Count > 0;
    public bool HasSelectedEvent => SelectedEvent != null;

    public bool CanStartRecording => CurrentState == RecordingState.Idle;
    public bool CanStopRecording => CurrentState == RecordingState.Recording || CurrentState == RecordingState.Paused;
    public bool CanPauseRecording => CurrentState == RecordingState.Recording || CurrentState == RecordingState.Paused;

    public string PauseButtonText => CurrentState == RecordingState.Paused ? "Resume" : "Pause";

    public string StatusText => CurrentState switch
    {
        RecordingState.Recording => "Recording",
        RecordingState.Paused => "Paused",
        RecordingState.Stopping => "Stopping...",
        _ => "Idle"
    };

    public Brush StatusColor => CurrentState switch
    {
        RecordingState.Recording => Brushes.Red,
        RecordingState.Paused => Brushes.Orange,
        _ => Brushes.Gray
    };

    public RecordingDashboardViewModel(WpfEventRecorderPackage package)
    {
        _package = package;

        _durationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _durationTimer.Tick += (_, _) => UpdateDuration();

        // Subscribe to session events
        _package.RecordingSessionService.StateChanged += OnStateChanged;
        _package.RecordingSessionService.EventRecorded += OnEventRecorded;
    }

    private void OnStateChanged(object? sender, RecordingState state)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            CurrentState = state;

            if (state == RecordingState.Recording)
            {
                _durationTimer.Start();
            }
            else if (state == RecordingState.Idle)
            {
                _durationTimer.Stop();
            }

            OnPropertyChanged(nameof(CanStartRecording));
            OnPropertyChanged(nameof(CanStopRecording));
            OnPropertyChanged(nameof(CanPauseRecording));
            OnPropertyChanged(nameof(PauseButtonText));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
        });
    }

    private void OnEventRecorded(object? sender, RecordedEvent @event)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            var displayItem = new EventDisplayItem(@event);
            Events.Add(displayItem);
            ApplyFilter();
            OnPropertyChanged(nameof(EventCount));
            OnPropertyChanged(nameof(HasEvents));
        });
    }

    private void UpdateDuration()
    {
        var session = _package.RecordingSessionService.CurrentSession;
        if (session != null)
        {
            Duration = session.Duration;
        }
    }

    partial void OnSelectedEventChanged(EventDisplayItem? value)
    {
        if (value?.Event != null)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            SelectedEventDetails = EventSerializer.SerializeIndented(value.Event);
        }
        else
        {
            SelectedEventDetails = "";
        }

        OnPropertyChanged(nameof(HasSelectedEvent));
    }

    partial void OnSelectedEventTypeFilterChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (SelectedEventTypeFilter == "All")
        {
            FilteredEvents = new ObservableCollection<EventDisplayItem>(Events);
        }
        else
        {
            FilteredEvents = new ObservableCollection<EventDisplayItem>(
                Events.Where(e => e.EventType == SelectedEventTypeFilter));
        }
    }

    [RelayCommand]
    private async Task StartRecordingAsync()
    {
        await _package.RecordingSessionService.StartRecordingAsync();
    }

    [RelayCommand]
    private void StopRecording()
    {
        _package.RecordingSessionService.StopRecording();
    }

    [RelayCommand]
    private void PauseRecording()
    {
        if (CurrentState == RecordingState.Recording)
        {
            _package.RecordingSessionService.PauseRecording();
        }
        else if (CurrentState == RecordingState.Paused)
        {
            _package.RecordingSessionService.ResumeRecording();
        }
    }

    [RelayCommand]
    private void Clear()
    {
        _package.RecordingSessionService.ClearEvents();
        Events.Clear();
        FilteredEvents.Clear();
        Duration = TimeSpan.Zero;
        OnPropertyChanged(nameof(EventCount));
        OnPropertyChanged(nameof(HasEvents));
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        // This would show the export dialog
        // For now, trigger the export command
        await Community.VisualStudio.Toolkit.VS.Commands.ExecuteAsync(
            VSCommandTable.CommandSetGuid,
            VSCommandTable.CommandIds.ExportRecording);
    }
}
