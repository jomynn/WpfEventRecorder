using System.IO.Pipes;
using System.Text;
using WpfEventRecorder.Core.Events;

namespace WpfEventRecorder.Core.Communication;

/// <summary>
/// Server for receiving events from target applications via named pipe.
/// </summary>
public class PipeServer : IDisposable
{
    private readonly string _pipeName;
    private NamedPipeServerStream? _pipeStream;
    private StreamReader? _reader;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _listenTask;
    private bool _disposed;

    public bool IsListening { get; private set; }

    public event EventHandler<RecordedEvent>? EventReceived;
    public event EventHandler<PipeMessage>? MessageReceived;
    public event EventHandler? ClientConnected;
    public event EventHandler? ClientDisconnected;
    public event EventHandler<Exception>? ErrorOccurred;

    public PipeServer(string pipeName = "WpfEventRecorder")
    {
        _pipeName = pipeName;
    }

    /// <summary>
    /// Starts listening for connections.
    /// </summary>
    public async Task StartAsync()
    {
        if (IsListening) return;

        _cancellationTokenSource = new CancellationTokenSource();
        IsListening = true;

        _listenTask = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await AcceptConnectionAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                    await Task.Delay(1000); // Wait before retrying
                }
            }
        });

        await Task.CompletedTask;
    }

    private async Task AcceptConnectionAsync(CancellationToken cancellationToken)
    {
        _pipeStream = new NamedPipeServerStream(_pipeName,
            PipeDirection.In, 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        await _pipeStream.WaitForConnectionAsync(cancellationToken);
        ClientConnected?.Invoke(this, EventArgs.Empty);

        _reader = new StreamReader(_pipeStream, Encoding.UTF8);

        try
        {
            while (_pipeStream.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                var line = await _reader.ReadLineAsync(cancellationToken);
                if (line == null) break;

                ProcessMessage(line);
            }
        }
        finally
        {
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
            _reader?.Dispose();
            _pipeStream?.Dispose();
            _reader = null;
            _pipeStream = null;
        }
    }

    private void ProcessMessage(string json)
    {
        try
        {
            var message = EventSerializer.DeserializeMessage(json);
            if (message == null) return;

            MessageReceived?.Invoke(this, message);

            if (message is EventPipeMessage eventMessage)
            {
                var @event = EventSerializer.ExtractEvent(eventMessage);
                if (@event != null)
                {
                    EventReceived?.Invoke(this, @event);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    public async Task StopAsync()
    {
        if (!IsListening) return;

        IsListening = false;
        _cancellationTokenSource?.Cancel();

        if (_listenTask != null)
        {
            try
            {
                await _listenTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        _reader?.Dispose();
        _pipeStream?.Dispose();
        _cancellationTokenSource?.Dispose();

        _reader = null;
        _pipeStream = null;
        _cancellationTokenSource = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cancellationTokenSource?.Cancel();
        _reader?.Dispose();
        _pipeStream?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
