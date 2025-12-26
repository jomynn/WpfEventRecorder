using System;

namespace WpfEventRecorder.Core.Attributes
{
    /// <summary>
    /// Marks a ViewModel class for automatic recording of property changes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class RecordViewModelAttribute : Attribute
    {
        /// <summary>
        /// Optional name to use in recorded events
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether to record all properties by default
        /// </summary>
        public bool RecordAllProperties { get; set; } = true;

        /// <summary>
        /// Creates a new RecordViewModel attribute
        /// </summary>
        public RecordViewModelAttribute()
        {
        }

        /// <summary>
        /// Creates a new RecordViewModel attribute with a custom name
        /// </summary>
        /// <param name="name">Name to use in recorded events</param>
        public RecordViewModelAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Marks a specific property for recording
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class RecordPropertyAttribute : Attribute
    {
        /// <summary>
        /// Optional display name for the property
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Whether to include the value in recordings (set to false for sensitive data)
        /// </summary>
        public bool IncludeValue { get; set; } = true;

        /// <summary>
        /// Optional group name for organizing recorded events
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Creates a new RecordProperty attribute
        /// </summary>
        public RecordPropertyAttribute()
        {
        }

        /// <summary>
        /// Creates a new RecordProperty attribute with a display name
        /// </summary>
        /// <param name="displayName">Display name for the property</param>
        public RecordPropertyAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Excludes a property or class from recording
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class IgnoreRecordingAttribute : Attribute
    {
        /// <summary>
        /// Optional reason for ignoring
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Creates a new IgnoreRecording attribute
        /// </summary>
        public IgnoreRecordingAttribute()
        {
        }

        /// <summary>
        /// Creates a new IgnoreRecording attribute with a reason
        /// </summary>
        /// <param name="reason">Reason for ignoring</param>
        public IgnoreRecordingAttribute(string reason)
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// Marks a command for recording when executed
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class RecordCommandAttribute : Attribute
    {
        /// <summary>
        /// Optional display name for the command
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Whether to record the command parameter
        /// </summary>
        public bool RecordParameter { get; set; } = true;

        /// <summary>
        /// Creates a new RecordCommand attribute
        /// </summary>
        public RecordCommandAttribute()
        {
        }

        /// <summary>
        /// Creates a new RecordCommand attribute with a display name
        /// </summary>
        /// <param name="displayName">Display name for the command</param>
        public RecordCommandAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Marks a method as an API endpoint handler for recording
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class RecordApiCallAttribute : Attribute
    {
        /// <summary>
        /// Optional description of the API call
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether to record request body
        /// </summary>
        public bool RecordRequest { get; set; } = true;

        /// <summary>
        /// Whether to record response body
        /// </summary>
        public bool RecordResponse { get; set; } = true;

        /// <summary>
        /// Creates a new RecordApiCall attribute
        /// </summary>
        public RecordApiCallAttribute()
        {
        }

        /// <summary>
        /// Creates a new RecordApiCall attribute with a description
        /// </summary>
        /// <param name="description">Description of the API call</param>
        public RecordApiCallAttribute(string description)
        {
            Description = description;
        }
    }
}
