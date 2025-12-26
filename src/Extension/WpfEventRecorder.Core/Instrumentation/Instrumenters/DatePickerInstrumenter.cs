using System.Windows;
using System.Windows.Controls;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for DatePicker controls.
/// </summary>
public class DatePickerInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(DatePicker);

    private readonly Dictionary<int, EventHandler<SelectionChangedEventArgs>> _handlers = new();

    public bool CanInstrument(DependencyObject element) => element is DatePicker;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not DatePicker datePicker) return;

        var hash = datePicker.GetHashCode();
        var previousValue = datePicker.SelectedDate;

        EventHandler<SelectionChangedEventArgs> handler = (sender, e) =>
        {
            if (sender is not DatePicker dp) return;

            var newValue = dp.SelectedDate;

            session.AddEvent(new InputEvent
            {
                InputType = InputEventType.DateChanged,
                SourceElementName = dp.Name,
                SourceElementType = nameof(DatePicker),
                AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(dp),
                OldValue = previousValue?.ToString("yyyy-MM-dd"),
                NewValue = newValue?.ToString("yyyy-MM-dd"),
                BindingPath = BindingHelper.GetBindingPath(dp, DatePicker.SelectedDateProperty),
                ViewModelProperty = BindingHelper.GetViewModelPropertyName(
                    BindingHelper.GetBindingPath(dp, DatePicker.SelectedDateProperty)),
                ControlInfo = new Dictionary<string, object?>
                {
                    ["DisplayDateStart"] = dp.DisplayDateStart?.ToString("yyyy-MM-dd"),
                    ["DisplayDateEnd"] = dp.DisplayDateEnd?.ToString("yyyy-MM-dd"),
                    ["FirstDayOfWeek"] = dp.FirstDayOfWeek.ToString()
                }
            });

            previousValue = newValue;
        };

        datePicker.SelectedDateChanged += handler;
        _handlers[hash] = handler;
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not DatePicker datePicker) return;

        var hash = datePicker.GetHashCode();
        if (_handlers.TryGetValue(hash, out var handler))
        {
            datePicker.SelectedDateChanged -= handler;
            _handlers.Remove(hash);
        }
    }
}
