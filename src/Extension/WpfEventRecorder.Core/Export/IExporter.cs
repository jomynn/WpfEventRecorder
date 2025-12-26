using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Export;

/// <summary>
/// Interface for event exporters.
/// </summary>
public interface IExporter
{
    /// <summary>
    /// Gets the export format name.
    /// </summary>
    string FormatName { get; }

    /// <summary>
    /// Gets the default file extension.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Exports events to a string.
    /// </summary>
    string Export(IEnumerable<RecordedEvent> events, RecordingSession? session = null);

    /// <summary>
    /// Exports events to a file.
    /// </summary>
    Task ExportToFileAsync(IEnumerable<RecordedEvent> events, string filePath, RecordingSession? session = null);
}

/// <summary>
/// Base class for exporters.
/// </summary>
public abstract class ExporterBase : IExporter
{
    public abstract string FormatName { get; }
    public abstract string FileExtension { get; }

    public abstract string Export(IEnumerable<RecordedEvent> events, RecordingSession? session = null);

    public virtual async Task ExportToFileAsync(IEnumerable<RecordedEvent> events, string filePath, RecordingSession? session = null)
    {
        var content = Export(events, session);
        await File.WriteAllTextAsync(filePath, content);
    }

    /// <summary>
    /// Sanitizes a string for use in generated code.
    /// </summary>
    protected static string SanitizeForCode(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    /// <summary>
    /// Creates a valid identifier from a string.
    /// </summary>
    protected static string ToIdentifier(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "Unknown";

        var result = new System.Text.StringBuilder();
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
                result.Append(c);
            else if (c == ' ' || c == '_' || c == '-')
                result.Append('_');
        }

        var identifier = result.ToString();

        // Ensure starts with letter
        if (identifier.Length > 0 && char.IsDigit(identifier[0]))
            identifier = "_" + identifier;

        return string.IsNullOrEmpty(identifier) ? "Unknown" : identifier;
    }

    /// <summary>
    /// Gets a descriptive method name for an event.
    /// </summary>
    protected static string GetMethodName(RecordedEvent @event, int index)
    {
        var prefix = @event switch
        {
            InputEvent ie => ie.InputType.ToString(),
            CommandEvent => "ExecuteCommand",
            ApiCallEvent ae => $"{ae.HttpMethod}Api",
            NavigationEvent => "Navigate",
            WindowEvent we => we.WindowEventType.ToString(),
            _ => "Event"
        };

        var target = @event.SourceElementName ?? @event.AutomationId ?? "";
        return $"{prefix}_{ToIdentifier(target)}_{index}";
    }
}
