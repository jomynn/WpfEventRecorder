namespace WpfEventRecorder.Core.Attributes;

/// <summary>
/// Marks a ViewModel class for recording.
/// When applied, all command executions and property changes will be recorded.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RecordViewModelAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to record property changes.
    /// </summary>
    public bool RecordPropertyChanges { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to record command executions.
    /// </summary>
    public bool RecordCommands { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom name for the ViewModel in recordings.
    /// </summary>
    public string? DisplayName { get; set; }

    public RecordViewModelAttribute()
    {
    }

    public RecordViewModelAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}
