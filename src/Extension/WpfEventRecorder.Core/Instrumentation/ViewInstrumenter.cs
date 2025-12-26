using System.Windows;
using System.Windows.Media;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Instrumentation.Instrumenters;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation;

/// <summary>
/// Instruments WPF views for event recording.
/// </summary>
public class ViewInstrumenter
{
    private readonly RecordingSession _session;
    private readonly RecordingConfiguration _configuration;
    private readonly List<IControlInstrumenter> _instrumenters;
    private readonly HashSet<int> _instrumentedElements = new();
    private readonly object _lock = new();

    public ViewInstrumenter(RecordingSession session, RecordingConfiguration configuration)
    {
        _session = session;
        _configuration = configuration;
        _instrumenters = InitializeInstrumenters();
    }

    private List<IControlInstrumenter> InitializeInstrumenters()
    {
        return new List<IControlInstrumenter>
        {
            new TextBoxInstrumenter(),
            new ComboBoxInstrumenter(),
            new ButtonInstrumenter(),
            new CheckBoxInstrumenter(),
            new DataGridInstrumenter(),
            new ListBoxInstrumenter(),
            new DatePickerInstrumenter(),
            new SliderInstrumenter(),
            new TabControlInstrumenter()
        };
    }

    /// <summary>
    /// Instruments a window and all its child elements.
    /// </summary>
    public void InstrumentWindow(Window window)
    {
        if (window == null) return;

        // Record window events
        if (_configuration.RecordWindowEvents)
        {
            window.Activated += (s, _) => RecordWindowEvent(window, WindowEventType.Activated);
            window.Deactivated += (s, _) => RecordWindowEvent(window, WindowEventType.Deactivated);
            window.Closed += (s, _) => RecordWindowEvent(window, WindowEventType.Closed);
            window.StateChanged += (s, _) => RecordWindowStateChange(window);

            RecordWindowEvent(window, WindowEventType.Opened);
        }

        // Instrument all child elements
        InstrumentElement(window);

        // Hook into loaded event for dynamically added content
        window.Loaded += (_, _) => InstrumentElement(window);
    }

    /// <summary>
    /// Instruments an element and its visual tree.
    /// </summary>
    public void InstrumentElement(DependencyObject element)
    {
        if (element == null) return;

        VisualTreeWalker.Walk(element, child =>
        {
            if (child is FrameworkElement fe)
            {
                InstrumentSingleElement(fe);
            }
            return true;
        });
    }

    private void InstrumentSingleElement(FrameworkElement element)
    {
        var hash = element.GetHashCode();

        lock (_lock)
        {
            if (_instrumentedElements.Contains(hash))
                return;

            // Check exclusions
            var elementType = element.GetType().Name;
            var elementName = element.Name;

            if (_configuration.ExcludedElementTypes.Contains(elementType))
                return;

            if (!string.IsNullOrEmpty(elementName) && _configuration.ExcludedElementNames.Contains(elementName))
                return;

            // Find and apply appropriate instrumenter
            foreach (var instrumenter in _instrumenters)
            {
                if (instrumenter.CanInstrument(element))
                {
                    instrumenter.Instrument(element, _session, _configuration);
                    _instrumentedElements.Add(hash);
                    break;
                }
            }
        }
    }

    private void RecordWindowEvent(Window window, WindowEventType eventType)
    {
        _session.AddEvent(new WindowEvent
        {
            WindowEventType = eventType,
            WindowTitle = window.Title,
            WindowType = window.GetType().FullName,
            X = window.Left,
            Y = window.Top,
            Width = window.Width,
            Height = window.Height,
            CurrentState = window.WindowState.ToString()
        });
    }

    private void RecordWindowStateChange(Window window)
    {
        var eventType = window.WindowState switch
        {
            WindowState.Minimized => WindowEventType.Minimized,
            WindowState.Maximized => WindowEventType.Maximized,
            WindowState.Normal => WindowEventType.Restored,
            _ => WindowEventType.Restored
        };

        RecordWindowEvent(window, eventType);
    }
}
