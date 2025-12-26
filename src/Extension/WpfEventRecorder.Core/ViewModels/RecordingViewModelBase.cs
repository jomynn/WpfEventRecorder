using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.ViewModels;

/// <summary>
/// Base ViewModel class that records property changes.
/// </summary>
public abstract class RecordingViewModelBase : INotifyPropertyChanged
{
    private readonly RecordingSession? _session;
    private readonly bool _recordPropertyChanges;

    protected RecordingViewModelBase(bool recordPropertyChanges = false)
    {
        _session = RecordingBootstrapper.CurrentSession;
        _recordPropertyChanges = recordPropertyChanges && _session?.Configuration.RecordPropertyChanges == true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets a property value and raises PropertyChanged if the value changed.
    /// Records the property change if recording is enabled.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        var oldValue = field;
        field = value;

        OnPropertyChanged(propertyName);

        if (_recordPropertyChanges && propertyName != null)
        {
            RecordPropertyChange(propertyName, oldValue, value);
        }

        return true;
    }

    /// <summary>
    /// Sets a property value with validation.
    /// </summary>
    protected bool SetPropertyWithValidation<T>(ref T field, T value, Func<T, bool> validate,
        [CallerMemberName] string? propertyName = null)
    {
        if (!validate(value))
            return false;

        return SetProperty(ref field, value, propertyName);
    }

    /// <summary>
    /// Sets a property value and executes a callback after the change.
    /// </summary>
    protected bool SetPropertyWithCallback<T>(ref T field, T value, Action? onChanged,
        [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
            return false;

        onChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises PropertyChanged for multiple properties.
    /// </summary>
    protected void OnPropertiesChanged(params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Records a property change event.
    /// </summary>
    private void RecordPropertyChange<T>(string propertyName, T? oldValue, T? newValue)
    {
        _session?.AddEvent(new InputEvent
        {
            InputType = InputEventType.TextChanged, // Property changes are treated as text changes
            SourceElementName = GetType().Name,
            SourceElementType = "ViewModel",
            ViewModelProperty = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            Metadata = new Dictionary<string, object?>
            {
                ["ViewModelType"] = GetType().FullName,
                ["PropertyType"] = typeof(T).Name,
                ["IsPropertyChange"] = true
            }
        });
    }
}
