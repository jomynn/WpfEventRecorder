using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Services;

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

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private const uint GW_OWNER = 4;

    #endregion

    /// <summary>
    /// Gets all visible top-level windows
    /// </summary>
    public static List<WindowInfo> GetVisibleWindows()
    {
        var windows = new List<WindowInfo>();
        var processCache = new Dictionary<uint, Process>();

        EnumWindows((hWnd, lParam) =>
        {
            // Skip invisible windows
            if (!IsWindowVisible(hWnd))
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
            if (!processCache.TryGetValue(processId, out var process))
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

            var windowInfo = new WindowInfo
            {
                ProcessId = (int)processId,
                ProcessName = process.ProcessName,
                WindowTitle = title,
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
            return true;
        }, IntPtr.Zero);

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
}
