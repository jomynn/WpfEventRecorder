using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Extension.Models;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Extension.Commands;

/// <summary>
/// Command to start recording WPF events.
/// </summary>
[Command(VSCommandTable.CommandIds.StartRecording)]
internal sealed class StartRecordingCommand : BaseCommand<StartRecordingCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        var sessionService = package.RecordingSessionService;

        if (sessionService.CurrentState == RecordingState.Recording)
        {
            await VS.MessageBox.ShowWarningAsync("WPF Event Recorder", "Recording is already in progress.");
            return;
        }

        try
        {
            await sessionService.StartRecordingAsync();
            await VS.StatusBar.ShowMessageAsync("WPF Event Recording started");
        }
        catch (Exception ex)
        {
            await VS.MessageBox.ShowErrorAsync("WPF Event Recorder", $"Failed to start recording: {ex.Message}");
        }
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        var package = Package as WpfEventRecorderPackage;
        if (package == null) return;

        Command.Enabled = package.RecordingSessionService.CurrentState != RecordingState.Recording;
    }
}
