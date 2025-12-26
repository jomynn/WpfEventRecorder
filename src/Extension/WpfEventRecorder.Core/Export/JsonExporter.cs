using System.Text.Json;
using WpfEventRecorder.Core.Communication;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Export;

/// <summary>
/// Exports events to JSON format.
/// </summary>
public class JsonExporter : ExporterBase
{
    public override string FormatName => "JSON";
    public override string FileExtension => ".json";

    public override string Export(IEnumerable<RecordedEvent> events, RecordingSession? session = null)
    {
        var exportData = new
        {
            session = session != null ? new
            {
                id = session.Id,
                name = session.Name,
                startTime = session.StartTime,
                endTime = session.EndTime,
                duration = session.Duration.ToString(),
                applicationName = session.ApplicationName,
                processId = session.ProcessId,
                eventCount = events.Count()
            } : null,
            events = events.ToList()
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        return JsonSerializer.Serialize(exportData, options);
    }
}
