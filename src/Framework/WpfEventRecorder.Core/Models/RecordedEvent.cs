using System;
using System.Collections.Generic;

namespace WpfEventRecorder.Core.Models
{
    /// <summary>
    /// Base class for all recorded events
    /// </summary>
    public abstract class RecordedEvent
    {
        /// <summary>
        /// Unique identifier for this event
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Timestamp when the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Correlation ID for linking related events
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Type discriminator for JSON serialization
        /// </summary>
        public abstract string EventType { get; }
    }

    /// <summary>
    /// Represents a UI input event (text input, selection, etc.)
    /// </summary>
    public class InputEvent : RecordedEvent
    {
        /// <inheritdoc />
        public override string EventType => "Input";

        /// <summary>
        /// Name of the control
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// Type of the control (TextBox, ComboBox, etc.)
        /// </summary>
        public string ControlType { get; set; }

        /// <summary>
        /// Data binding path to the ViewModel property
        /// </summary>
        public string BindingPath { get; set; }

        /// <summary>
        /// Current/new value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Previous value (if applicable)
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// AutomationId for UI automation
        /// </summary>
        public string AutomationId { get; set; }

        /// <summary>
        /// Window title where the event occurred
        /// </summary>
        public string WindowTitle { get; set; }
    }

    /// <summary>
    /// Represents a command execution event
    /// </summary>
    public class CommandEvent : RecordedEvent
    {
        /// <inheritdoc />
        public override string EventType => "Command";

        /// <summary>
        /// Name of the command
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// Full type name of the ViewModel containing the command
        /// </summary>
        public string ViewModelType { get; set; }

        /// <summary>
        /// Command parameter (serialized)
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public string ParameterType { get; set; }

        /// <summary>
        /// Whether the command executed successfully
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Error message if command failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents an HTTP API call event
    /// </summary>
    public class ApiCallEvent : RecordedEvent
    {
        /// <inheritdoc />
        public override string EventType => "ApiCall";

        /// <summary>
        /// HTTP method (GET, POST, etc.)
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Request URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Request headers
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; set; }

        /// <summary>
        /// Request body as string
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// Request model type name
        /// </summary>
        public string RequestModel { get; set; }

        /// <summary>
        /// Response body as string
        /// </summary>
        public string ResponseBody { get; set; }

        /// <summary>
        /// Response model type name
        /// </summary>
        public string ResponseModel { get; set; }

        /// <summary>
        /// Response headers
        /// </summary>
        public Dictionary<string, string> ResponseHeaders { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Duration in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool IsSuccess { get; set; }
    }

    /// <summary>
    /// Represents a navigation event between views
    /// </summary>
    public class NavigationEvent : RecordedEvent
    {
        /// <inheritdoc />
        public override string EventType => "Navigation";

        /// <summary>
        /// Source view/page name
        /// </summary>
        public string FromView { get; set; }

        /// <summary>
        /// Destination view/page name
        /// </summary>
        public string ToView { get; set; }

        /// <summary>
        /// Navigation parameters
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Whether this was a back navigation
        /// </summary>
        public bool IsBackNavigation { get; set; }
    }

    /// <summary>
    /// Represents a property change event in a ViewModel
    /// </summary>
    public class PropertyChangeEvent : RecordedEvent
    {
        /// <inheritdoc />
        public override string EventType => "PropertyChange";

        /// <summary>
        /// Name of the property that changed
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Full type name of the ViewModel
        /// </summary>
        public string ViewModelType { get; set; }

        /// <summary>
        /// New value
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Previous value
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// Type of the property
        /// </summary>
        public string PropertyType { get; set; }
    }
}
