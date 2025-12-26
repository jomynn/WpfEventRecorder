using System.Windows;
using System.Windows.Controls;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for ComboBox controls.
/// </summary>
public class ComboBoxInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(ComboBox);

    private readonly Dictionary<int, SelectionChangedEventHandler> _handlers = new();

    public bool CanInstrument(DependencyObject element) => element is ComboBox;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not ComboBox comboBox) return;

        var hash = comboBox.GetHashCode();
        var previousValue = GetDisplayValue(comboBox.SelectedItem);

        SelectionChangedEventHandler handler = (sender, e) =>
        {
            if (sender is not ComboBox cb) return;

            var newValue = GetDisplayValue(cb.SelectedItem);

            session.AddEvent(new InputEvent
            {
                InputType = InputEventType.SelectionChanged,
                SourceElementName = cb.Name,
                SourceElementType = nameof(ComboBox),
                AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(cb),
                OldValue = previousValue,
                NewValue = newValue,
                BindingPath = BindingHelper.GetBindingPath(cb, ComboBox.SelectedItemProperty),
                ViewModelProperty = BindingHelper.GetViewModelPropertyName(
                    BindingHelper.GetBindingPath(cb, ComboBox.SelectedItemProperty)),
                ControlInfo = new Dictionary<string, object?>
                {
                    ["SelectedIndex"] = cb.SelectedIndex,
                    ["DisplayMemberPath"] = cb.DisplayMemberPath
                }
            });

            previousValue = newValue;
        };

        comboBox.SelectionChanged += handler;
        _handlers[hash] = handler;
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not ComboBox comboBox) return;

        var hash = comboBox.GetHashCode();
        if (_handlers.TryGetValue(hash, out var handler))
        {
            comboBox.SelectionChanged -= handler;
            _handlers.Remove(hash);
        }
    }

    private static string? GetDisplayValue(object? item)
    {
        if (item == null) return null;

        // Try common display properties
        var type = item.GetType();
        var displayProps = new[] { "Name", "DisplayName", "Title", "Text", "Value" };

        foreach (var propName in displayProps)
        {
            var prop = type.GetProperty(propName);
            if (prop != null)
            {
                return prop.GetValue(item)?.ToString();
            }
        }

        return item.ToString();
    }
}
