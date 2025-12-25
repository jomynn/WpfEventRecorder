using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace WpfEventRecorder.ToolWindows
{
    /// <summary>
    /// Tool window for viewing recorded WPF events
    /// </summary>
    [Guid("D4E5F6A7-B8C9-0123-DEFG-456789012345")]
    public class RecorderToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the RecorderToolWindow class
        /// </summary>
        public RecorderToolWindow() : base(null)
        {
            Caption = "WPF Event Recorder";
            Content = new RecorderToolWindowControl();
        }
    }
}
