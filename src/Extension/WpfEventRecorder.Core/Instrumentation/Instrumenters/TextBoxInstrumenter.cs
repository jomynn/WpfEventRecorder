using System.Windows;
using System.Windows.Controls;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for TextBox controls.
/// </summary>
public class TextBoxInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(TextBox);

    private readonly Dictionary<int, TextChangedEventHandler> _handlers = new();

    public bool CanInstrument(DependencyObject element) => element is TextBox;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not TextBox textBox) return;

        var previousValue = textBox.Text;
        var hash = textBox.GetHashCode();

        TextChangedEventHandler handler = (sender, e) =>
        {
            if (sender is not TextBox tb) return;

            var currentValue = tb.Text;

            // Apply debounce - defer recording until typing stops
            var bindingPath = BindingHelper.GetBindingPath(tb, TextBox.TextProperty);
            var isSensitive = IsSensitiveField(tb.Name, bindingPath, configuration);

            session.AddEvent(new InputEvent
            {
                InputType = InputEventType.TextChanged,
                SourceElementName = tb.Name,
                SourceElementType = nameof(TextBox),
                AutomationId = GetAutomationId(tb),
                OldValue = isSensitive ? "***" : previousValue,
                NewValue = isSensitive ? "***" : currentValue,
                BindingPath = bindingPath,
                ViewModelProperty = BindingHelper.GetViewModelPropertyName(bindingPath)
            });

            previousValue = currentValue;
        };

        textBox.TextChanged += handler;
        _handlers[hash] = handler;
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not TextBox textBox) return;

        var hash = textBox.GetHashCode();
        if (_handlers.TryGetValue(hash, out var handler))
        {
            textBox.TextChanged -= handler;
            _handlers.Remove(hash);
        }
    }

    private static string? GetAutomationId(TextBox textBox)
    {
        return System.Windows.Automation.AutomationProperties.GetAutomationId(textBox);
    }

    private static bool IsSensitiveField(string? name, string? bindingPath, RecordingConfiguration config)
    {
        var fieldsToCheck = new[] { name?.ToLowerInvariant(), bindingPath?.ToLowerInvariant() };
        return config.SensitiveFields.Any(sensitive =>
            fieldsToCheck.Any(field => field?.Contains(sensitive) == true));
    }
}
