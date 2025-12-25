using System;

namespace WpfEventRecorder.Core.Models
{
    /// <summary>
    /// Represents a single recorded event (UI interaction or API call)
    /// </summary>
    public class RecordEntry
    {
        /// <summary>
        /// Unique identifier for this entry
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Timestamp when the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Type of the recorded event
        /// </summary>
        public RecordEntryType EntryType { get; set; }

        /// <summary>
        /// UI interaction information (null for API entries)
        /// </summary>
        public UIInfo? UIInfo { get; set; }

        /// <summary>
        /// API call information (null for UI entries)
        /// </summary>
        public ApiInfo? ApiInfo { get; set; }

        /// <summary>
        /// Correlation ID for linking UI actions to API calls
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Duration in milliseconds (for API calls)
        /// </summary>
        public long? DurationMs { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Type of recorded entry
    /// </summary>
    public enum RecordEntryType
    {
        /// <summary>UI click event</summary>
        UIClick,
        /// <summary>UI double-click event</summary>
        UIDoubleClick,
        /// <summary>Text input event</summary>
        UITextInput,
        /// <summary>Selection change event</summary>
        UISelectionChange,
        /// <summary>Checkbox/toggle change</summary>
        UIToggle,
        /// <summary>Keyboard shortcut</summary>
        UIKeyboardShortcut,
        /// <summary>Drag and drop operation</summary>
        UIDragDrop,
        /// <summary>Window opened</summary>
        UIWindowOpen,
        /// <summary>Window closed</summary>
        UIWindowClose,
        /// <summary>HTTP API request</summary>
        ApiRequest,
        /// <summary>HTTP API response</summary>
        ApiResponse,
        /// <summary>Custom event</summary>
        Custom
    }
}
