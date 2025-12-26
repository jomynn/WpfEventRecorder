using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfEventRecorder.Core.Hooks;
using WpfEventRecorder.Core.Ipc;
using WpfEventRecorder.Core.Services;

namespace WpfEventRecorder.Core.Infrastructure
{
    /// <summary>
    /// Bootstraps the recording infrastructure for a WPF application
    /// </summary>
    public class RecordingBootstrapper : IDisposable
    {
        private static RecordingBootstrapper _instance;
        private static readonly object _lock = new object();

        private readonly RecordingHub _hub;
        private readonly UIHook _uiHook;
        private readonly ViewInstrumenter _viewInstrumenter;
        private IpcRecordingBridge _ipcBridge;
        private bool _isInitialized;
        private bool _disposed;

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static RecordingBootstrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new RecordingBootstrapper();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets whether recording is currently active
        /// </summary>
        public bool IsRecording => _hub.IsRecording;

        /// <summary>
        /// Gets the recording hub
        /// </summary>
        public RecordingHub Hub => _hub;

        /// <summary>
        /// Event raised when recording state changes
        /// </summary>
        public event EventHandler<bool> RecordingStateChanged;

        /// <summary>
        /// Event raised when connected to VSIX
        /// </summary>
        public event EventHandler<bool> ConnectionStateChanged;

        private RecordingBootstrapper()
        {
            _hub = RecordingHub.Instance;
            _uiHook = new UIHook();
            _viewInstrumenter = new ViewInstrumenter();

            _hub.SetUIHook(_uiHook);
            _hub.RecordingStateChanged += (s, e) => RecordingStateChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Initializes recording for the current application
        /// </summary>
        /// <param name="app">The WPF application instance</param>
        public void Initialize(Application app)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            // Hook into application events
            app.Activated += OnApplicationActivated;

            // Instrument existing windows
            foreach (Window window in app.Windows)
            {
                _viewInstrumenter.InstrumentWindow(window);
            }
        }

        /// <summary>
        /// Initializes recording and connects to VSIX via Named Pipe
        /// </summary>
        /// <param name="app">The WPF application instance</param>
        /// <param name="sessionId">Session ID provided by VSIX</param>
        public async Task InitializeWithIpcAsync(Application app, string sessionId)
        {
            Initialize(app);

            _ipcBridge = new IpcRecordingBridge(sessionId);
            _ipcBridge.ConnectionStateChanged += (s, e) => ConnectionStateChanged?.Invoke(this, e);

            await _ipcBridge.ConnectAsync();
        }

        /// <summary>
        /// Creates an HttpClient with recording capabilities
        /// </summary>
        public HttpClient CreateRecordingHttpClient()
        {
            var handler = _hub.CreateHttpHandler();
            return new HttpClient(handler);
        }

        /// <summary>
        /// Creates a DelegatingHandler for recording HTTP calls
        /// </summary>
        public RecordingHttpHandler CreateRecordingHandler()
        {
            return _hub.CreateHttpHandler();
        }

        /// <summary>
        /// Wraps an existing HttpClient handler with recording capabilities
        /// </summary>
        public HttpClient CreateRecordingHttpClient(HttpMessageHandler innerHandler)
        {
            var handler = _hub.CreateHttpHandler();
            handler.InnerHandler = innerHandler;
            return new HttpClient(handler);
        }

        /// <summary>
        /// Starts recording
        /// </summary>
        /// <param name="sessionName">Optional session name</param>
        public void StartRecording(string sessionName = null)
        {
            _hub.Start(sessionName);
        }

        /// <summary>
        /// Stops recording
        /// </summary>
        public void StopRecording()
        {
            _hub.Stop();
        }

        /// <summary>
        /// Clears all recorded events
        /// </summary>
        public void ClearRecording()
        {
            _hub.Clear();
        }

        /// <summary>
        /// Saves the recording to a file
        /// </summary>
        public void SaveRecording(string filePath)
        {
            _hub.SaveToFile(filePath);
        }

        /// <summary>
        /// Instruments a window for recording
        /// </summary>
        public void InstrumentWindow(Window window)
        {
            _viewInstrumenter.InstrumentWindow(window);
        }

        private void OnApplicationActivated(object sender, EventArgs e)
        {
            // Re-instrument windows when application is activated
            if (sender is Application app)
            {
                foreach (Window window in app.Windows)
                {
                    _viewInstrumenter.InstrumentWindow(window);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _ipcBridge?.Dispose();
            _uiHook?.Dispose();
            _hub?.Dispose();
        }
    }
}
