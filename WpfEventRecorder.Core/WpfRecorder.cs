using System;
using System.Net.Http;
using WpfEventRecorder.Core.Hooks;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;

namespace WpfEventRecorder.Core
{
    /// <summary>
    /// Main public API for WPF event recording
    /// </summary>
    public static class WpfRecorder
    {
        private static UIHook? _uiHook;
        private static bool _initialized;

        /// <summary>
        /// Gets the central recording hub instance
        /// </summary>
        public static RecordingHub Hub => RecordingHub.Instance;

        /// <summary>
        /// Whether recording is currently active
        /// </summary>
        public static bool IsRecording => Hub.IsRecording;

        /// <summary>
        /// Number of entries recorded in the current session
        /// </summary>
        public static int EntryCount => Hub.EntryCount;

        /// <summary>
        /// Event raised when recording state changes
        /// </summary>
        public static event EventHandler<bool>? RecordingStateChanged
        {
            add => Hub.RecordingStateChanged += value;
            remove => Hub.RecordingStateChanged -= value;
        }

        /// <summary>
        /// Event raised when a new entry is recorded
        /// </summary>
        public static event EventHandler<RecordEntry>? EntryRecorded
        {
            add => Hub.EntryRecorded += value;
            remove => Hub.EntryRecorded -= value;
        }

        /// <summary>
        /// Initializes the recorder with default settings
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            _uiHook = new UIHook();
            Hub.SetUIHook(_uiHook);

            _initialized = true;
        }

        /// <summary>
        /// Creates an HttpClient with recording capabilities
        /// </summary>
        public static HttpClient CreateHttpClient()
        {
            var handler = Hub.CreateHttpHandler();
            return new HttpClient(handler);
        }

        /// <summary>
        /// Gets a recording HTTP handler for custom HttpClient configuration
        /// </summary>
        public static RecordingHttpHandler CreateHttpHandler(HttpMessageHandler? innerHandler = null)
        {
            var handler = new RecordingHttpHandler(innerHandler);
            Hub.SetHttpHandler(handler);
            return handler;
        }

        /// <summary>
        /// Starts recording
        /// </summary>
        /// <param name="sessionName">Optional session name</param>
        public static void Start(string? sessionName = null)
        {
            Initialize();
            Hub.Start(sessionName);
        }

        /// <summary>
        /// Stops recording
        /// </summary>
        public static void Stop()
        {
            Hub.Stop();
        }

        /// <summary>
        /// Clears all recorded entries
        /// </summary>
        public static void Clear()
        {
            Hub.Clear();
        }

        /// <summary>
        /// Saves the current recording to a file
        /// </summary>
        public static void SaveToFile(string filePath)
        {
            Hub.SaveToFile(filePath);
        }

        /// <summary>
        /// Exports the current recording as JSON
        /// </summary>
        public static string ExportAsJson()
        {
            return Hub.ExportAsJson();
        }

        /// <summary>
        /// Records a custom event
        /// </summary>
        public static void RecordCustomEvent(string eventType, string? data = null)
        {
            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.Custom,
                Metadata = $"{{\"eventType\":\"{eventType}\",\"data\":{data ?? "null"}}}"
            };

            Hub.AddEntry(entry);
        }

        /// <summary>
        /// Records a UI click event manually
        /// </summary>
        public static void RecordClick(string controlType, string? controlName, string? text = null)
        {
            _uiHook?.RecordClick(controlType, controlName, null, text, null);
        }

        /// <summary>
        /// Records a text input event manually
        /// </summary>
        public static void RecordTextInput(string controlType, string? controlName, string? oldValue, string? newValue)
        {
            _uiHook?.RecordTextInput(controlType, controlName, null, oldValue, newValue, null);
        }

        /// <summary>
        /// Sets the correlation ID for linking events
        /// </summary>
        public static void SetCorrelationId(string correlationId)
        {
            Hub.SetCorrelationId(correlationId);
        }

        /// <summary>
        /// Generates and sets a new correlation ID
        /// </summary>
        public static string NewCorrelationId()
        {
            return Hub.NewCorrelationId();
        }
    }
}
