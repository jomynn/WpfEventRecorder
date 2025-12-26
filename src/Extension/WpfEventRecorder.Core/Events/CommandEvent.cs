namespace WpfEventRecorder.Core.Events;

/// <summary>
/// Represents a command execution event.
/// </summary>
public class CommandEvent : RecordedEvent
{
    public override string EventType => "Command";

    /// <summary>
    /// Name of the command.
    /// </summary>
    public string? CommandName { get; set; }

    /// <summary>
    /// Type name of the command class.
    /// </summary>
    public string? CommandType { get; set; }

    /// <summary>
    /// Parameter passed to the command.
    /// </summary>
    public object? CommandParameter { get; set; }

    /// <summary>
    /// ViewModel type that contains the command.
    /// </summary>
    public string? ViewModelType { get; set; }

    /// <summary>
    /// Whether the command execution was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if the command failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of command execution in milliseconds.
    /// </summary>
    public long? ExecutionDurationMs { get; set; }

    public override string GetDescription()
    {
        var param = CommandParameter != null ? $" with parameter '{CommandParameter}'" : "";
        var status = IsSuccess ? "" : " (failed)";
        return $"Execute {CommandName ?? CommandType ?? "command"}{param}{status}";
    }
}
