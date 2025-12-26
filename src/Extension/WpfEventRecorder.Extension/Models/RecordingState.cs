namespace WpfEventRecorder.Extension.Models;

/// <summary>
/// Recording session states.
/// </summary>
public enum RecordingState
{
    /// <summary>
    /// No recording in progress.
    /// </summary>
    Idle,

    /// <summary>
    /// Recording is active.
    /// </summary>
    Recording,

    /// <summary>
    /// Recording is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// Recording is being stopped.
    /// </summary>
    Stopping
}
