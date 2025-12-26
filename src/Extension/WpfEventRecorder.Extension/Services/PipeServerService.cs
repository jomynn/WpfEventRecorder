using WpfEventRecorder.Core.Communication;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Implementation of pipe server service.
/// </summary>
public class PipeServerService : IPipeServerService, IDisposable
{
    private readonly IRecordingSessionService _sessionService;
    private PipeServer? _pipeServer;
    private bool _disposed;

    public bool IsRunning => _pipeServer?.IsListening == true;
    public bool IsClientConnected { get; private set; }

    public event EventHandler? ClientConnected;
    public event EventHandler? ClientDisconnected;

    public PipeServerService(IRecordingSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task StartAsync()
    {
        if (IsRunning) return;

        _pipeServer = new PipeServer("WpfEventRecorder");

        _pipeServer.ClientConnected += (_, _) =>
        {
            IsClientConnected = true;
            ClientConnected?.Invoke(this, EventArgs.Empty);
        };

        _pipeServer.ClientDisconnected += (_, _) =>
        {
            IsClientConnected = false;
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
        };

        _pipeServer.EventReceived += (_, @event) =>
        {
            if (_sessionService is RecordingSessionService service)
            {
                service.AddEvent(@event);
            }
        };

        await _pipeServer.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_pipeServer != null)
        {
            await _pipeServer.StopAsync();
            _pipeServer.Dispose();
            _pipeServer = null;
        }

        IsClientConnected = false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _pipeServer?.Dispose();
    }
}
