using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace WpfEventRecorder.Extension.Options;

/// <summary>
/// Recording options page for WPF Event Recorder.
/// </summary>
[Guid("e5f6a7b8-c9d0-1234-ef01-456789012345")]
public class RecordingOptionsPage : DialogPage
{
    [Category("Event Types")]
    [DisplayName("Record Input Events")]
    [Description("Record user input events (text changes, selections, clicks).")]
    public bool RecordInputEvents { get; set; } = true;

    [Category("Event Types")]
    [DisplayName("Record Commands")]
    [Description("Record ICommand executions.")]
    public bool RecordCommands { get; set; } = true;

    [Category("Event Types")]
    [DisplayName("Record API Calls")]
    [Description("Record HTTP API calls.")]
    public bool RecordApiCalls { get; set; } = true;

    [Category("Event Types")]
    [DisplayName("Record Navigation")]
    [Description("Record navigation events.")]
    public bool RecordNavigation { get; set; } = true;

    [Category("Event Types")]
    [DisplayName("Record Window Events")]
    [Description("Record window open/close/state events.")]
    public bool RecordWindowEvents { get; set; } = true;

    [Category("API Recording")]
    [DisplayName("Capture API Payloads")]
    [Description("Capture request and response bodies for API calls.")]
    public bool CaptureApiPayloads { get; set; } = true;

    [Category("API Recording")]
    [DisplayName("Max Payload Size (KB)")]
    [Description("Maximum size in KB for captured API payloads.")]
    public int MaxPayloadSizeKB { get; set; } = 1024;

    [Category("Filtering")]
    [DisplayName("Excluded Element Types")]
    [Description("Comma-separated list of element types to exclude from recording.")]
    public string ExcludedElementTypes { get; set; } = "";

    [Category("Filtering")]
    [DisplayName("Excluded Element Names")]
    [Description("Comma-separated list of element names to exclude from recording.")]
    public string ExcludedElementNames { get; set; } = "";

    [Category("Performance")]
    [DisplayName("Debounce Interval (ms)")]
    [Description("Minimum time between events of the same type from the same source.")]
    public int DebounceIntervalMs { get; set; } = 100;

    [Category("Security")]
    [DisplayName("Sensitive Field Names")]
    [Description("Comma-separated list of field names whose values should be masked.")]
    public string SensitiveFieldNames { get; set; } = "password,pwd,secret,token,apikey,api_key,authorization";
}
