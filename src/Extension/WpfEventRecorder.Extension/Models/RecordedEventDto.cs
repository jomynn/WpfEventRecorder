namespace WpfEventRecorder.Extension.Models;

/// <summary>
/// DTO for recorded events in the extension.
/// </summary>
public class RecordedEventDto
{
    public string Id { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; }
    public int SequenceNumber { get; set; }
    public string EventType { get; set; } = "";
    public string? CorrelationId { get; set; }
    public string? SourceElementName { get; set; }
    public string? SourceElementType { get; set; }
    public string? AutomationId { get; set; }
    public string Description { get; set; } = "";
    public Dictionary<string, object?> Details { get; set; } = new();
}
