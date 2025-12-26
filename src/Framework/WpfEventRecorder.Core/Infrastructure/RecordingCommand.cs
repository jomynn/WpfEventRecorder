using System;
using System.Windows.Input;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;

namespace WpfEventRecorder.Core.Infrastructure
{
    /// <summary>
    /// ICommand wrapper that records command executions
    /// </summary>
    public class RecordingCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly string _commandName;
        private readonly string _viewModelType;
        private readonly bool _recordParameter;

        /// <summary>
        /// Event raised when CanExecute changes
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Creates a new recording command
        /// </summary>
        /// <param name="execute">Execute action</param>
        /// <param name="commandName">Name of the command for recording</param>
        /// <param name="viewModelType">ViewModel type name</param>
        /// <param name="canExecute">CanExecute predicate</param>
        /// <param name="recordParameter">Whether to record the command parameter</param>
        public RecordingCommand(Action<object> execute, string commandName, string viewModelType = null,
            Func<object, bool> canExecute = null, bool recordParameter = true)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _commandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
            _viewModelType = viewModelType;
            _canExecute = canExecute;
            _recordParameter = recordParameter;
        }

        /// <summary>
        /// Creates a new recording command from an existing command
        /// </summary>
        /// <param name="innerCommand">The command to wrap</param>
        /// <param name="commandName">Name of the command for recording</param>
        /// <param name="viewModelType">ViewModel type name</param>
        /// <param name="recordParameter">Whether to record the command parameter</param>
        public static RecordingCommand Wrap(ICommand innerCommand, string commandName,
            string viewModelType = null, bool recordParameter = true)
        {
            return new RecordingCommand(
                param =>
                {
                    if (innerCommand.CanExecute(param))
                    {
                        innerCommand.Execute(param);
                    }
                },
                commandName,
                viewModelType,
                innerCommand.CanExecute,
                recordParameter);
        }

        /// <summary>
        /// Determines if the command can execute
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executes the command and records the execution
        /// </summary>
        public void Execute(object parameter)
        {
            var hub = RecordingHub.Instance;
            var startTime = DateTime.UtcNow;
            Exception error = null;

            try
            {
                _execute(parameter);
            }
            catch (Exception ex)
            {
                error = ex;
                throw;
            }
            finally
            {
                if (hub.IsRecording)
                {
                    RecordExecution(parameter, error, DateTime.UtcNow - startTime);
                }
            }
        }

        private void RecordExecution(object parameter, Exception error, TimeSpan duration)
        {
            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UIClick,
                DurationMs = (long)duration.TotalMilliseconds,
                UIInfo = new UIInfo
                {
                    ControlType = "Command",
                    ControlName = _commandName,
                    WindowType = _viewModelType,
                    NewValue = _recordParameter ? parameter?.ToString() : null,
                    Properties = error != null
                        ? new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "Error", error.Message }
                        }
                        : null
                }
            };

            RecordingHub.Instance.AddEntry(entry);
        }

        /// <summary>
        /// Raises CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Generic ICommand wrapper that records command executions
    /// </summary>
    /// <typeparam name="T">Parameter type</typeparam>
    public class RecordingCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        private readonly string _commandName;
        private readonly string _viewModelType;
        private readonly bool _recordParameter;

        /// <summary>
        /// Event raised when CanExecute changes
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Creates a new recording command
        /// </summary>
        public RecordingCommand(Action<T> execute, string commandName, string viewModelType = null,
            Func<T, bool> canExecute = null, bool recordParameter = true)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _commandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
            _viewModelType = viewModelType;
            _canExecute = canExecute;
            _recordParameter = recordParameter;
        }

        /// <summary>
        /// Determines if the command can execute
        /// </summary>
        public bool CanExecute(object parameter)
        {
            if (parameter == null && default(T) != null)
                return false;

            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        /// <summary>
        /// Executes the command and records the execution
        /// </summary>
        public void Execute(object parameter)
        {
            var hub = RecordingHub.Instance;
            var startTime = DateTime.UtcNow;
            Exception error = null;

            try
            {
                _execute((T)parameter);
            }
            catch (Exception ex)
            {
                error = ex;
                throw;
            }
            finally
            {
                if (hub.IsRecording)
                {
                    RecordExecution(parameter, error, DateTime.UtcNow - startTime);
                }
            }
        }

        private void RecordExecution(object parameter, Exception error, TimeSpan duration)
        {
            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.UIClick,
                DurationMs = (long)duration.TotalMilliseconds,
                UIInfo = new UIInfo
                {
                    ControlType = "Command",
                    ControlName = _commandName,
                    WindowType = _viewModelType,
                    NewValue = _recordParameter ? parameter?.ToString() : null,
                    Properties = error != null
                        ? new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "Error", error.Message }
                        }
                        : null
                }
            };

            RecordingHub.Instance.AddEntry(entry);
        }

        /// <summary>
        /// Raises CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Simple RelayCommand implementation with recording
    /// </summary>
    public class RelayCommand : RecordingCommand
    {
        /// <summary>
        /// Creates a new relay command
        /// </summary>
        public RelayCommand(Action execute, string commandName, string viewModelType = null,
            Func<bool> canExecute = null)
            : base(_ => execute(), commandName, viewModelType, _ => canExecute?.Invoke() ?? true)
        {
        }

        /// <summary>
        /// Creates a new relay command with parameter
        /// </summary>
        public RelayCommand(Action<object> execute, string commandName, string viewModelType = null,
            Func<object, bool> canExecute = null)
            : base(execute, commandName, viewModelType, canExecute)
        {
        }
    }
}
