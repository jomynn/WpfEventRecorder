using System.Windows;
using WpfEventRecorder.Core.Communication;
using WpfEventRecorder.Core.Http;
using WpfEventRecorder.Core.Instrumentation;

namespace WpfEventRecorder.Core.Recording;

/// <summary>
/// Bootstrapper for initializing recording in a WPF application.
/// </summary>
public static class RecordingBootstrapper
{
    private static RecordingSession? _session;
    private static PipeClient? _pipeClient;
    private static ViewInstrumenter? _instrumenter;
    private static bool _isInitialized;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the current recording session.
    /// </summary>
    public static RecordingSession? CurrentSession => _session;

    /// <summary>
    /// Gets whether recording is initialized.
    /// </summary>
    public static bool IsInitialized => _isInitialized;

    /// <summary>
    /// Event raised when recording status changes.
    /// </summary>
    public static event EventHandler<bool>? RecordingStatusChanged;

    /// <summary>
    /// Initializes the recording infrastructure for the application.
    /// </summary>
    /// <param name="application">The WPF application instance.</param>
    /// <param name="configuration">Optional recording configuration.</param>
    public static async Task InitializeAsync(Application application, RecordingConfiguration? configuration = null)
    {
        lock (_lock)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
        }

        configuration ??= new RecordingConfiguration();

        _session = new RecordingSession
        {
            Configuration = configuration,
            ApplicationName = application.GetType().Assembly.GetName().Name,
            ProcessId = Environment.ProcessId
        };

        // Initialize pipe client
        _pipeClient = new PipeClient(configuration.PipeName);
        await _pipeClient.ConnectAsync();

        // Forward events to pipe
        _session.EventRecorded += async (_, @event) =>
        {
            if (_pipeClient?.IsConnected == true)
            {
                await _pipeClient.SendEventAsync(@event);
            }
        };

        // Initialize view instrumenter
        _instrumenter = new ViewInstrumenter(_session, configuration);

        // Hook into application events
        application.Activated += (_, _) => InstrumentActiveWindows();
        if (application.MainWindow != null)
        {
            application.MainWindow.Loaded += (_, _) =>
            {
                if (application.MainWindow != null)
                    _instrumenter.InstrumentWindow(application.MainWindow);
            };
        }

        // Instrument existing windows
        foreach (Window window in application.Windows)
        {
            _instrumenter.InstrumentWindow(window);
        }

        RecordingStatusChanged?.Invoke(null, true);
    }

    /// <summary>
    /// Creates an HttpClient with recording enabled.
    /// </summary>
    public static HttpClient CreateRecordingHttpClient(HttpMessageHandler? innerHandler = null)
    {
        var recordingHandler = new RecordingHttpHandler(_session, innerHandler);
        return new HttpClient(recordingHandler);
    }

    /// <summary>
    /// Instruments all active windows.
    /// </summary>
    private static void InstrumentActiveWindows()
    {
        if (_instrumenter == null || Application.Current == null)
            return;

        foreach (Window window in Application.Current.Windows)
        {
            _instrumenter.InstrumentWindow(window);
        }
    }

    /// <summary>
    /// Stops recording and disconnects.
    /// </summary>
    public static async Task ShutdownAsync()
    {
        lock (_lock)
        {
            if (!_isInitialized)
                return;

            _isInitialized = false;
        }

        _session?.Clear();
        _session = null;

        if (_pipeClient != null)
        {
            await _pipeClient.DisconnectAsync();
            _pipeClient.Dispose();
            _pipeClient = null;
        }

        _instrumenter = null;
        RecordingStatusChanged?.Invoke(null, false);
    }

    /// <summary>
    /// Records a custom event.
    /// </summary>
    public static void RecordCustomEvent(Core.Events.RecordedEvent @event)
    {
        _session?.AddEvent(@event);
    }
}
