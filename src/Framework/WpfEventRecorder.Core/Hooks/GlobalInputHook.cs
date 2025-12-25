using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Forms;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Hooks
{
    /// <summary>
    /// Provides global input hooks for capturing mouse and keyboard events from any application
    /// </summary>
    public class GlobalInputHook : IDisposable
    {
        #region Win32 API

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        #endregion

        private IntPtr _mouseHookId = IntPtr.Zero;
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private LowLevelMouseProc _mouseProc;
        private LowLevelKeyboardProc _keyboardProc;
        private bool _disposed;

        private WindowInfo _targetWindow;
        private DateTime _lastClickTime = DateTime.MinValue;
        private POINT _lastClickPoint;
        private const int DoubleClickTimeMs = 500;
        private const int DoubleClickDistancePixels = 4;

        /// <summary>
        /// Event raised when a mouse click is detected
        /// </summary>
        public event EventHandler<MouseClickEventArgs> MouseClick;

        /// <summary>
        /// Event raised when a keyboard key is pressed
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyPress;

        /// <summary>
        /// Event raised when text input is detected (focus change with value change)
        /// </summary>
        public event EventHandler<TextInputEventArgs> TextInput;

        /// <summary>
        /// Starts the global input hooks
        /// </summary>
        /// <param name="targetWindow">Optional target window to filter events</param>
        public void Start(WindowInfo targetWindow = null)
        {
            _targetWindow = targetWindow;

            _mouseProc = MouseHookCallback;
            _keyboardProc = KeyboardHookCallback;

            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                if (curModule != null)
                {
                    var moduleHandle = GetModuleHandle(curModule.ModuleName);
                    _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, moduleHandle, 0);
                    _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, moduleHandle, 0);
                }
            }
        }

        /// <summary>
        /// Stops the global input hooks
        /// </summary>
        public void Stop()
        {
            if (_mouseHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookId);
                _mouseHookId = IntPtr.Zero;
            }

            if (_keyboardHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookId);
                _keyboardHookId = IntPtr.Zero;
            }

            _targetWindow = null;
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                var messageType = (int)wParam;

                // Only process left button events for now
                if (messageType == WM_LBUTTONDOWN || messageType == WM_LBUTTONUP ||
                    messageType == WM_LBUTTONDBLCLK)
                {
                    try
                    {
                        ProcessMouseEvent(messageType, hookStruct);
                    }
                    catch
                    {
                        // Silently ignore errors to avoid blocking input
                    }
                }
            }

            return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private void ProcessMouseEvent(int messageType, MSLLHOOKSTRUCT hookStruct)
        {
            // Only process button down events
            if (messageType != WM_LBUTTONDOWN)
                return;

            // Get the window under the cursor
            var hWnd = WindowFromPoint(hookStruct.pt);
            if (hWnd == IntPtr.Zero)
                return;

            // Check if this event is for our target window
            if (_targetWindow != null)
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                if (processId != (uint)_targetWindow.ProcessId)
                    return;
            }

            // Determine if this is a double-click
            var isDoubleClick = false;
            var now = DateTime.Now;
            if ((now - _lastClickTime).TotalMilliseconds < DoubleClickTimeMs)
            {
                var dx = Math.Abs(hookStruct.pt.X - _lastClickPoint.X);
                var dy = Math.Abs(hookStruct.pt.Y - _lastClickPoint.Y);
                if (dx <= DoubleClickDistancePixels && dy <= DoubleClickDistancePixels)
                {
                    isDoubleClick = true;
                }
            }
            _lastClickTime = now;
            _lastClickPoint = hookStruct.pt;

            // Get UI element info using UI Automation
            var elementInfo = GetElementInfo(hookStruct.pt);

            var args = new MouseClickEventArgs
            {
                X = hookStruct.pt.X,
                Y = hookStruct.pt.Y,
                IsDoubleClick = isDoubleClick,
                ControlType = elementInfo.ControlType,
                ControlName = elementInfo.ControlName,
                AutomationId = elementInfo.AutomationId,
                Text = elementInfo.Text,
                Value = elementInfo.Value,
                WindowTitle = elementInfo.WindowTitle,
                WindowHandle = hWnd,
                ClassName = elementInfo.ClassName,
                FrameworkId = elementInfo.FrameworkId,
                IsEnabled = elementInfo.IsEnabled,
                IsSelected = elementInfo.IsSelected,
                SelectionItemText = elementInfo.SelectionItemText,
                ToggleState = elementInfo.ToggleState
            };

            MouseClick?.Invoke(this, args);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var messageType = (int)wParam;

                if (messageType == WM_KEYDOWN || messageType == WM_SYSKEYDOWN)
                {
                    try
                    {
                        var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                        ProcessKeyboardEvent(hookStruct);
                    }
                    catch
                    {
                        // Silently ignore errors
                    }
                }
            }

            return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        private void ProcessKeyboardEvent(KBDLLHOOKSTRUCT hookStruct)
        {
            // Check if this event is for our target window
            if (_targetWindow != null)
            {
                var foregroundWindow = GetForegroundWindow();
                if (foregroundWindow != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(foregroundWindow, out uint processId);
                    if (processId != (uint)_targetWindow.ProcessId)
                        return;
                }
            }

            var key = (Keys)hookStruct.vkCode;
            var keyName = key.ToString();

            // Get modifier keys
            var modifiers = "";
            if ((Control.ModifierKeys & Keys.Control) != 0)
                modifiers += "Ctrl+";
            if ((Control.ModifierKeys & Keys.Alt) != 0)
                modifiers += "Alt+";
            if ((Control.ModifierKeys & Keys.Shift) != 0)
                modifiers += "Shift+";

            var keyCombination = modifiers + keyName;

            // Get focused element info
            var elementInfo = GetFocusedElementInfo();

            var args = new KeyboardEventArgs
            {
                VirtualKeyCode = (int)hookStruct.vkCode,
                KeyName = keyName,
                KeyCombination = keyCombination,
                ControlType = elementInfo.ControlType,
                ControlName = elementInfo.ControlName,
                AutomationId = elementInfo.AutomationId,
                WindowTitle = elementInfo.WindowTitle,
                ClassName = elementInfo.ClassName,
                FrameworkId = elementInfo.FrameworkId,
                IsEnabled = elementInfo.IsEnabled,
                Value = elementInfo.Value,
                ToggleState = elementInfo.ToggleState
            };

            KeyPress?.Invoke(this, args);
        }

        private ElementInfo GetElementInfo(POINT point)
        {
            var info = new ElementInfo();

            try
            {
                var element = AutomationElement.FromPoint(new System.Windows.Point(point.X, point.Y));
                if (element != null)
                {
                    info.ControlType = element.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");
                    info.ControlName = element.Current.Name;
                    info.AutomationId = element.Current.AutomationId;
                    info.ClassName = element.Current.ClassName;
                    info.FrameworkId = element.Current.FrameworkId;
                    info.IsEnabled = element.Current.IsEnabled;

                    // Try to get text value from ValuePattern
                    object valuePattern;
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                    {
                        info.Value = ((ValuePattern)valuePattern).Current.Value;
                        info.Text = info.Value;
                    }
                    else
                    {
                        info.Text = element.Current.Name;
                    }

                    // Try to get toggle state (for checkboxes, radio buttons, toggle buttons)
                    object togglePattern;
                    if (element.TryGetCurrentPattern(TogglePattern.Pattern, out togglePattern))
                    {
                        info.ToggleState = ((TogglePattern)togglePattern).Current.ToggleState.ToString();
                    }

                    // Try to get selection item info (for list items, combo box items)
                    object selectionItemPattern;
                    if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out selectionItemPattern))
                    {
                        var selItem = (SelectionItemPattern)selectionItemPattern;
                        info.IsSelected = selItem.Current.IsSelected;
                        info.SelectionItemText = element.Current.Name;
                    }

                    // For combo boxes, try to get the selected item text
                    if (element.Current.ControlType == ControlType.ComboBox)
                    {
                        object selectionPattern;
                        if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out selectionPattern))
                        {
                            var selection = ((SelectionPattern)selectionPattern).Current.GetSelection();
                            if (selection.Length > 0)
                            {
                                info.SelectionItemText = selection[0].Current.Name;
                            }
                        }
                    }

                    // Get window title
                    var window = GetParentWindow(element);
                    if (window != null)
                    {
                        info.WindowTitle = window.Current.Name;
                    }
                }
            }
            catch
            {
                info.ControlType = "Unknown";
            }

            return info;
        }

        private ElementInfo GetFocusedElementInfo()
        {
            var info = new ElementInfo();

            try
            {
                var element = AutomationElement.FocusedElement;
                if (element != null)
                {
                    info.ControlType = element.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");
                    info.ControlName = element.Current.Name;
                    info.AutomationId = element.Current.AutomationId;
                    info.ClassName = element.Current.ClassName;
                    info.FrameworkId = element.Current.FrameworkId;
                    info.IsEnabled = element.Current.IsEnabled;

                    // Try to get text value from ValuePattern
                    object valuePattern;
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                    {
                        info.Value = ((ValuePattern)valuePattern).Current.Value;
                        info.Text = info.Value;
                    }
                    else
                    {
                        info.Text = element.Current.Name;
                    }

                    // Try to get toggle state
                    object togglePattern;
                    if (element.TryGetCurrentPattern(TogglePattern.Pattern, out togglePattern))
                    {
                        info.ToggleState = ((TogglePattern)togglePattern).Current.ToggleState.ToString();
                    }

                    // Get window title
                    var window = GetParentWindow(element);
                    if (window != null)
                    {
                        info.WindowTitle = window.Current.Name;
                    }
                }
            }
            catch
            {
                info.ControlType = "Unknown";
            }

            return info;
        }

        private AutomationElement GetParentWindow(AutomationElement element)
        {
            try
            {
                var walker = TreeWalker.ControlViewWalker;
                var current = element;

                while (current != null)
                {
                    if (current.Current.ControlType == ControlType.Window)
                    {
                        return current;
                    }
                    current = walker.GetParent(current);
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
        }

        private class ElementInfo
        {
            public string ControlType { get; set; } = "Unknown";
            public string ControlName { get; set; }
            public string AutomationId { get; set; }
            public string Text { get; set; }
            public string Value { get; set; }
            public string WindowTitle { get; set; }
            public string ClassName { get; set; }
            public string FrameworkId { get; set; }
            public bool IsEnabled { get; set; } = true;
            public bool IsSelected { get; set; }
            public string SelectionItemText { get; set; }
            public string ToggleState { get; set; }
        }
    }

    /// <summary>
    /// Event arguments for mouse click events
    /// </summary>
    public class MouseClickEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsDoubleClick { get; set; }
        public string ControlType { get; set; } = "Unknown";
        public string ControlName { get; set; }
        public string AutomationId { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string WindowTitle { get; set; }
        public IntPtr WindowHandle { get; set; }
        public string ClassName { get; set; }
        public string FrameworkId { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsSelected { get; set; }
        public string SelectionItemText { get; set; }
        public string ToggleState { get; set; }
    }

    /// <summary>
    /// Event arguments for keyboard events
    /// </summary>
    public class KeyboardEventArgs : EventArgs
    {
        public int VirtualKeyCode { get; set; }
        public string KeyName { get; set; } = "";
        public string KeyCombination { get; set; } = "";
        public string ControlType { get; set; } = "Unknown";
        public string ControlName { get; set; }
        public string AutomationId { get; set; }
        public string WindowTitle { get; set; }
        public string ClassName { get; set; }
        public string FrameworkId { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Value { get; set; }
        public string ToggleState { get; set; }
    }

    /// <summary>
    /// Event arguments for text input events
    /// </summary>
    public class TextInputEventArgs : EventArgs
    {
        public string ControlType { get; set; } = "Unknown";
        public string ControlName { get; set; }
        public string AutomationId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string WindowTitle { get; set; }
    }
}
