using System.Text.Json.Serialization;

namespace WpfEventRecorder.Core.Events;

/// <summary>
/// Base class for all recorded events.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(InputEvent), "input")]
[JsonDerivedType(typeof(CommandEvent), "command")]
[JsonDerivedType(typeof(ApiCallEvent), "api")]
[JsonDerivedType(typeof(NavigationEvent), "navigation")]
[JsonDerivedType(typeof(WindowEvent), "window")]
public abstract class RecordedEvent
{
    /// <summary>
    /// Unique identifier for this event.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// Correlation ID for grouping related events.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Sequence number within the recording session.
    /// </summary>
    public int SequenceNumber { get; set; }

    /// <summary>
    /// Type of the event.
    /// </summary>
    public abstract string EventType { get; }

    /// <summary>
    /// Name of the source element that triggered the event.
    /// </summary>
    public string? SourceElementName { get; set; }

    /// <summary>
    /// Type of the source element.
    /// </summary>
    public string? SourceElementType { get; set; }

    /// <summary>
    /// Automation ID of the source element.
    /// </summary>
    public string? AutomationId { get; set; }

    /// <summary>
    /// Additional metadata about the event.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();

    /// <summary>
    /// Human-readable description of the event.
    /// </summary>
    public abstract string GetDescription();
}
