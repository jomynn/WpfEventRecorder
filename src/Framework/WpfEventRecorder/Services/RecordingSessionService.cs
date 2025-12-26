using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using WpfEventRecorder.Core.Ipc;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Services
{
    /// <summary>
    /// Service for managing recording sessions and IPC communication with the target app
    /// </summary>
    public class RecordingSessionService : IDisposable
    {
        private static RecordingSessionService _instance;
        private static readonly object _lock = new object();

        private readonly ObservableCollection<RecordEntry> _entries;
        private readonly List<RecordEntry> _allEntries;
        private NamedPipeServer _pipeServer;
        private string _currentSessionId;
        private bool _isRecording;
        private bool _disposed;
        private DateTime _sessionStartTime;

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static RecordingSessionService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new RecordingSessionService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the collection of recorded entries
        /// </summary>
        public ObservableCollection<RecordEntry> Entries => _entries;

        /// <summary>
        /// Gets all entries as a list
        /// </summary>
        public IReadOnlyList<RecordEntry> AllEntries => _allEntries;

        /// <summary>
        /// Gets the current session ID
        /// </summary>
        public string SessionId => _currentSessionId;

        /// <summary>
        /// Gets whether recording is active
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// Gets the session start time
        /// </summary>
        public DateTime SessionStartTime => _sessionStartTime;

        /// <summary>
        /// Gets the entry count
        /// </summary>
        public int EntryCount => _allEntries.Count;

        /// <summary>
        /// Event raised when a new entry is received
        /// </summary>
        public event EventHandler<RecordEntry> EntryReceived;

        /// <summary>
        /// Event raised when recording state changes
        /// </summary>
        public event EventHandler<bool> RecordingStateChanged;

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

        private RecordingSessionService()
        {
            _entries = new ObservableCollection<RecordEntry>();
            _allEntries = new List<RecordEntry>();
        }

        /// <summary>
        /// Starts a new recording session
        /// </summary>
        /// <returns>Session ID for the target app to connect</returns>
        public async Task<string> StartSessionAsync()
        {
            if (_isRecording)
            {
                StopSession();
            }

            _currentSessionId = Guid.NewGuid().ToString("N");
            _sessionStartTime = DateTime.UtcNow;
            _isRecording = true;

            // Create and start the named pipe server
            _pipeServer = new NamedPipeServer(_currentSessionId);
            _pipeServer.EntryReceived += OnEntryReceived;
            _pipeServer.ClientConnected += (s, e) => ClientConnected?.Invoke(this, EventArgs.Empty);
            _pipeServer.ClientDisconnected += (s, e) => ClientDisconnected?.Invoke(this, EventArgs.Empty);
            _pipeServer.ErrorOccurred += (s, e) => ErrorOccurred?.Invoke(this, e);

            // Start listening in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _pipeServer.StartAsync();
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                }
            });

            RecordingStateChanged?.Invoke(this, true);

            return _currentSessionId;
        }

        /// <summary>
        /// Stops the current recording session
        /// </summary>
        public void StopSession()
        {
            if (!_isRecording) return;

            _isRecording = false;

            if (_pipeServer != null)
            {
                _pipeServer.EntryReceived -= OnEntryReceived;
                _pipeServer.Stop();
                _pipeServer.Dispose();
                _pipeServer = null;
            }

            RecordingStateChanged?.Invoke(this, false);
        }

        /// <summary>
        /// Clears all recorded entries
        /// </summary>
        public void ClearEntries()
        {
            _entries.Clear();
            _allEntries.Clear();
        }

        /// <summary>
        /// Gets a recording session object with all entries
        /// </summary>
        public RecordingSession GetSession()
        {
            return new RecordingSession
            {
                SessionId = _currentSessionId ?? Guid.NewGuid().ToString(),
                SessionName = $"Session_{_sessionStartTime:yyyyMMdd_HHmmss}",
                StartTime = _sessionStartTime,
                EndTime = DateTime.UtcNow,
                Entries = new List<RecordEntry>(_allEntries)
            };
        }

        /// <summary>
        /// Adds an entry manually (for testing or external sources)
        /// </summary>
        public void AddEntry(RecordEntry entry)
        {
            if (entry == null) return;

            _allEntries.Add(entry);
            _entries.Add(entry);
            EntryReceived?.Invoke(this, entry);
        }

        private void OnEntryReceived(object sender, RecordEntry entry)
        {
            if (entry == null) return;

            // Marshal to UI thread if needed
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _allEntries.Add(entry);
                    _entries.Add(entry);
                    EntryReceived?.Invoke(this, entry);
                }));
            }
            else
            {
                _allEntries.Add(entry);
                _entries.Add(entry);
                EntryReceived?.Invoke(this, entry);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopSession();
        }
    }
}
