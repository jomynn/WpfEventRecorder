namespace WpfEventRecorder.Extension.Models;

/// <summary>
/// Export format options.
/// </summary>
public enum ExportFormat
{
    Json,
    XUnit,
    NUnit,
    MSTest,
    Playwright
}

/// <summary>
/// Options for exporting recorded events.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Output file path.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Export format.
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.Json;

    /// <summary>
    /// Whether to include timestamps in the export.
    /// </summary>
    public bool IncludeTimestamps { get; set; } = true;

    /// <summary>
    /// Whether to group events by correlation ID.
    /// </summary>
    public bool GroupByCorrelation { get; set; } = true;

    /// <summary>
    /// Whether to include API payloads.
    /// </summary>
    public bool IncludeApiPayloads { get; set; } = false;

    /// <summary>
    /// Filter to specific event types.
    /// </summary>
    public List<string>? EventTypeFilter { get; set; }

    /// <summary>
    /// Test class name (for code generation).
    /// </summary>
    public string? TestClassName { get; set; }

    /// <summary>
    /// Test namespace (for code generation).
    /// </summary>
    public string TestNamespace { get; set; } = "WpfEventRecorder.GeneratedTests";
}
