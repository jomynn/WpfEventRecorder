using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for Button controls.
/// </summary>
public class ButtonInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(ButtonBase);

    private readonly Dictionary<int, RoutedEventHandler> _handlers = new();

    public bool CanInstrument(DependencyObject element) => element is ButtonBase;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not ButtonBase button) return;

        var hash = button.GetHashCode();

        RoutedEventHandler handler = (sender, e) =>
        {
            if (sender is not ButtonBase btn) return;

            var buttonText = GetButtonText(btn);
            var commandName = GetCommandName(btn);

            session.AddEvent(new InputEvent
            {
                InputType = InputEventType.ButtonClicked,
                SourceElementName = btn.Name,
                SourceElementType = btn.GetType().Name,
                AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(btn),
                NewValue = buttonText,
                BindingPath = BindingHelper.GetBindingPath(btn, ButtonBase.CommandProperty),
                ControlInfo = new Dictionary<string, object?>
                {
                    ["ButtonText"] = buttonText,
                    ["CommandName"] = commandName,
                    ["CommandParameter"] = btn.CommandParameter
                }
            });
        };

        button.Click += handler;
        _handlers[hash] = handler;
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not ButtonBase button) return;

        var hash = button.GetHashCode();
        if (_handlers.TryGetValue(hash, out var handler))
        {
            button.Click -= handler;
            _handlers.Remove(hash);
        }
    }

    private static string? GetButtonText(ButtonBase button)
    {
        if (button is Button btn && btn.Content is string text)
            return text;

        if (button is Button btn2 && btn2.Content is TextBlock textBlock)
            return textBlock.Text;

        return button.Name;
    }

    private static string? GetCommandName(ButtonBase button)
    {
        var command = button.Command;
        if (command == null) return null;

        var type = command.GetType();
        return type.Name;
    }
}
