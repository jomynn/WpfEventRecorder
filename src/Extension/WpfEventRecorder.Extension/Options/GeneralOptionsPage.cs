using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace WpfEventRecorder.Extension.Options;

/// <summary>
/// General options page for WPF Event Recorder.
/// </summary>
[Guid("d4e5f6a7-b8c9-0123-def0-345678901234")]
public class GeneralOptionsPage : DialogPage
{
    [Category("General")]
    [DisplayName("Pipe Name")]
    [Description("Name of the named pipe for IPC communication.")]
    public string PipeName { get; set; } = "WpfEventRecorder";

    [Category("General")]
    [DisplayName("Auto-Start Recording")]
    [Description("Automatically start recording when a debug session begins.")]
    public bool AutoStartRecording { get; set; } = false;

    [Category("General")]
    [DisplayName("Show Notifications")]
    [Description("Show status bar notifications for recording events.")]
    public bool ShowNotifications { get; set; } = true;

    [Category("Export")]
    [DisplayName("Default Export Format")]
    [Description("Default format for exporting recorded events.")]
    public string DefaultExportFormat { get; set; } = "JSON";

    [Category("Export")]
    [DisplayName("Include Timestamps")]
    [Description("Include timestamps in exported test code.")]
    public bool IncludeTimestamps { get; set; } = true;

    [Category("Export")]
    [DisplayName("Group by Correlation")]
    [Description("Group related events by correlation ID in exports.")]
    public bool GroupByCorrelation { get; set; } = true;
}
