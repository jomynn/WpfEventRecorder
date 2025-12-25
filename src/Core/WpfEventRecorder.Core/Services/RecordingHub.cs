using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using WpfEventRecorder.Core.Hooks;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Services
{
    /// <summary>
    /// Central hub for managing recording sessions and coordinating hooks
    /// </summary>
    public class RecordingHub : IDisposable
    {
        private static readonly Lazy<RecordingHub> _instance =
            new Lazy<RecordingHub>(() => new RecordingHub(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly Subject<RecordEntry> _entriesSubject = new Subject<RecordEntry>();
        private readonly List<RecordEntry> _entries = new List<RecordEntry>();
        private readonly object _lock = new object();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        private UIHook? _uiHook;
        private RecordingHttpHandler? _httpHandler;
        private RecordingSession? _currentSession;
        private bool _isRecording;
        private bool _disposed;
        private string? _currentCorrelationId;

        /// <summary>
        /// Singleton instance of the recording hub
        /// </summary>
        public static RecordingHub Instance => _instance.Value;

        /// <summary>
        /// Observable stream of all recorded entries
        /// </summary>
        public IObservable<RecordEntry> Entries => _entriesSubject.AsObservable();

        /// <summary>
        /// Whether recording is currently active
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// Number of entries in the current session
        /// </summary>
        public int EntryCount
        {
            get
            {
                lock (_lock)
                {
                    return _entries.Count;
                }
            }
        }

        /// <summary>
        /// Current recording session
        /// </summary>
        public RecordingSession? CurrentSession => _currentSession;

        /// <summary>
        /// Event raised when recording state changes
        /// </summary>
        public event EventHandler<bool>? RecordingStateChanged;

        /// <summary>
        /// Event raised when a new entry is recorded
        /// </summary>
        public event EventHandler<RecordEntry>? EntryRecorded;

        private RecordingHub()
        {
        }

        /// <summary>
        /// Sets up the UI hook for capturing UI events
        /// </summary>
        public void SetUIHook(UIHook hook)
        {
            _uiHook = hook ?? throw new ArgumentNullException(nameof(hook));

            var subscription = _uiHook.Events.Subscribe(entry =>
            {
                entry.CorrelationId = _currentCorrelationId;
                AddEntry(entry);
            });

            _subscriptions.Add(subscription);
        }

        /// <summary>
        /// Sets up the HTTP handler for capturing API calls
        /// </summary>
        public void SetHttpHandler(RecordingHttpHandler handler)
        {
            _httpHandler = handler ?? throw new ArgumentNullException(nameof(handler));

            var subscription = _httpHandler.AllEvents.Subscribe(AddEntry);
            _subscriptions.Add(subscription);
        }

        /// <summary>
        /// Gets a pre-configured HTTP handler for use with HttpClient
        /// </summary>
        public RecordingHttpHandler CreateHttpHandler()
        {
            var handler = new RecordingHttpHandler();
            SetHttpHandler(handler);
            return handler;
        }

        /// <summary>
        /// Starts a new recording session
        /// </summary>
        /// <param name="sessionName">Optional session name</param>
        public void Start(string? sessionName = null)
        {
            if (_isRecording) return;

            lock (_lock)
            {
                _entries.Clear();
                _currentSession = RecordingSession.Create(sessionName ?? $"Session_{DateTime.Now:yyyyMMdd_HHmmss}");
                _isRecording = true;
                _currentCorrelationId = Guid.NewGuid().ToString();
            }

            _uiHook?.Start();
            if (_httpHandler != null)
            {
                _httpHandler.IsActive = true;
            }

            RecordingStateChanged?.Invoke(this, true);
        }

        /// <summary>
        /// Starts a new recording session for a specific target window
        /// </summary>
        /// <param name="targetWindow">The window to monitor</param>
        /// <param name="sessionName">Optional session name</param>
        public void Start(WindowInfo targetWindow, string? sessionName = null)
        {
            if (_isRecording) return;
            if (targetWindow == null) throw new ArgumentNullException(nameof(targetWindow));

            lock (_lock)
            {
                _entries.Clear();
                _currentSession = RecordingSession.Create(sessionName ?? $"Session_{DateTime.Now:yyyyMMdd_HHmmss}");
                _currentSession.TargetWindow = targetWindow;
                _isRecording = true;
                _currentCorrelationId = Guid.NewGuid().ToString();
            }

            _uiHook?.Start(targetWindow);
            if (_httpHandler != null)
            {
                _httpHandler.IsActive = true;
            }

            RecordingStateChanged?.Invoke(this, true);
        }

        /// <summary>
        /// Stops the current recording session
        /// </summary>
        public void Stop()
        {
            if (!_isRecording) return;

            _uiHook?.Stop();
            if (_httpHandler != null)
            {
                _httpHandler.IsActive = false;
            }

            lock (_lock)
            {
                _isRecording = false;
                if (_currentSession != null)
                {
                    _currentSession.EndTime = DateTime.UtcNow;
                    _currentSession.Entries = new List<RecordEntry>(_entries);
                }
            }

            RecordingStateChanged?.Invoke(this, false);
        }

        /// <summary>
        /// Clears all recorded entries
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
                _currentSession = null;
            }
        }

        /// <summary>
        /// Gets all recorded entries
        /// </summary>
        public IReadOnlyList<RecordEntry> GetEntries()
        {
            lock (_lock)
            {
                return new List<RecordEntry>(_entries);
            }
        }

        /// <summary>
        /// Adds a custom entry to the recording
        /// </summary>
        public void AddEntry(RecordEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            lock (_lock)
            {
                if (string.IsNullOrEmpty(entry.CorrelationId))
                {
                    entry.CorrelationId = _currentCorrelationId;
                }
                _entries.Add(entry);
            }

            _entriesSubject.OnNext(entry);
            EntryRecorded?.Invoke(this, entry);
        }

        /// <summary>
        /// Sets the current correlation ID for linking UI actions to API calls
        /// </summary>
        public void SetCorrelationId(string correlationId)
        {
            _currentCorrelationId = correlationId;
        }

        /// <summary>
        /// Generates a new correlation ID and returns it
        /// </summary>
        public string NewCorrelationId()
        {
            _currentCorrelationId = Guid.NewGuid().ToString();
            return _currentCorrelationId;
        }

        /// <summary>
        /// Saves the current session to a JSON file
        /// </summary>
        public void SaveToFile(string filePath)
        {
            RecordingSession session;

            lock (_lock)
            {
                if (_currentSession == null)
                {
                    session = RecordingSession.Create("Unsaved Session");
                    session.Entries = new List<RecordEntry>(_entries);
                }
                else
                {
                    session = _currentSession;
                    session.Entries = new List<RecordEntry>(_entries);
                }
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var json = JsonSerializer.Serialize(session, options);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a session from a JSON file
        /// </summary>
        public RecordingSession LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var session = JsonSerializer.Deserialize<RecordingSession>(json, options);

            if (session == null)
            {
                throw new InvalidOperationException("Failed to deserialize recording session");
            }

            lock (_lock)
            {
                _currentSession = session;
                _entries.Clear();
                _entries.AddRange(session.Entries);
            }

            return session;
        }

        /// <summary>
        /// Exports the current session as JSON string
        /// </summary>
        public string ExportAsJson()
        {
            RecordingSession session;

            lock (_lock)
            {
                if (_currentSession == null)
                {
                    session = RecordingSession.Create("Export");
                    session.Entries = new List<RecordEntry>(_entries);
                }
                else
                {
                    session = _currentSession;
                    session.Entries = new List<RecordEntry>(_entries);
                }
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Serialize(session, options);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Stop();

            foreach (var sub in _subscriptions)
            {
                sub.Dispose();
            }

            _uiHook?.Dispose();
            _entriesSubject.Dispose();
        }
    }
}
