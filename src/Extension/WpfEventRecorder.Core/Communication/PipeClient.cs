using System.IO.Pipes;
using System.Text;
using WpfEventRecorder.Core.Events;

namespace WpfEventRecorder.Core.Communication;

/// <summary>
/// Client for sending events to the VSIX extension via named pipe.
/// </summary>
public class PipeClient : IDisposable
{
    private readonly string _pipeName;
    private NamedPipeClientStream? _pipeStream;
    private StreamWriter? _writer;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private bool _disposed;

    public bool IsConnected => _pipeStream?.IsConnected == true;

    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public PipeClient(string pipeName = "WpfEventRecorder")
    {
        _pipeName = pipeName;
    }

    /// <summary>
    /// Connects to the pipe server.
    /// </summary>
    public async Task<bool> ConnectAsync(int timeoutMs = 5000)
    {
        try
        {
            _pipeStream = new NamedPipeClientStream(".", _pipeName,
                PipeDirection.Out, PipeOptions.Asynchronous);

            await _pipeStream.ConnectAsync(timeoutMs);
            _writer = new StreamWriter(_pipeStream, Encoding.UTF8) { AutoFlush = true };

            Connected?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
            return false;
        }
    }

    /// <summary>
    /// Sends a recorded event to the server.
    /// </summary>
    public async Task<bool> SendEventAsync(RecordedEvent @event)
    {
        var message = EventSerializer.CreateEventMessage(@event);
        return await SendMessageAsync(message);
    }

    /// <summary>
    /// Sends a pipe message to the server.
    /// </summary>
    public async Task<bool> SendMessageAsync(PipeMessage message)
    {
        if (!IsConnected)
        {
            // Try to reconnect
            if (!await ConnectAsync())
                return false;
        }

        await _sendLock.WaitAsync();
        try
        {
            var json = EventSerializer.SerializeMessage(message);
            await _writer!.WriteLineAsync(json);
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
            Disconnected?.Invoke(this, EventArgs.Empty);
            return false;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    /// Sends a status update to the server.
    /// </summary>
    public async Task<bool> SendStatusAsync(bool isRecording, bool isPaused, int eventCount,
        string? applicationName, int processId)
    {
        var message = new StatusPipeMessage
        {
            IsRecording = isRecording,
            IsPaused = isPaused,
            EventCount = eventCount,
            ApplicationName = applicationName,
            ProcessId = processId
        };
        return await SendMessageAsync(message);
    }

    /// <summary>
    /// Disconnects from the pipe server.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_writer != null)
        {
            await _writer.DisposeAsync();
            _writer = null;
        }

        if (_pipeStream != null)
        {
            await _pipeStream.DisposeAsync();
            _pipeStream = null;
        }

        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _writer?.Dispose();
        _pipeStream?.Dispose();
        _sendLock.Dispose();
    }
}
