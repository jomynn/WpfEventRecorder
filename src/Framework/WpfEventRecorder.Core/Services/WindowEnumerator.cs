using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Services
{
    /// <summary>
    /// Service for enumerating running windows
    /// </summary>
    public static class WindowEnumerator
    {
        #region Native Methods

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const uint GW_OWNER = 4;
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        #endregion

        /// <summary>
        /// Checks if the current platform supports window enumeration (always true for .NET Framework on Windows)
        /// </summary>
        public static bool IsSupported => true;

        /// <summary>
        /// Gets all visible top-level windows
        /// </summary>
        public static List<WindowInfo> GetVisibleWindows()
        {
            var windows = new List<WindowInfo>();
            var processCache = new Dictionary<uint, Process>();

            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    try
                    {
                        // Skip invisible windows (but include minimized windows)
                        if (!IsWindowVisible(hWnd) && !IsIconic(hWnd))
                            return true;

                        // Skip windows with no title
                        var titleLength = GetWindowTextLength(hWnd);
                        if (titleLength == 0)
                            return true;

                        // Skip owned windows (popup, child windows)
                        if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero)
                            return true;

                        // Get window title
                        var titleBuilder = new StringBuilder(titleLength + 1);
                        GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
                        var title = titleBuilder.ToString();

                        // Get process ID
                        GetWindowThreadProcessId(hWnd, out uint processId);

                        // Get process info
                        Process process;
                        if (!processCache.TryGetValue(processId, out process))
                        {
                            try
                            {
                                process = Process.GetProcessById((int)processId);
                                processCache[processId] = process;
                            }
                            catch
                            {
                                return true;
                            }
                        }

                        // Check if it's a WPF app by looking for WPF class names
                        var isWpf = IsWpfWindow(hWnd);

                        // Check if minimized
                        var isMinimized = IsIconic(hWnd);

                        var windowInfo = new WindowInfo
                        {
                            ProcessId = (int)processId,
                            ProcessName = process.ProcessName,
                            WindowTitle = isMinimized ? $"{title} (Minimized)" : title,
                            WindowHandle = hWnd,
                            IsWpfApp = isWpf
                        };

                        try
                        {
                            windowInfo.ExecutablePath = process.MainModule?.FileName;
                        }
                        catch
                        {
                            // Access denied for some processes
                        }

                        windows.Add(windowInfo);
                    }
                    catch
                    {
                        // Skip windows that cause errors
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch
            {
                // Handle platform-specific errors
            }

            // Dispose cached processes
            foreach (var process in processCache.Values)
            {
                process.Dispose();
            }

            return windows;
        }

        /// <summary>
        /// Gets only WPF application windows
        /// </summary>
        public static List<WindowInfo> GetWpfWindows()
        {
            var allWindows = GetVisibleWindows();
            return allWindows.FindAll(w => w.IsWpfApp);
        }

        /// <summary>
        /// Checks if a window is a WPF window
        /// </summary>
        private static bool IsWpfWindow(IntPtr hWnd)
        {
            var classNameBuilder = new StringBuilder(256);
            GetClassName(hWnd, classNameBuilder, classNameBuilder.Capacity);
            var className = classNameBuilder.ToString();

            // WPF windows typically have "HwndWrapper" in their class name
            // or use "Window" class from WPF
            return className.Contains("HwndWrapper") ||
                   className.StartsWith("WPF", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Refreshes the window list
        /// </summary>
        public static List<WindowInfo> Refresh(bool wpfOnly = false)
        {
            return wpfOnly ? GetWpfWindows() : GetVisibleWindows();
        }

        /// <summary>
        /// Brings the specified window to the foreground
        /// </summary>
        /// <param name="windowInfo">The window to bring to front</param>
        /// <returns>True if successful</returns>
        public static bool BringToFront(WindowInfo windowInfo)
        {
            if (windowInfo == null || windowInfo.WindowHandle == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                // If window is minimized, restore it first
                if (IsIconic(windowInfo.WindowHandle))
                {
                    ShowWindow(windowInfo.WindowHandle, SW_RESTORE);
                }
                else
                {
                    ShowWindow(windowInfo.WindowHandle, SW_SHOW);
                }

                // Bring to foreground
                return SetForegroundWindow(windowInfo.WindowHandle);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the currently active foreground window
        /// </summary>
        /// <returns>The window info of the foreground window, or null if not found</returns>
        public static WindowInfo GetForegroundWindowInfo()
        {
            try
            {
                var hWnd = GetForegroundWindow();
                if (hWnd == IntPtr.Zero)
                {
                    return null;
                }

                var windows = GetVisibleWindows();
                return windows.Find(w => w.WindowHandle == hWnd);
            }
            catch
            {
                return null;
            }
        }
    }
}
