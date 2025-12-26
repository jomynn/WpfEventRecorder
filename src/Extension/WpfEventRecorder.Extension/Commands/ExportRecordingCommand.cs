using System.Windows.Forms;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Extension.Models;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Extension.Commands;

/// <summary>
/// Command to export recorded events to various formats.
/// </summary>
[Command(VSCommandTable.CommandIds.ExportRecording)]
internal sealed class ExportRecordingCommand : BaseCommand<ExportRecordingCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        var sessionService = package.RecordingSessionService;
        var exportService = package.ExportService;

        if (sessionService.CurrentSession == null || !sessionService.CurrentSession.Events.Any())
        {
            await VS.MessageBox.ShowWarningAsync("WPF Event Recorder", "No events to export. Start a recording session first.");
            return;
        }

        // Show export dialog
        using var dialog = new SaveFileDialog
        {
            Title = "Export Recording",
            Filter = "JSON File (*.json)|*.json|" +
                     "xUnit Test (*.cs)|*.cs|" +
                     "NUnit Test (*.cs)|*.cs|" +
                     "MSTest (*.cs)|*.cs|" +
                     "Playwright Test (*.ts)|*.ts",
            DefaultExt = "json",
            FileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            var options = new ExportOptions
            {
                FilePath = dialog.FileName,
                Format = GetExportFormat(dialog.FilterIndex),
                IncludeTimestamps = true,
                GroupByCorrelation = true
            };

            await exportService.ExportAsync(sessionService.CurrentSession, options);
            await VS.StatusBar.ShowMessageAsync($"Recording exported to {dialog.FileName}");
        }
        catch (Exception ex)
        {
            await VS.MessageBox.ShowErrorAsync("WPF Event Recorder", $"Failed to export recording: {ex.Message}");
        }
    }

    private static ExportFormat GetExportFormat(int filterIndex) => filterIndex switch
    {
        1 => ExportFormat.Json,
        2 => ExportFormat.XUnit,
        3 => ExportFormat.NUnit,
        4 => ExportFormat.MSTest,
        5 => ExportFormat.Playwright,
        _ => ExportFormat.Json
    };

    protected override void BeforeQueryStatus(EventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        Command.Enabled = package.RecordingSessionService.CurrentSession?.Events.Any() == true;
    }
}
