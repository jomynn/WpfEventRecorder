using System.Diagnostics;
using System.Windows.Input;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Commands;

/// <summary>
/// A relay command implementation with built-in recording support.
/// </summary>
public class RecordingRelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private readonly RecordingSession? _session;
    private readonly string _commandName;
    private readonly bool _enableRecording;

    public RecordingRelayCommand(Action execute, Func<bool>? canExecute = null,
        string? commandName = null, bool enableRecording = true)
        : this(
            _ => execute(),
            canExecute != null ? _ => canExecute() : null,
            commandName,
            enableRecording)
    {
    }

    public RecordingRelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null,
        string? commandName = null, bool enableRecording = true)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _commandName = commandName ?? execute.Method.Name;
        _enableRecording = enableRecording;
        _session = RecordingBootstrapper.CurrentSession;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (!_enableRecording)
        {
            _execute(parameter);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var commandEvent = new CommandEvent
        {
            CommandName = _commandName,
            CommandParameter = parameter,
            CommandType = GetType().FullName
        };

        try
        {
            _execute(parameter);
            stopwatch.Stop();
            commandEvent.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
            commandEvent.IsSuccess = true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            commandEvent.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
            commandEvent.IsSuccess = false;
            commandEvent.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            _session?.AddEvent(commandEvent);
        }
    }

    /// <summary>
    /// Raises CanExecuteChanged event.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}

/// <summary>
/// A generic relay command implementation with built-in recording support.
/// </summary>
public class RecordingRelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private readonly RecordingSession? _session;
    private readonly string _commandName;
    private readonly bool _enableRecording;

    public RecordingRelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null,
        string? commandName = null, bool enableRecording = true)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _commandName = commandName ?? execute.Method.Name;
        _enableRecording = enableRecording;
        _session = RecordingBootstrapper.CurrentSession;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        if (parameter == null && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            return _canExecute?.Invoke(default) ?? true;

        return _canExecute?.Invoke((T?)parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        var typedParameter = parameter is T t ? t : default;

        if (!_enableRecording)
        {
            _execute(typedParameter);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var commandEvent = new CommandEvent
        {
            CommandName = _commandName,
            CommandParameter = parameter,
            CommandType = GetType().FullName
        };

        try
        {
            _execute(typedParameter);
            stopwatch.Stop();
            commandEvent.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
            commandEvent.IsSuccess = true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            commandEvent.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
            commandEvent.IsSuccess = false;
            commandEvent.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            _session?.AddEvent(commandEvent);
        }
    }

    /// <summary>
    /// Raises CanExecuteChanged event.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
