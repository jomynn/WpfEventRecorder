namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Service for managing the named pipe server.
/// </summary>
public interface IPipeServerService
{
    /// <summary>
    /// Gets whether the server is running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets whether a client is connected.
    /// </summary>
    bool IsClientConnected { get; }

    /// <summary>
    /// Event raised when a client connects.
    /// </summary>
    event EventHandler? ClientConnected;

    /// <summary>
    /// Event raised when a client disconnects.
    /// </summary>
    event EventHandler? ClientDisconnected;

    /// <summary>
    /// Starts the pipe server.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stops the pipe server.
    /// </summary>
    Task StopAsync();
}
