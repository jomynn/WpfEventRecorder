using System.Windows;
using System.Windows.Controls;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for CheckBox and RadioButton controls.
/// </summary>
public class CheckBoxInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(CheckBox);

    private readonly Dictionary<int, (RoutedEventHandler Checked, RoutedEventHandler Unchecked)> _handlers = new();

    public bool CanInstrument(DependencyObject element) =>
        element is CheckBox or RadioButton;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not System.Windows.Controls.Primitives.ToggleButton toggleButton) return;

        var hash = toggleButton.GetHashCode();

        RoutedEventHandler checkedHandler = (sender, e) =>
            RecordCheckChange(sender as System.Windows.Controls.Primitives.ToggleButton, true, session);

        RoutedEventHandler uncheckedHandler = (sender, e) =>
            RecordCheckChange(sender as System.Windows.Controls.Primitives.ToggleButton, false, session);

        toggleButton.Checked += checkedHandler;
        toggleButton.Unchecked += uncheckedHandler;

        _handlers[hash] = (checkedHandler, uncheckedHandler);
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not System.Windows.Controls.Primitives.ToggleButton toggleButton) return;

        var hash = toggleButton.GetHashCode();
        if (_handlers.TryGetValue(hash, out var handlers))
        {
            toggleButton.Checked -= handlers.Checked;
            toggleButton.Unchecked -= handlers.Unchecked;
            _handlers.Remove(hash);
        }
    }

    private static void RecordCheckChange(System.Windows.Controls.Primitives.ToggleButton? toggle, bool isChecked, RecordingSession session)
    {
        if (toggle == null) return;

        var controlType = toggle is RadioButton ? nameof(RadioButton) : nameof(CheckBox);
        var label = GetToggleLabel(toggle);

        session.AddEvent(new InputEvent
        {
            InputType = InputEventType.CheckedChanged,
            SourceElementName = toggle.Name,
            SourceElementType = controlType,
            AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(toggle),
            OldValue = !isChecked,
            NewValue = isChecked,
            BindingPath = BindingHelper.GetBindingPath(toggle, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty),
            ViewModelProperty = BindingHelper.GetViewModelPropertyName(
                BindingHelper.GetBindingPath(toggle, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty)),
            ControlInfo = new Dictionary<string, object?>
            {
                ["Label"] = label,
                ["IsThreeState"] = toggle.IsThreeState
            }
        });
    }

    private static string? GetToggleLabel(System.Windows.Controls.Primitives.ToggleButton toggle)
    {
        if (toggle.Content is string text)
            return text;

        if (toggle.Content is TextBlock textBlock)
            return textBlock.Text;

        return toggle.Name;
    }
}
