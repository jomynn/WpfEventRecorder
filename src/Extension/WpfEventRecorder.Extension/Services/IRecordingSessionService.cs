using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;
using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Service for managing recording sessions.
/// </summary>
public interface IRecordingSessionService
{
    /// <summary>
    /// Gets the current recording state.
    /// </summary>
    RecordingState CurrentState { get; }

    /// <summary>
    /// Gets the current session.
    /// </summary>
    RecordingSession? CurrentSession { get; }

    /// <summary>
    /// Event raised when the state changes.
    /// </summary>
    event EventHandler<RecordingState>? StateChanged;

    /// <summary>
    /// Event raised when an event is recorded.
    /// </summary>
    event EventHandler<RecordedEvent>? EventRecorded;

    /// <summary>
    /// Starts a new recording session.
    /// </summary>
    Task StartRecordingAsync();

    /// <summary>
    /// Stops the current recording session.
    /// </summary>
    void StopRecording();

    /// <summary>
    /// Pauses the current recording session.
    /// </summary>
    void PauseRecording();

    /// <summary>
    /// Resumes a paused recording session.
    /// </summary>
    void ResumeRecording();

    /// <summary>
    /// Clears all events from the current session.
    /// </summary>
    void ClearEvents();
}
