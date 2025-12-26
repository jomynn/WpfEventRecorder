using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Ipc
{
    /// <summary>
    /// Named pipe server for receiving events from the recording client
    /// </summary>
    public class NamedPipeServer : IDisposable
    {
        private const string PipeName = "WpfEventRecorder_{0}";
        private readonly string _sessionId;
        private readonly CancellationTokenSource _cts;
        private NamedPipeServerStream _pipeServer;
        private bool _disposed;
        private bool _isListening;

        /// <summary>
        /// Event raised when a record entry is received
        /// </summary>
        public event EventHandler<RecordEntry> EntryReceived;

        /// <summary>
        /// Event raised when a client connects
        /// </summary>
        public event EventHandler ClientConnected;

        /// <summary>
        /// Event raised when a client disconnects
        /// </summary>
        public event EventHandler ClientDisconnected;

        /// <summary>
        /// Event raised when an error occurs
        /// </summary>
        public event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// Gets the session ID for this server
        /// </summary>
        public string SessionId => _sessionId;

        /// <summary>
        /// Gets the full pipe name
        /// </summary>
        public string FullPipeName => string.Format(PipeName, _sessionId);

        /// <summary>
        /// Gets whether the server is currently listening
        /// </summary>
        public bool IsListening => _isListening;

        /// <summary>
        /// Creates a new named pipe server
        /// </summary>
        /// <param name="sessionId">Unique session identifier</param>
        public NamedPipeServer(string sessionId = null)
        {
            _sessionId = sessionId ?? Guid.NewGuid().ToString("N");
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Starts listening for connections
        /// </summary>
        public async Task StartAsync()
        {
            if (_isListening) return;
            _isListening = true;

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(
                        FullPipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous);

                    await _pipeServer.WaitForConnectionAsync(_cts.Token);
                    ClientConnected?.Invoke(this, EventArgs.Empty);

                    await ProcessMessagesAsync(_cts.Token);

                    ClientDisconnected?.Invoke(this, EventArgs.Empty);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                }
                finally
                {
                    ClosePipe();
                }
            }

            _isListening = false;
        }

        private async Task ProcessMessagesAsync(CancellationToken ct)
        {
            using var reader = new StreamReader(_pipeServer, Encoding.UTF8);

            while (_pipeServer.IsConnected && !ct.IsCancellationRequested)
            {
                try
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;

                    var entry = JsonSerializer.Deserialize<RecordEntry>(line, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (entry != null)
                    {
                        EntryReceived?.Invoke(this, entry);
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                }
            }
        }

        private void ClosePipe()
        {
            try
            {
                _pipeServer?.Dispose();
                _pipeServer = null;
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();
            ClosePipe();
            _isListening = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Stop();
            _cts.Dispose();
        }
    }
}
