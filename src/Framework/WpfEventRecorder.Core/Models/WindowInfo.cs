using System;

namespace WpfEventRecorder.Core.Models
{
    /// <summary>
    /// Information about a running window/process
    /// </summary>
    public class WindowInfo
    {
        /// <summary>
        /// Process ID
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// Process name
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// Main window title
        /// </summary>
        public string WindowTitle { get; set; } = string.Empty;

        /// <summary>
        /// Main window handle
        /// </summary>
        public IntPtr WindowHandle { get; set; }

        /// <summary>
        /// Path to the executable
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// Whether this is a WPF application
        /// </summary>
        public bool IsWpfApp { get; set; }

        /// <summary>
        /// Display name for UI
        /// </summary>
        public string DisplayName
        {
            get
            {
                return string.IsNullOrEmpty(WindowTitle)
                    ? ProcessName
                    : $"{WindowTitle} ({ProcessName})";
            }
        }

        public override string ToString() => DisplayName;
    }
}
