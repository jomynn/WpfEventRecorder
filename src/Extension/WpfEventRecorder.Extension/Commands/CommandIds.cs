namespace WpfEventRecorder.Extension.Commands;

/// <summary>
/// Centralized command IDs for the extension.
/// </summary>
public static class CommandIds
{
    public const int StartRecording = VSCommandTable.CommandIds.StartRecording;
    public const int StopRecording = VSCommandTable.CommandIds.StopRecording;
    public const int PauseRecording = VSCommandTable.CommandIds.PauseRecording;
    public const int ExportRecording = VSCommandTable.CommandIds.ExportRecording;
    public const int ClearRecording = VSCommandTable.CommandIds.ClearRecording;
    public const int OpenDashboard = VSCommandTable.CommandIds.OpenDashboard;
}
