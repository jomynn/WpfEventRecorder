namespace WpfEventRecorder.Core.Events;

/// <summary>
/// Types of navigation events.
/// </summary>
public enum NavigationType
{
    ViewNavigation,
    TabChanged,
    WindowOpened,
    WindowClosed,
    DialogOpened,
    DialogClosed,
    PageNavigation
}

/// <summary>
/// Represents a navigation event within the application.
/// </summary>
public class NavigationEvent : RecordedEvent
{
    public override string EventType => "Navigation";

    /// <summary>
    /// Type of navigation.
    /// </summary>
    public NavigationType NavigationType { get; set; }

    /// <summary>
    /// Source view/page.
    /// </summary>
    public string? FromView { get; set; }

    /// <summary>
    /// Target view/page.
    /// </summary>
    public string? ToView { get; set; }

    /// <summary>
    /// Navigation parameter.
    /// </summary>
    public object? Parameter { get; set; }

    /// <summary>
    /// ViewModel type of the source.
    /// </summary>
    public string? FromViewModelType { get; set; }

    /// <summary>
    /// ViewModel type of the target.
    /// </summary>
    public string? ToViewModelType { get; set; }

    /// <summary>
    /// Tab index (for tab navigation).
    /// </summary>
    public int? TabIndex { get; set; }

    /// <summary>
    /// Tab header (for tab navigation).
    /// </summary>
    public string? TabHeader { get; set; }

    public override string GetDescription()
    {
        return NavigationType switch
        {
            NavigationType.ViewNavigation => $"Navigate from {FromView ?? "Unknown"} to {ToView ?? "Unknown"}",
            NavigationType.TabChanged => $"Switch to tab '{TabHeader ?? TabIndex?.ToString() ?? "Unknown"}'",
            NavigationType.WindowOpened => $"Open window {ToView ?? "Unknown"}",
            NavigationType.WindowClosed => $"Close window {FromView ?? "Unknown"}",
            NavigationType.DialogOpened => $"Open dialog {ToView ?? "Unknown"}",
            NavigationType.DialogClosed => $"Close dialog {FromView ?? "Unknown"}",
            NavigationType.PageNavigation => $"Navigate to page {ToView ?? "Unknown"}",
            _ => $"{NavigationType}"
        };
    }
}
