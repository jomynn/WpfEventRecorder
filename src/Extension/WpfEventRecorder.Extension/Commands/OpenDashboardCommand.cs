using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Extension.ToolWindows;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Extension.Commands;

/// <summary>
/// Command to open the Recording Dashboard tool window.
/// </summary>
[Command(VSCommandTable.CommandIds.OpenDashboard)]
internal sealed class OpenDashboardCommand : BaseCommand<OpenDashboardCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        await RecordingDashboardWindow.ShowAsync();
    }
}
