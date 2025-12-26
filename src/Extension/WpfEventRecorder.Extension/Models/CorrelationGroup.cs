using WpfEventRecorder.Core.Events;

namespace WpfEventRecorder.Extension.Models;

/// <summary>
/// A group of correlated events.
/// </summary>
public class CorrelationGroup
{
    /// <summary>
    /// Correlation ID.
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Events in this group.
    /// </summary>
    public List<RecordedEvent> Events { get; } = new();

    /// <summary>
    /// First event timestamp.
    /// </summary>
    public DateTimeOffset StartTime => Events.FirstOrDefault()?.Timestamp ?? DateTimeOffset.MinValue;

    /// <summary>
    /// Last event timestamp.
    /// </summary>
    public DateTimeOffset EndTime => Events.LastOrDefault()?.Timestamp ?? DateTimeOffset.MinValue;

    /// <summary>
    /// Duration of the correlation group.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Summary description of the group.
    /// </summary>
    public string Summary
    {
        get
        {
            var eventTypes = Events.Select(e => e.EventType).Distinct().ToList();
            return $"{Events.Count} events ({string.Join(", ", eventTypes)})";
        }
    }

    public CorrelationGroup(string correlationId)
    {
        CorrelationId = correlationId;
    }

    public CorrelationGroup(string correlationId, IEnumerable<RecordedEvent> events)
        : this(correlationId)
    {
        Events.AddRange(events.OrderBy(e => e.Timestamp));
    }
}
