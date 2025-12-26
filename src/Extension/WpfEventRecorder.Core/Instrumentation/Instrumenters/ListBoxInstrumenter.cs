using System.Windows;
using System.Windows.Controls;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for ListBox controls.
/// </summary>
public class ListBoxInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(ListBox);

    private readonly Dictionary<int, SelectionChangedEventHandler> _handlers = new();

    public bool CanInstrument(DependencyObject element) => element is ListBox && element is not ListView;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not ListBox listBox) return;

        var hash = listBox.GetHashCode();

        SelectionChangedEventHandler handler = (sender, e) =>
        {
            if (sender is not ListBox lb) return;

            var selectedItem = lb.SelectedItem;
            var displayValue = GetDisplayValue(selectedItem, lb.DisplayMemberPath);

            session.AddEvent(new InputEvent
            {
                InputType = InputEventType.SelectionChanged,
                SourceElementName = lb.Name,
                SourceElementType = nameof(ListBox),
                AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(lb),
                NewValue = displayValue,
                BindingPath = BindingHelper.GetBindingPath(lb, ListBox.SelectedItemProperty),
                ViewModelProperty = BindingHelper.GetViewModelPropertyName(
                    BindingHelper.GetBindingPath(lb, ListBox.SelectedItemProperty)),
                ControlInfo = new Dictionary<string, object?>
                {
                    ["SelectedIndex"] = lb.SelectedIndex,
                    ["SelectionMode"] = lb.SelectionMode.ToString(),
                    ["SelectedItemsCount"] = lb.SelectedItems.Count
                }
            });
        };

        listBox.SelectionChanged += handler;
        _handlers[hash] = handler;
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not ListBox listBox) return;

        var hash = listBox.GetHashCode();
        if (_handlers.TryGetValue(hash, out var handler))
        {
            listBox.SelectionChanged -= handler;
            _handlers.Remove(hash);
        }
    }

    private static string? GetDisplayValue(object? item, string? displayMemberPath)
    {
        if (item == null) return null;

        if (!string.IsNullOrEmpty(displayMemberPath))
        {
            var prop = item.GetType().GetProperty(displayMemberPath);
            if (prop != null)
            {
                return prop.GetValue(item)?.ToString();
            }
        }

        return item.ToString();
    }
}
