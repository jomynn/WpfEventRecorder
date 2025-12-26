using System.Runtime.InteropServices;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace WpfEventRecorder.Extension.ToolWindows;

/// <summary>
/// Recording Dashboard tool window.
/// </summary>
[Guid("c3d4e5f6-a7b8-9012-cdef-123456789abc")]
public class RecordingDashboardWindow : ToolWindowPane
{
    public RecordingDashboardWindow() : base(null)
    {
        Caption = "WPF Event Recorder";
        BitmapImageMoniker = KnownMonikers.PerformanceReport;
    }

    public override void OnToolWindowCreated()
    {
        base.OnToolWindowCreated();

        var package = Package as WpfEventRecorderPackage;
        if (package != null)
        {
            Content = new RecordingDashboardControl(package);
        }
    }

    /// <summary>
    /// Shows the tool window.
    /// </summary>
    public static async Task ShowAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var window = await VS.Windows.ShowToolWindowAsync<RecordingDashboardWindow>();
    }
}
