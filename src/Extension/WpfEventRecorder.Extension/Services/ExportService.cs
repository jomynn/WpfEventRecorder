using WpfEventRecorder.Core.Export;
using WpfEventRecorder.Core.Recording;
using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Implementation of export service.
/// </summary>
public class ExportService : IExportService
{
    private readonly Dictionary<ExportFormat, IExporter> _exporters;

    public ExportService()
    {
        _exporters = new Dictionary<ExportFormat, IExporter>
        {
            [ExportFormat.Json] = new JsonExporter(),
            [ExportFormat.XUnit] = new XUnitExporter(),
            [ExportFormat.NUnit] = new NUnitExporter(),
            [ExportFormat.MSTest] = new MSTestExporter(),
            [ExportFormat.Playwright] = new PlaywrightExporter()
        };
    }

    public async Task ExportAsync(RecordingSession session, ExportOptions options)
    {
        if (!_exporters.TryGetValue(options.Format, out var exporter))
        {
            throw new NotSupportedException($"Export format {options.Format} is not supported.");
        }

        var events = session.Events.AsEnumerable();

        // Apply event type filter
        if (options.EventTypeFilter?.Any() == true)
        {
            events = events.Where(e => options.EventTypeFilter.Contains(e.EventType));
        }

        await exporter.ExportToFileAsync(events, options.FilePath, session);
    }

    public IEnumerable<ExportFormat> GetAvailableFormats()
    {
        return _exporters.Keys;
    }
}
