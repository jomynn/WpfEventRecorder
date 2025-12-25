using System;
using System.Collections.Generic;

namespace WpfEventRecorder.Core.Models
{
    /// <summary>
    /// Information about a UI element interaction
    /// </summary>
    public class UIInfo
    {
        /// <summary>
        /// Type of the control (Button, TextBox, etc.)
        /// </summary>
        public string ControlType { get; set; } = string.Empty;

        /// <summary>
        /// Name of the control (x:Name in XAML)
        /// </summary>
        public string? ControlName { get; set; }

        /// <summary>
        /// AutomationId for UI automation
        /// </summary>
        public string? AutomationId { get; set; }

        /// <summary>
        /// Text content of the control
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Value before the interaction
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// Value after the interaction
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// Full path to the control in the visual tree
        /// </summary>
        public string? VisualTreePath { get; set; }

        /// <summary>
        /// Parent window title
        /// </summary>
        public string? WindowTitle { get; set; }

        /// <summary>
        /// Parent window type name
        /// </summary>
        public string? WindowType { get; set; }

        /// <summary>
        /// Screen coordinates where the interaction occurred
        /// </summary>
        public ScreenPoint? ScreenPosition { get; set; }

        /// <summary>
        /// Relative position within the control (0-1 range)
        /// </summary>
        public RelativePoint? RelativePosition { get; set; }

        /// <summary>
        /// For keyboard events, the key combination pressed
        /// </summary>
        public string? KeyCombination { get; set; }

        /// <summary>
        /// Additional control properties
        /// </summary>
        public Dictionary<string, string>? Properties { get; set; }

        /// <summary>
        /// CSS-like selector for the control
        /// </summary>
        public string? Selector { get; set; }

        /// <summary>
        /// Content text displayed on the control (e.g., button text like "Cancel", "OK")
        /// </summary>
        public string? ContentText { get; set; }
    }

    /// <summary>
    /// Screen coordinates
    /// </summary>
    public class ScreenPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// Relative position within a control (0-1 range)
    /// </summary>
    public class RelativePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
