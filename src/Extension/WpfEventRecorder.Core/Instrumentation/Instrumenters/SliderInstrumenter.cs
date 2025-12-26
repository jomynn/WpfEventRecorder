using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Helpers;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation.Instrumenters;

/// <summary>
/// Instrumenter for Slider controls.
/// </summary>
public class SliderInstrumenter : IControlInstrumenter
{
    public Type ControlType => typeof(Slider);

    private readonly Dictionary<int, DragCompletedEventHandler> _handlers = new();
    private readonly Dictionary<int, double> _previousValues = new();

    public bool CanInstrument(DependencyObject element) => element is Slider;

    public void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration)
    {
        if (element is not Slider slider) return;

        var hash = slider.GetHashCode();
        _previousValues[hash] = slider.Value;

        // Use DragCompleted instead of ValueChanged to avoid recording every intermediate value
        slider.AddHandler(Thumb.DragCompletedEvent, (DragCompletedEventHandler)((sender, e) =>
        {
            RecordSliderChange(slider, session, hash);
        }));

        // Also record if value changes programmatically or via keyboard
        slider.LostFocus += (sender, e) =>
        {
            if (_previousValues.TryGetValue(hash, out var prev) && Math.Abs(prev - slider.Value) > 0.001)
            {
                RecordSliderChange(slider, session, hash);
            }
        };
    }

    private void RecordSliderChange(Slider slider, RecordingSession session, int hash)
    {
        var oldValue = _previousValues.GetValueOrDefault(hash, slider.Minimum);
        var newValue = slider.Value;

        session.AddEvent(new InputEvent
        {
            InputType = InputEventType.SliderChanged,
            SourceElementName = slider.Name,
            SourceElementType = nameof(Slider),
            AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(slider),
            OldValue = oldValue,
            NewValue = newValue,
            BindingPath = BindingHelper.GetBindingPath(slider, RangeBase.ValueProperty),
            ViewModelProperty = BindingHelper.GetViewModelPropertyName(
                BindingHelper.GetBindingPath(slider, RangeBase.ValueProperty)),
            ControlInfo = new Dictionary<string, object?>
            {
                ["Minimum"] = slider.Minimum,
                ["Maximum"] = slider.Maximum,
                ["TickFrequency"] = slider.TickFrequency,
                ["IsSnapToTickEnabled"] = slider.IsSnapToTickEnabled
            }
        });

        _previousValues[hash] = newValue;
    }

    public void Uninstrument(DependencyObject element)
    {
        if (element is not Slider slider) return;

        var hash = slider.GetHashCode();
        _handlers.Remove(hash);
        _previousValues.Remove(hash);
    }
}
