using WpfEventRecorder.Core.Recording;
using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Service for exporting recorded events.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports a recording session with the given options.
    /// </summary>
    Task ExportAsync(RecordingSession session, ExportOptions options);

    /// <summary>
    /// Gets the available export formats.
    /// </summary>
    IEnumerable<ExportFormat> GetAvailableFormats();
}
