using System.Windows;
using System.Windows.Controls;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for TabControl controls.
/// </summary>
public class TabControlInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(TabControl);

    private readonly Dictionary<int, SelectionChangedEventHandler> _handlers = new();

    public bool CanInstrument(DependencyObject element) => element is TabControl;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not TabControl tabControl) return;

        var hash = tabControl.GetHashCode();
        var previousIndex = tabControl.SelectedIndex;
        var previousHeader = GetTabHeader(tabControl.SelectedItem as TabItem);

        SelectionChangedEventHandler handler = (sender, e) =>
        {
            if (sender is not TabControl tc) return;

            // Only handle tab control's own selection, not nested selectors
            if (e.OriginalSource != tc) return;

            var newIndex = tc.SelectedIndex;
            var newHeader = GetTabHeader(tc.SelectedItem as TabItem);

            session.AddEvent(new NavigationEvent
            {
                NavigationType = NavigationType.TabChanged,
                SourceElementName = tc.Name,
                SourceElementType = nameof(TabControl),
                AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(tc),
                FromView = previousHeader,
                ToView = newHeader,
                TabIndex = newIndex,
                TabHeader = newHeader,
                Metadata = new Dictionary<string, object?>
                {
                    ["PreviousIndex"] = previousIndex,
                    ["TotalTabs"] = tc.Items.Count
                }
            });

            previousIndex = newIndex;
            previousHeader = newHeader;
        };

        tabControl.SelectionChanged += handler;
        _handlers[hash] = handler;
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not TabControl tabControl) return;

        var hash = tabControl.GetHashCode();
        if (_handlers.TryGetValue(hash, out var handler))
        {
            tabControl.SelectionChanged -= handler;
            _handlers.Remove(hash);
        }
    }

    private static string? GetTabHeader(TabItem? tabItem)
    {
        if (tabItem == null) return null;

        if (tabItem.Header is string header)
            return header;

        if (tabItem.Header is TextBlock textBlock)
            return textBlock.Text;

        return tabItem.Name;
    }
}
