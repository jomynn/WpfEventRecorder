using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;
using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Implementation of recording session service.
/// </summary>
public class RecordingSessionService : IRecordingSessionService
{
    private readonly WpfEventRecorderPackage _package;
    private RecordingSession? _session;
    private RecordingState _state = RecordingState.Idle;

    public RecordingState CurrentState
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }
    }

    public RecordingSession? CurrentSession => _session;

    public event EventHandler<RecordingState>? StateChanged;
    public event EventHandler<RecordedEvent>? EventRecorded;

    public RecordingSessionService(WpfEventRecorderPackage package)
    {
        _package = package;
    }

    public async Task StartRecordingAsync()
    {
        if (CurrentState == RecordingState.Recording)
            throw new InvalidOperationException("Recording is already in progress.");

        _session = new RecordingSession
        {
            Name = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}",
            StartTime = DateTimeOffset.Now
        };

        _session.EventRecorded += OnEventRecorded;

        // Start the pipe server to receive events
        await _package.PipeServerService.StartAsync();

        CurrentState = RecordingState.Recording;
    }

    public void StopRecording()
    {
        if (CurrentState == RecordingState.Idle)
            return;

        CurrentState = RecordingState.Stopping;

        if (_session != null)
        {
            _session.EndTime = DateTimeOffset.Now;
            _session.EventRecorded -= OnEventRecorded;
        }

        CurrentState = RecordingState.Idle;
    }

    public void PauseRecording()
    {
        if (CurrentState != RecordingState.Recording)
            throw new InvalidOperationException("Recording is not in progress.");

        CurrentState = RecordingState.Paused;
    }

    public void ResumeRecording()
    {
        if (CurrentState != RecordingState.Paused)
            throw new InvalidOperationException("Recording is not paused.");

        CurrentState = RecordingState.Recording;
    }

    public void ClearEvents()
    {
        _session?.Clear();
    }

    private void OnEventRecorded(object? sender, RecordedEvent e)
    {
        EventRecorded?.Invoke(this, e);
    }

    internal void AddEvent(RecordedEvent @event)
    {
        if (CurrentState != RecordingState.Recording)
            return;

        _session?.AddEvent(@event);
    }
}
