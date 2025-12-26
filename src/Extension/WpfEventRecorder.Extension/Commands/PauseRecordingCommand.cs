using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Extension.Models;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Extension.Commands;

/// <summary>
/// Command to pause/resume recording WPF events.
/// </summary>
[Command(VSCommandTable.CommandIds.PauseRecording)]
internal sealed class PauseRecordingCommand : BaseCommand<PauseRecordingCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        var sessionService = package.RecordingSessionService;

        try
        {
            if (sessionService.CurrentState == RecordingState.Recording)
            {
                sessionService.PauseRecording();
                await VS.StatusBar.ShowMessageAsync("WPF Event Recording paused");
            }
            else if (sessionService.CurrentState == RecordingState.Paused)
            {
                sessionService.ResumeRecording();
                await VS.StatusBar.ShowMessageAsync("WPF Event Recording resumed");
            }
        }
        catch (Exception ex)
        {
            await VS.MessageBox.ShowErrorAsync("WPF Event Recorder", $"Failed to pause/resume recording: {ex.Message}");
        }
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        var state = package.RecordingSessionService.CurrentState;
        Command.Enabled = state == RecordingState.Recording || state == RecordingState.Paused;
        Command.Text = state == RecordingState.Paused ? "Resume Recording" : "Pause Recording";
    }
}
