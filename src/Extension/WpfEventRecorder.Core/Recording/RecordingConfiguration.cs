namespace WpfEventRecorder.Core.Recording;

/// <summary>
/// Configuration options for recording.
/// </summary>
public class RecordingConfiguration
{
    /// <summary>
    /// Whether to record input events (text changes, selections, etc.).
    /// </summary>
    public bool RecordInputEvents { get; set; } = true;

    /// <summary>
    /// Whether to record command executions.
    /// </summary>
    public bool RecordCommands { get; set; } = true;

    /// <summary>
    /// Whether to record HTTP API calls.
    /// </summary>
    public bool RecordApiCalls { get; set; } = true;

    /// <summary>
    /// Whether to record navigation events.
    /// </summary>
    public bool RecordNavigation { get; set; } = true;

    /// <summary>
    /// Whether to record window events.
    /// </summary>
    public bool RecordWindowEvents { get; set; } = true;

    /// <summary>
    /// Whether to capture request/response bodies for API calls.
    /// </summary>
    public bool CaptureApiPayloads { get; set; } = true;

    /// <summary>
    /// Maximum size in bytes for captured API payloads.
    /// </summary>
    public int MaxPayloadSize { get; set; } = 1024 * 1024; // 1MB

    /// <summary>
    /// Whether to record property changes in ViewModels.
    /// </summary>
    public bool RecordPropertyChanges { get; set; } = false;

    /// <summary>
    /// Whether to automatically correlate related events.
    /// </summary>
    public bool EnableCorrelation { get; set; } = true;

    /// <summary>
    /// Minimum time between events of the same type from the same source (debounce).
    /// </summary>
    public TimeSpan DebounceInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// List of element types to exclude from recording.
    /// </summary>
    public List<string> ExcludedElementTypes { get; set; } = new();

    /// <summary>
    /// List of element names to exclude from recording.
    /// </summary>
    public List<string> ExcludedElementNames { get; set; } = new();

    /// <summary>
    /// List of API URL patterns to exclude.
    /// </summary>
    public List<string> ExcludedApiPatterns { get; set; } = new();

    /// <summary>
    /// Named pipe name for IPC communication.
    /// </summary>
    public string PipeName { get; set; } = "WpfEventRecorder";

    /// <summary>
    /// Whether to record keyboard shortcuts.
    /// </summary>
    public bool RecordKeyboardShortcuts { get; set; } = true;

    /// <summary>
    /// List of sensitive field names whose values should be masked.
    /// </summary>
    public List<string> SensitiveFields { get; set; } = new()
    {
        "password", "pwd", "secret", "token", "apikey", "api_key", "authorization"
    };
}
