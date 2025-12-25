using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.ToolWindows
{
    /// <summary>
    /// View model for displaying a RecordEntry in the tool window
    /// </summary>
    public class RecordEntryViewModel : INotifyPropertyChanged
    {
        private readonly RecordEntry _entry;

        public RecordEntryViewModel(RecordEntry entry)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }

        /// <summary>
        /// The underlying record entry
        /// </summary>
        public RecordEntry Entry => _entry;

        /// <summary>
        /// Entry ID
        /// </summary>
        public Guid Id => _entry.Id;

        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTime Timestamp => _entry.Timestamp;

        /// <summary>
        /// Formatted timestamp
        /// </summary>
        public string TimestampFormatted => _entry.Timestamp.ToString("HH:mm:ss.fff");

        /// <summary>
        /// Type of entry
        /// </summary>
        public RecordEntryType EntryType => _entry.EntryType;

        /// <summary>
        /// Type icon based on entry type
        /// </summary>
        public string TypeIcon
        {
            get
            {
                return _entry.EntryType switch
                {
                    RecordEntryType.UIClick => "Click",
                    RecordEntryType.UIDoubleClick => "DblClick",
                    RecordEntryType.UITextInput => "Text",
                    RecordEntryType.UISelectionChange => "Select",
                    RecordEntryType.UIToggle => "Toggle",
                    RecordEntryType.UIKeyboardShortcut => "Key",
                    RecordEntryType.UIDragDrop => "Drag",
                    RecordEntryType.UIWindowOpen => "WinOpen",
                    RecordEntryType.UIWindowClose => "WinClose",
                    RecordEntryType.ApiRequest => "Request",
                    RecordEntryType.ApiResponse => "Response",
                    RecordEntryType.Custom => "Custom",
                    _ => "?"
                };
            }
        }

        /// <summary>
        /// Summary description of the entry
        /// </summary>
        public string Summary
        {
            get
            {
                if (_entry.UIInfo != null)
                {
                    var ui = _entry.UIInfo;
                    var name = ui.ControlName ?? ui.AutomationId ?? ui.ControlType;
                    var text = !string.IsNullOrEmpty(ui.Text) ? $" \"{ui.Text}\"" : "";
                    return $"{ui.ControlType}: {name}{text}";
                }

                if (_entry.ApiInfo != null)
                {
                    var api = _entry.ApiInfo;
                    if (_entry.EntryType == RecordEntryType.ApiRequest)
                    {
                        return $"{api.Method} {api.Path ?? api.Url}";
                    }
                    else
                    {
                        return $"{api.StatusCode} {api.Path ?? api.Url} ({_entry.DurationMs}ms)";
                    }
                }

                return _entry.Metadata ?? "Unknown event";
            }
        }

        /// <summary>
        /// Correlation ID
        /// </summary>
        public string? CorrelationId => _entry.CorrelationId;

        /// <summary>
        /// Duration in milliseconds
        /// </summary>
        public long? DurationMs => _entry.DurationMs;

        /// <summary>
        /// Whether this is a UI event
        /// </summary>
        public bool IsUIEvent => _entry.UIInfo != null;

        /// <summary>
        /// Whether this is an API event
        /// </summary>
        public bool IsApiEvent => _entry.ApiInfo != null;

        /// <summary>
        /// Whether this is a successful API response
        /// </summary>
        public bool IsSuccess => _entry.ApiInfo?.IsSuccess ?? true;

        /// <summary>
        /// Control type for UI events
        /// </summary>
        public string? ControlType => _entry.UIInfo?.ControlType;

        /// <summary>
        /// Control name for UI events
        /// </summary>
        public string? ControlName => _entry.UIInfo?.ControlName;

        /// <summary>
        /// HTTP method for API events
        /// </summary>
        public string? HttpMethod => _entry.ApiInfo?.Method;

        /// <summary>
        /// URL for API events
        /// </summary>
        public string? Url => _entry.ApiInfo?.Url;

        /// <summary>
        /// Status code for API responses
        /// </summary>
        public int? StatusCode => _entry.ApiInfo?.StatusCode;

        /// <summary>
        /// Request body for API events
        /// </summary>
        public string? RequestBody => _entry.ApiInfo?.RequestBody;

        /// <summary>
        /// Response body for API events
        /// </summary>
        public string? ResponseBody => _entry.ApiInfo?.ResponseBody;

        /// <summary>
        /// Detailed JSON representation
        /// </summary>
        public string Details
        {
            get
            {
                if (_entry.UIInfo != null)
                {
                    var ui = _entry.UIInfo;
                    return $"Control: {ui.ControlType}\n" +
                           $"Name: {ui.ControlName ?? "(none)"}\n" +
                           $"AutomationId: {ui.AutomationId ?? "(none)"}\n" +
                           $"Text: {ui.Text ?? "(none)"}\n" +
                           $"Window: {ui.WindowTitle ?? "(none)"}\n" +
                           $"Old Value: {ui.OldValue ?? "(none)"}\n" +
                           $"New Value: {ui.NewValue ?? "(none)"}";
                }

                if (_entry.ApiInfo != null)
                {
                    var api = _entry.ApiInfo;
                    return $"Method: {api.Method}\n" +
                           $"URL: {api.Url}\n" +
                           $"Status: {api.StatusCode}\n" +
                           $"Duration: {_entry.DurationMs}ms\n" +
                           $"Request Body:\n{api.RequestBody ?? "(none)"}\n\n" +
                           $"Response Body:\n{api.ResponseBody ?? "(none)"}";
                }

                return _entry.Metadata ?? "No details available";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
