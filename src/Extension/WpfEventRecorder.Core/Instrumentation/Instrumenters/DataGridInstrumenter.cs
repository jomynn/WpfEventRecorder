using System.Windows;
using System.Windows.Controls;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for DataGrid controls.
/// </summary>
public class DataGridInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(DataGrid);

    private readonly Dictionary<int, SelectionChangedEventHandler> _handlers = new();

    public bool CanInstrument(DependencyObject element) => element is DataGrid;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not DataGrid dataGrid) return;

        var hash = dataGrid.GetHashCode();

        SelectionChangedEventHandler handler = (sender, e) =>
        {
            if (sender is not DataGrid dg) return;

            var selectedItem = dg.SelectedItem;
            var selectedIndex = dg.SelectedIndex;

            session.AddEvent(new InputEvent
            {
                InputType = InputEventType.SelectionChanged,
                SourceElementName = dg.Name,
                SourceElementType = nameof(DataGrid),
                AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(dg),
                NewValue = GetItemDescription(selectedItem),
                BindingPath = BindingHelper.GetBindingPath(dg, DataGrid.SelectedItemProperty),
                ViewModelProperty = BindingHelper.GetViewModelPropertyName(
                    BindingHelper.GetBindingPath(dg, DataGrid.SelectedItemProperty)),
                ControlInfo = new Dictionary<string, object?>
                {
                    ["SelectedIndex"] = selectedIndex,
                    ["ItemType"] = selectedItem?.GetType().Name,
                    ["AddedItems"] = e.AddedItems.Count,
                    ["RemovedItems"] = e.RemovedItems.Count
                }
            });
        };

        dataGrid.SelectionChanged += handler;
        _handlers[hash] = handler;
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not DataGrid dataGrid) return;

        var hash = dataGrid.GetHashCode();
        if (_handlers.TryGetValue(hash, out var handler))
        {
            dataGrid.SelectionChanged -= handler;
            _handlers.Remove(hash);
        }
    }

    private static string? GetItemDescription(object? item)
    {
        if (item == null) return null;

        var type = item.GetType();

        // Try common identifier properties
        var idProps = new[] { "Id", "ID", "Name", "Title", "Key" };
        foreach (var propName in idProps)
        {
            var prop = type.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(item);
                return $"{type.Name} ({propName}={value})";
            }
        }

        return item.ToString();
    }
}
