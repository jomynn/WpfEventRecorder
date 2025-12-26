namespace WpfEventRecorder.Core.Events;

/// <summary>
/// Types of input events.
/// </summary>
public enum InputEventType
{
    TextChanged,
    SelectionChanged,
    CheckedChanged,
    SliderChanged,
    DateChanged,
    ButtonClicked,
    KeyPressed,
    MouseClicked
}

/// <summary>
/// Represents a user input event.
/// </summary>
public class InputEvent : RecordedEvent
{
    public override string EventType => "Input";

    /// <summary>
    /// Type of input event.
    /// </summary>
    public InputEventType InputType { get; set; }

    /// <summary>
    /// Value before the change (if applicable).
    /// </summary>
    public object? OldValue { get; set; }

    /// <summary>
    /// Value after the change.
    /// </summary>
    public object? NewValue { get; set; }

    /// <summary>
    /// Binding path associated with the control.
    /// </summary>
    public string? BindingPath { get; set; }

    /// <summary>
    /// ViewModel property name associated with the input.
    /// </summary>
    public string? ViewModelProperty { get; set; }

    /// <summary>
    /// Control-specific information.
    /// </summary>
    public Dictionary<string, object?> ControlInfo { get; set; } = new();

    public override string GetDescription()
    {
        var target = !string.IsNullOrEmpty(AutomationId) ? AutomationId :
                     !string.IsNullOrEmpty(SourceElementName) ? SourceElementName :
                     SourceElementType ?? "Unknown";

        return InputType switch
        {
            InputEventType.TextChanged => $"Enter '{NewValue}' in {target}",
            InputEventType.SelectionChanged => $"Select '{NewValue}' in {target}",
            InputEventType.CheckedChanged => $"{(Convert.ToBoolean(NewValue) ? "Check" : "Uncheck")} {target}",
            InputEventType.SliderChanged => $"Set {target} slider to {NewValue}",
            InputEventType.DateChanged => $"Set {target} date to {NewValue}",
            InputEventType.ButtonClicked => $"Click {target}",
            InputEventType.KeyPressed => $"Press key '{NewValue}' in {target}",
            InputEventType.MouseClicked => $"Click {target}",
            _ => $"{InputType} on {target}"
        };
    }
}
