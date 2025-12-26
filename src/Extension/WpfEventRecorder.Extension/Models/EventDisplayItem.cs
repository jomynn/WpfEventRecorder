using WpfEventRecorder.Core.Events;

namespace WpfEventRecorder.Extension.Models;

/// <summary>
/// Display item for events in the dashboard.
/// </summary>
public class EventDisplayItem
{
    /// <summary>
    /// The underlying recorded event.
    /// </summary>
    public RecordedEvent Event { get; }

    /// <summary>
    /// Sequence number for display.
    /// </summary>
    public int SequenceNumber => Event.SequenceNumber;

    /// <summary>
    /// Formatted timestamp.
    /// </summary>
    public string TimeStamp => Event.Timestamp.ToString("HH:mm:ss.fff");

    /// <summary>
    /// Event type name.
    /// </summary>
    public string EventType => Event.EventType;

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string Description => Event.GetDescription();

    /// <summary>
    /// Source element information.
    /// </summary>
    public string Source => Event.AutomationId ?? Event.SourceElementName ?? Event.SourceElementType ?? "Unknown";

    /// <summary>
    /// Whether this event is part of a correlation group.
    /// </summary>
    public bool IsCorrelated => !string.IsNullOrEmpty(Event.CorrelationId);

    /// <summary>
    /// Color indicator based on event type.
    /// </summary>
    public string ColorIndicator => Event switch
    {
        InputEvent => "#4CAF50",      // Green
        CommandEvent => "#2196F3",    // Blue
        ApiCallEvent ae => ae.IsSuccess ? "#9C27B0" : "#F44336", // Purple/Red
        NavigationEvent => "#FF9800", // Orange
        WindowEvent => "#607D8B",     // Grey-Blue
        _ => "#9E9E9E"                // Grey
    };

    public EventDisplayItem(RecordedEvent @event)
    {
        Event = @event;
    }
}
