using System;
using System.Threading;
using System.Threading.Tasks;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;

namespace WpfEventRecorder.Core.Ipc
{
    /// <summary>
    /// Bridge that connects RecordingHub to IPC communication
    /// </summary>
    public class IpcRecordingBridge : IDisposable
    {
        private readonly RecordingHub _hub;
        private readonly NamedPipeClient _client;
        private readonly IDisposable _subscription;
        private bool _disposed;

        /// <summary>
        /// Event raised when connection state changes
        /// </summary>
        public event EventHandler<bool> ConnectionStateChanged;

        /// <summary>
        /// Gets whether the bridge is connected
        /// </summary>
        public bool IsConnected => _client.IsConnected;

        /// <summary>
        /// Creates a new IPC recording bridge
        /// </summary>
        /// <param name="sessionId">The session ID to connect to</param>
        public IpcRecordingBridge(string sessionId)
        {
            _hub = RecordingHub.Instance;
            _client = new NamedPipeClient(sessionId);

            _client.Connected += (s, e) => ConnectionStateChanged?.Invoke(this, true);
            _client.Disconnected += (s, e) => ConnectionStateChanged?.Invoke(this, false);

            // Subscribe to hub entries and forward to pipe
            _subscription = _hub.Entries.Subscribe(OnEntryRecorded);
        }

        /// <summary>
        /// Connects to the VSIX extension
        /// </summary>
        public async Task<bool> ConnectAsync(CancellationToken ct = default)
        {
            return await _client.ConnectAsync(ct);
        }

        private void OnEntryRecorded(RecordEntry entry)
        {
            if (_client.IsConnected)
            {
                _client.Send(entry);
            }
        }

        /// <summary>
        /// Disconnects from the VSIX extension
        /// </summary>
        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _subscription?.Dispose();
            _client?.Dispose();
        }
    }
}
