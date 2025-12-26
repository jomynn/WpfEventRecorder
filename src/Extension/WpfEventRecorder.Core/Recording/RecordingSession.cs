using System.Collections.ObjectModel;
using WpfEventRecorder.Core.Events;

namespace WpfEventRecorder.Core.Recording;

/// <summary>
/// Represents a recording session containing captured events.
/// </summary>
public class RecordingSession
{
    /// <summary>
    /// Unique identifier for this session.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the recording session.
    /// </summary>
    public string Name { get; set; } = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}";

    /// <summary>
    /// When the session was started.
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// When the session was stopped (null if still active).
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Duration of the session.
    /// </summary>
    public TimeSpan Duration => EndTime.HasValue
        ? EndTime.Value - StartTime
        : DateTimeOffset.Now - StartTime;

    /// <summary>
    /// Name of the target application.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Process ID of the target application.
    /// </summary>
    public int? ProcessId { get; set; }

    /// <summary>
    /// Configuration used for this session.
    /// </summary>
    public RecordingConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Collection of recorded events.
    /// </summary>
    public ObservableCollection<RecordedEvent> Events { get; } = new();

    /// <summary>
    /// Current sequence number.
    /// </summary>
    private int _sequenceNumber;

    /// <summary>
    /// Lock for thread-safe operations.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Event raised when a new event is recorded.
    /// </summary>
    public event EventHandler<RecordedEvent>? EventRecorded;

    /// <summary>
    /// Adds an event to the session.
    /// </summary>
    public void AddEvent(RecordedEvent @event)
    {
        lock (_lock)
        {
            @event.SequenceNumber = ++_sequenceNumber;
            Events.Add(@event);
            EventRecorded?.Invoke(this, @event);
        }
    }

    /// <summary>
    /// Clears all events from the session.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            Events.Clear();
            _sequenceNumber = 0;
        }
    }

    /// <summary>
    /// Gets events grouped by correlation ID.
    /// </summary>
    public IEnumerable<IGrouping<string?, RecordedEvent>> GetCorrelatedGroups()
    {
        return Events
            .Where(e => !string.IsNullOrEmpty(e.CorrelationId))
            .GroupBy(e => e.CorrelationId);
    }

    /// <summary>
    /// Gets events by type.
    /// </summary>
    public IEnumerable<T> GetEventsByType<T>() where T : RecordedEvent
    {
        return Events.OfType<T>();
    }

    /// <summary>
    /// Gets events within a time range.
    /// </summary>
    public IEnumerable<RecordedEvent> GetEventsInRange(DateTimeOffset start, DateTimeOffset end)
    {
        return Events.Where(e => e.Timestamp >= start && e.Timestamp <= end);
    }
}
