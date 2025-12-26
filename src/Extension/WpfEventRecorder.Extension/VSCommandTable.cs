// This file is auto-generated based on Menus.vsct
namespace WpfEventRecorder.Extension;

using System;

/// <summary>
/// Contains all command IDs and GUIDs for the extension.
/// </summary>
internal static class VSCommandTable
{
    public static readonly Guid PackageGuid = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid CommandSetGuid = new("b2c3d4e5-f6a7-8901-bcde-f12345678901");

    public static class CommandIds
    {
        public const int WpfEventRecorderMenuGroup = 0x1020;
        public const int WpfEventRecorderMenu = 0x1021;
        public const int WpfEventRecorderToolbar = 0x1000;
        public const int WpfEventRecorderToolbarGroup = 0x1050;

        public const int StartRecording = 0x0100;
        public const int StopRecording = 0x0101;
        public const int PauseRecording = 0x0102;
        public const int ExportRecording = 0x0103;
        public const int ClearRecording = 0x0104;
        public const int OpenDashboard = 0x0105;
    }
}
