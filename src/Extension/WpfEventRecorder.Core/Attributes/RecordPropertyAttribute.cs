namespace WpfEventRecorder.Core.Attributes;

/// <summary>
/// Marks a property for recording.
/// When applied, changes to this property will be recorded.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class RecordPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a custom name for the property in recordings.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets whether this is a sensitive property that should be masked.
    /// </summary>
    public bool IsSensitive { get; set; } = false;

    /// <summary>
    /// Gets or sets the mask to use for sensitive properties.
    /// </summary>
    public string SensitiveMask { get; set; } = "***";

    public RecordPropertyAttribute()
    {
    }

    public RecordPropertyAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}
