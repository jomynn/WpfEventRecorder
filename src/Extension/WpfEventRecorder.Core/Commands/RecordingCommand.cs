using System.Diagnostics;
using System.Windows.Input;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Commands;

/// <summary>
/// A command wrapper that records command executions.
/// </summary>
public class RecordingCommand : ICommand
{
    private readonly ICommand _innerCommand;
    private readonly RecordingSession? _session;
    private readonly string? _commandName;

    public RecordingCommand(ICommand innerCommand, RecordingSession? session = null, string? commandName = null)
    {
        _innerCommand = innerCommand ?? throw new ArgumentNullException(nameof(innerCommand));
        _session = session ?? RecordingBootstrapper.CurrentSession;
        _commandName = commandName;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => _innerCommand.CanExecuteChanged += value;
        remove => _innerCommand.CanExecuteChanged -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _innerCommand.CanExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        var stopwatch = Stopwatch.StartNew();
        var commandEvent = new CommandEvent
        {
            CommandName = _commandName ?? _innerCommand.GetType().Name,
            CommandType = _innerCommand.GetType().FullName,
            CommandParameter = parameter
        };

        try
        {
            _innerCommand.Execute(parameter);
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
    /// Creates a recording wrapper for an existing command.
    /// </summary>
    public static ICommand Wrap(ICommand command, string? commandName = null)
    {
        return new RecordingCommand(command, commandName: commandName);
    }
}

/// <summary>
/// A command wrapper that records command executions with a typed parameter.
/// </summary>
public class RecordingCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private readonly RecordingSession? _session;
    private readonly string? _commandName;

    public RecordingCommand(Action<T?> execute, Func<T?, bool>? canExecute = null,
        RecordingSession? session = null, string? commandName = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _session = session ?? RecordingBootstrapper.CurrentSession;
        _commandName = commandName;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke((T?)parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        var stopwatch = Stopwatch.StartNew();
        var commandEvent = new CommandEvent
        {
            CommandName = _commandName ?? _execute.Method.Name,
            CommandParameter = parameter
        };

        try
        {
            _execute((T?)parameter);
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
}
