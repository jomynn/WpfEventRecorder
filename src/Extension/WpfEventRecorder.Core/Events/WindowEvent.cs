namespace WpfEventRecorder.Core.Events;

/// <summary>
/// Types of window events.
/// </summary>
public enum WindowEventType
{
    Opened,
    Closed,
    Activated,
    Deactivated,
    Minimized,
    Maximized,
    Restored,
    SizeChanged,
    LocationChanged
}

/// <summary>
/// Represents a window-level event.
/// </summary>
public class WindowEvent : RecordedEvent
{
    public override string EventType => "Window";

    /// <summary>
    /// Type of window event.
    /// </summary>
    public WindowEventType WindowEventType { get; set; }

    /// <summary>
    /// Title of the window.
    /// </summary>
    public string? WindowTitle { get; set; }

    /// <summary>
    /// Type name of the window class.
    /// </summary>
    public string? WindowType { get; set; }

    /// <summary>
    /// Window handle (HWND).
    /// </summary>
    public nint WindowHandle { get; set; }

    /// <summary>
    /// Window position X.
    /// </summary>
    public double? X { get; set; }

    /// <summary>
    /// Window position Y.
    /// </summary>
    public double? Y { get; set; }

    /// <summary>
    /// Window width.
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// Window height.
    /// </summary>
    public double? Height { get; set; }

    /// <summary>
    /// Previous window state (for state change events).
    /// </summary>
    public string? PreviousState { get; set; }

    /// <summary>
    /// Current window state.
    /// </summary>
    public string? CurrentState { get; set; }

    public override string GetDescription()
    {
        var target = !string.IsNullOrEmpty(WindowTitle) ? $"'{WindowTitle}'" :
                     !string.IsNullOrEmpty(WindowType) ? WindowType :
                     "Window";

        return WindowEventType switch
        {
            WindowEventType.Opened => $"Open {target}",
            WindowEventType.Closed => $"Close {target}",
            WindowEventType.Activated => $"Activate {target}",
            WindowEventType.Deactivated => $"Deactivate {target}",
            WindowEventType.Minimized => $"Minimize {target}",
            WindowEventType.Maximized => $"Maximize {target}",
            WindowEventType.Restored => $"Restore {target}",
            WindowEventType.SizeChanged => $"Resize {target} to {Width}x{Height}",
            WindowEventType.LocationChanged => $"Move {target} to ({X}, {Y})",
            _ => $"{WindowEventType} {target}"
        };
    }
}
