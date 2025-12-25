using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Hooks
{
    /// <summary>
    /// Hooks into WPF UI events and captures interactions
    /// </summary>
    public class UIHook : IDisposable
    {
        private readonly Subject<RecordEntry> _eventSubject = new Subject<RecordEntry>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private GlobalInputHook? _globalInputHook;
        private bool _isActive;
        private bool _disposed;
        private WindowInfo? _targetWindow;

        /// <summary>
        /// Observable stream of UI events
        /// </summary>
        public IObservable<RecordEntry> Events => _eventSubject.AsObservable();

        /// <summary>
        /// Whether the hook is currently active
        /// </summary>
        public bool IsActive => _isActive;

        /// <summary>
        /// The target window being monitored
        /// </summary>
        public WindowInfo? TargetWindow => _targetWindow;

        /// <summary>
        /// Starts capturing UI events
        /// </summary>
        public void Start()
        {
            if (_isActive) return;
            _isActive = true;
            AttachHooks();
        }

        /// <summary>
        /// Starts capturing UI events for a specific target window
        /// </summary>
        /// <param name="targetWindow">The window to monitor</param>
        public void Start(WindowInfo targetWindow)
        {
            if (_isActive) return;
            _targetWindow = targetWindow;
            _isActive = true;
            AttachHooks();
        }

        /// <summary>
        /// Stops capturing UI events
        /// </summary>
        public void Stop()
        {
            if (!_isActive) return;
            _isActive = false;
            _targetWindow = null;
            DetachHooks();
        }

        /// <summary>
        /// Records a custom UI event
        /// </summary>
        public void RecordEvent(RecordEntry entry)
        {
            if (!_isActive) return;
            _eventSubject.OnNext(entry);
        }

        /// <summary>
        /// Records a click event
        /// </summary>
        public void RecordClick(string controlType, string? controlName, string? automationId,
                                 string? text, string? windowTitle, ScreenPoint? position = null)
        {
            if (!_isActive) return;

            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UIClick,
                UIInfo = new UIInfo
                {
                    ControlType = controlType,
                    ControlName = controlName,
                    AutomationId = automationId,
                    Text = text,
                    WindowTitle = windowTitle,
                    ScreenPosition = position
                }
            };

            _eventSubject.OnNext(entry);
        }

        /// <summary>
        /// Records a text input event
        /// </summary>
        public void RecordTextInput(string controlType, string? controlName, string? automationId,
                                     string? oldValue, string? newValue, string? windowTitle)
        {
            if (!_isActive) return;

            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UITextInput,
                UIInfo = new UIInfo
                {
                    ControlType = controlType,
                    ControlName = controlName,
                    AutomationId = automationId,
                    OldValue = oldValue,
                    NewValue = newValue,
                    WindowTitle = windowTitle
                }
            };

            _eventSubject.OnNext(entry);
        }

        /// <summary>
        /// Records a selection change event
        /// </summary>
        public void RecordSelectionChange(string controlType, string? controlName, string? automationId,
                                           string? oldValue, string? newValue, string? windowTitle)
        {
            if (!_isActive) return;

            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UISelectionChange,
                UIInfo = new UIInfo
                {
                    ControlType = controlType,
                    ControlName = controlName,
                    AutomationId = automationId,
                    OldValue = oldValue,
                    NewValue = newValue,
                    WindowTitle = windowTitle
                }
            };

            _eventSubject.OnNext(entry);
        }

        /// <summary>
        /// Records a toggle (checkbox/radio) event
        /// </summary>
        public void RecordToggle(string controlType, string? controlName, string? automationId,
                                  bool oldValue, bool newValue, string? windowTitle)
        {
            if (!_isActive) return;

            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UIToggle,
                UIInfo = new UIInfo
                {
                    ControlType = controlType,
                    ControlName = controlName,
                    AutomationId = automationId,
                    OldValue = oldValue.ToString(),
                    NewValue = newValue.ToString(),
                    WindowTitle = windowTitle
                }
            };

            _eventSubject.OnNext(entry);
        }

        /// <summary>
        /// Records a keyboard shortcut event
        /// </summary>
        public void RecordKeyboardShortcut(string keyCombination, string? windowTitle)
        {
            if (!_isActive) return;

            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UIKeyboardShortcut,
                UIInfo = new UIInfo
                {
                    ControlType = "Keyboard",
                    KeyCombination = keyCombination,
                    WindowTitle = windowTitle
                }
            };

            _eventSubject.OnNext(entry);
        }

        /// <summary>
        /// Records a window open event
        /// </summary>
        public void RecordWindowOpen(string windowType, string? windowTitle)
        {
            if (!_isActive) return;

            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UIWindowOpen,
                UIInfo = new UIInfo
                {
                    ControlType = "Window",
                    WindowType = windowType,
                    WindowTitle = windowTitle
                }
            };

            _eventSubject.OnNext(entry);
        }

        /// <summary>
        /// Records a window close event
        /// </summary>
        public void RecordWindowClose(string windowType, string? windowTitle)
        {
            if (!_isActive) return;

            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UIWindowClose,
                UIInfo = new UIInfo
                {
                    ControlType = "Window",
                    WindowType = windowType,
                    WindowTitle = windowTitle
                }
            };

            _eventSubject.OnNext(entry);
        }

        private void AttachHooks()
        {
            // Create and start the global input hook
            _globalInputHook = new GlobalInputHook();

            // Subscribe to mouse click events
            _globalInputHook.MouseClick += OnMouseClick;

            // Subscribe to keyboard events
            _globalInputHook.KeyPress += OnKeyPress;

            // Start the hooks
            _globalInputHook.Start(_targetWindow);
        }

        private void OnMouseClick(object? sender, MouseClickEventArgs e)
        {
            if (!_isActive) return;

            var entryType = e.IsDoubleClick ? RecordEntryType.UIDoubleClick : RecordEntryType.UIClick;

            var properties = new Dictionary<string, string>();

            // Basic properties
            if (!string.IsNullOrEmpty(e.ClassName))
                properties["ClassName"] = e.ClassName;
            if (!string.IsNullOrEmpty(e.FrameworkId))
                properties["FrameworkId"] = e.FrameworkId;
            if (!string.IsNullOrEmpty(e.ToggleState))
                properties["ToggleState"] = e.ToggleState;
            if (!string.IsNullOrEmpty(e.SelectionItemText))
                properties["SelectedItem"] = e.SelectionItemText;
            if (e.IsSelected)
                properties["IsSelected"] = "true";
            if (!e.IsEnabled)
                properties["IsEnabled"] = "false";

            // Additional properties
            if (!string.IsNullOrEmpty(e.LocalizedControlType))
                properties["LocalizedControlType"] = e.LocalizedControlType;
            if (!string.IsNullOrEmpty(e.HelpText))
                properties["HelpText"] = e.HelpText;
            if (!string.IsNullOrEmpty(e.AcceleratorKey))
                properties["AcceleratorKey"] = e.AcceleratorKey;
            if (!string.IsNullOrEmpty(e.AccessKey))
                properties["AccessKey"] = e.AccessKey;
            if (e.IsPassword)
                properties["IsPassword"] = "true";
            if (e.IsReadOnly)
                properties["IsReadOnly"] = "true";
            if (e.IsRequired)
                properties["IsRequired"] = "true";
            if (e.IsKeyboardFocusable)
                properties["IsKeyboardFocusable"] = "true";
            if (e.HasKeyboardFocus)
                properties["HasKeyboardFocus"] = "true";
            if (!string.IsNullOrEmpty(e.ItemType))
                properties["ItemType"] = e.ItemType;
            if (!string.IsNullOrEmpty(e.ItemStatus))
                properties["ItemStatus"] = e.ItemStatus;
            if (e.ProcessId.HasValue)
                properties["ProcessId"] = e.ProcessId.Value.ToString();

            // Bounding rectangle
            if (e.BoundingRectangle.HasValue)
            {
                var rect = e.BoundingRectangle.Value;
                properties["BoundingRect"] = $"{rect.X},{rect.Y},{rect.Width},{rect.Height}";
            }

            // Range value properties (for sliders, progress bars)
            if (e.RangeValue.HasValue)
            {
                properties["RangeValue"] = e.RangeValue.Value.ToString("F2");
                if (e.RangeMinimum.HasValue)
                    properties["RangeMinimum"] = e.RangeMinimum.Value.ToString("F2");
                if (e.RangeMaximum.HasValue)
                    properties["RangeMaximum"] = e.RangeMaximum.Value.ToString("F2");
            }

            // Grid properties (for data grids, tables)
            if (e.RowCount.HasValue)
                properties["RowCount"] = e.RowCount.Value.ToString();
            if (e.ColumnCount.HasValue)
                properties["ColumnCount"] = e.ColumnCount.Value.ToString();
            if (e.RowIndex.HasValue)
                properties["RowIndex"] = e.RowIndex.Value.ToString();
            if (e.ColumnIndex.HasValue)
                properties["ColumnIndex"] = e.ColumnIndex.Value.ToString();

            var entry = new RecordEntry
            {
                EntryType = entryType,
                UIInfo = new UIInfo
                {
                    ControlType = e.ControlType,
                    ControlName = e.ControlName,
                    AutomationId = e.AutomationId,
                    Text = e.Text,
                    NewValue = e.Value,
                    WindowTitle = e.WindowTitle,
                    ScreenPosition = new ScreenPoint { X = e.X, Y = e.Y },
                    VisualTreePath = e.VisualTreePath,
                    ContentText = e.ContentText,
                    Properties = properties.Count > 0 ? properties : null
                }
            };

            _eventSubject.OnNext(entry);
        }

        private void OnKeyPress(object? sender, KeyboardEventArgs e)
        {
            if (!_isActive) return;

            var properties = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(e.ClassName))
                properties["ClassName"] = e.ClassName;
            if (!string.IsNullOrEmpty(e.FrameworkId))
                properties["FrameworkId"] = e.FrameworkId;
            if (!string.IsNullOrEmpty(e.ToggleState))
                properties["ToggleState"] = e.ToggleState;
            if (!e.IsEnabled)
                properties["IsEnabled"] = "false";

            // Record keyboard events
            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UIKeyboardShortcut,
                UIInfo = new UIInfo
                {
                    ControlType = e.ControlType,
                    ControlName = e.ControlName,
                    AutomationId = e.AutomationId,
                    KeyCombination = e.KeyCombination,
                    WindowTitle = e.WindowTitle,
                    NewValue = e.Value,
                    Properties = properties.Count > 0 ? properties : null
                }
            };

            _eventSubject.OnNext(entry);
        }

        private void DetachHooks()
        {
            if (_globalInputHook != null)
            {
                _globalInputHook.MouseClick -= OnMouseClick;
                _globalInputHook.KeyPress -= OnKeyPress;
                _globalInputHook.Stop();
                _globalInputHook.Dispose();
                _globalInputHook = null;
            }

            foreach (var sub in _subscriptions)
            {
                sub.Dispose();
            }
            _subscriptions.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Stop();
            _eventSubject.Dispose();
        }
    }
}
