using System.Text.Json.Serialization;

namespace WpfEventRecorder.Core.Communication;

/// <summary>
/// Message types for pipe communication.
/// </summary>
public enum PipeMessageType
{
    Event,
    Command,
    Status,
    Ping,
    Pong,
    Error
}

/// <summary>
/// Base class for pipe messages.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EventPipeMessage), "event")]
[JsonDerivedType(typeof(CommandPipeMessage), "command")]
[JsonDerivedType(typeof(StatusPipeMessage), "status")]
[JsonDerivedType(typeof(PingPipeMessage), "ping")]
[JsonDerivedType(typeof(PongPipeMessage), "pong")]
[JsonDerivedType(typeof(ErrorPipeMessage), "error")]
public abstract class PipeMessage
{
    /// <summary>
    /// Message ID.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp of the message.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// Type of the message.
    /// </summary>
    public abstract PipeMessageType MessageType { get; }
}

/// <summary>
/// Message containing a recorded event.
/// </summary>
public class EventPipeMessage : PipeMessage
{
    public override PipeMessageType MessageType => PipeMessageType.Event;

    /// <summary>
    /// The serialized event data.
    /// </summary>
    public string? EventData { get; set; }

    /// <summary>
    /// The event type name.
    /// </summary>
    public string? EventTypeName { get; set; }
}

/// <summary>
/// Message containing a command from the extension.
/// </summary>
public class CommandPipeMessage : PipeMessage
{
    public override PipeMessageType MessageType => PipeMessageType.Command;

    /// <summary>
    /// The command name.
    /// </summary>
    public string? CommandName { get; set; }

    /// <summary>
    /// Command parameters.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = new();
}

/// <summary>
/// Message containing status information.
/// </summary>
public class StatusPipeMessage : PipeMessage
{
    public override PipeMessageType MessageType => PipeMessageType.Status;

    /// <summary>
    /// Whether recording is active.
    /// </summary>
    public bool IsRecording { get; set; }

    /// <summary>
    /// Whether recording is paused.
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Number of events recorded.
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Application name.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Process ID.
    /// </summary>
    public int ProcessId { get; set; }
}

/// <summary>
/// Ping message for connection keep-alive.
/// </summary>
public class PingPipeMessage : PipeMessage
{
    public override PipeMessageType MessageType => PipeMessageType.Ping;
}

/// <summary>
/// Pong message in response to ping.
/// </summary>
public class PongPipeMessage : PipeMessage
{
    public override PipeMessageType MessageType => PipeMessageType.Pong;

    /// <summary>
    /// ID of the ping message this is responding to.
    /// </summary>
    public string? PingId { get; set; }
}

/// <summary>
/// Error message.
/// </summary>
public class ErrorPipeMessage : PipeMessage
{
    public override PipeMessageType MessageType => PipeMessageType.Error;

    /// <summary>
    /// Error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace (if available).
    /// </summary>
    public string? StackTrace { get; set; }
}
