namespace WpfEventRecorder.Core.Attributes;

/// <summary>
/// Excludes a class, property, or method from recording.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method,
    AllowMultiple = false, Inherited = true)]
public class IgnoreRecordingAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the reason for ignoring this member.
    /// </summary>
    public string? Reason { get; set; }

    public IgnoreRecordingAttribute()
    {
    }

    public IgnoreRecordingAttribute(string reason)
    {
        Reason = reason;
    }
}
