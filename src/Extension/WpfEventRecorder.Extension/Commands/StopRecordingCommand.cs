using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Extension.Models;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Extension.Commands;

/// <summary>
/// Command to stop recording WPF events.
/// </summary>
[Command(VSCommandTable.CommandIds.StopRecording)]
internal sealed class StopRecordingCommand : BaseCommand<StopRecordingCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        var sessionService = package.RecordingSessionService;

        if (sessionService.CurrentState == RecordingState.Idle)
        {
            await VS.MessageBox.ShowWarningAsync("WPF Event Recorder", "No recording session is active.");
            return;
        }

        try
        {
            sessionService.StopRecording();
            var eventCount = sessionService.CurrentSession?.Events.Count ?? 0;
            await VS.StatusBar.ShowMessageAsync($"WPF Event Recording stopped. {eventCount} events captured.");
        }
        catch (Exception ex)
        {
            await VS.MessageBox.ShowErrorAsync("WPF Event Recorder", $"Failed to stop recording: {ex.Message}");
        }
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        var state = package.RecordingSessionService.CurrentState;
        Command.Enabled = state == RecordingState.Recording || state == RecordingState.Paused;
    }
}
