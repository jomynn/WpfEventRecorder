using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Extension.Models;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Extension.Commands;

/// <summary>
/// Command to clear all recorded events.
/// </summary>
[Command(VSCommandTable.CommandIds.ClearRecording)]
internal sealed class ClearRecordingCommand : BaseCommand<ClearRecordingCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        var sessionService = package.RecordingSessionService;

        if (sessionService.CurrentSession == null || !sessionService.CurrentSession.Events.Any())
        {
            await VS.MessageBox.ShowWarningAsync("WPF Event Recorder", "No events to clear.");
            return;
        }

        var result = await VS.MessageBox.ShowConfirmAsync(
            "WPF Event Recorder",
            $"Are you sure you want to clear {sessionService.CurrentSession.Events.Count} recorded events?");

        if (result)
        {
            sessionService.ClearEvents();
            await VS.StatusBar.ShowMessageAsync("Recording cleared");
        }
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        Command.Enabled = package.RecordingSessionService.CurrentSession?.Events.Any() == true;
    }
}
