using System;

namespace WpfEventRecorder.Commands
{
    /// <summary>
    /// Command IDs and GUIDs matching RecorderCommandSet.vsct
    /// </summary>
    public static class CommandIds
    {
        public const string PackageGuidString = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890";
        public const string CommandSetGuidString = "B2C3D4E5-F6A7-8901-BCDE-F12345678901";

        public static readonly Guid PackageGuid = new Guid(PackageGuidString);
        public static readonly Guid CommandSetGuid = new Guid(CommandSetGuidString);

        // Command IDs
        public const int StartRecordingId = 0x0100;
        public const int StopRecordingId = 0x0101;
        public const int SaveRecordingId = 0x0102;
        public const int ClearRecordingId = 0x0103;
        public const int OpenToolWindowId = 0x0104;
    }
}
