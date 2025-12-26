using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Ipc
{
    /// <summary>
    /// Named pipe client for sending events to the VSIX extension
    /// </summary>
    public class NamedPipeClient : IDisposable
    {
        private const string PipeName = "WpfEventRecorder_{0}";
        private const int MaxRetries = 5;
        private const int RetryDelayMs = 1000;

        private readonly string _sessionId;
        private readonly object _writeLock = new object();
        private readonly JsonSerializerOptions _jsonOptions;
        private NamedPipeClientStream _pipeClient;
        private StreamWriter _writer;
        private bool _disposed;
        private bool _isConnected;

        /// <summary>
        /// Event raised when connected to the server
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Event raised when disconnected from the server
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Event raised when an error occurs
        /// </summary>
        public event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// Gets whether the client is connected
        /// </summary>
        public bool IsConnected => _isConnected && _pipeClient?.IsConnected == true;

        /// <summary>
        /// Gets the session ID
        /// </summary>
        public string SessionId => _sessionId;

        /// <summary>
        /// Creates a new named pipe client
        /// </summary>
        /// <param name="sessionId">Session ID to connect to</param>
        public NamedPipeClient(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// Connects to the named pipe server with retry logic
        /// </summary>
        public async Task<bool> ConnectAsync(CancellationToken ct = default)
        {
            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    _pipeClient = new NamedPipeClientStream(
                        ".",
                        string.Format(PipeName, _sessionId),
                        PipeDirection.Out,
                        PipeOptions.Asynchronous);

                    await _pipeClient.ConnectAsync(5000, ct);
                    _writer = new StreamWriter(_pipeClient, Encoding.UTF8) { AutoFlush = true };
                    _isConnected = true;

                    Connected?.Invoke(this, EventArgs.Empty);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                    await Task.Delay(RetryDelayMs, ct);
                }
            }

            return false;
        }

        /// <summary>
        /// Sends a record entry to the server
        /// </summary>
        public async Task<bool> SendAsync(RecordEntry entry)
        {
            if (!IsConnected)
            {
                return false;
            }

            try
            {
                var json = JsonSerializer.Serialize(entry, _jsonOptions);

                lock (_writeLock)
                {
                    _writer.WriteLine(json);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                HandleDisconnection();
                return false;
            }
        }

        /// <summary>
        /// Sends a record entry synchronously
        /// </summary>
        public bool Send(RecordEntry entry)
        {
            if (!IsConnected)
            {
                return false;
            }

            try
            {
                var json = JsonSerializer.Serialize(entry, _jsonOptions);

                lock (_writeLock)
                {
                    _writer.WriteLine(json);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                HandleDisconnection();
                return false;
            }
        }

        private void HandleDisconnection()
        {
            if (_isConnected)
            {
                _isConnected = false;
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Disconnects from the server
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _writer?.Dispose();
                _pipeClient?.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
            finally
            {
                _writer = null;
                _pipeClient = null;
                HandleDisconnection();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Disconnect();
        }
    }
}
