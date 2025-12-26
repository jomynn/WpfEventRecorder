using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfEventRecorder.Core.ViewModels;

namespace WpfEventRecorder.SampleApp.ViewModels;

/// <summary>
/// Base class for all ViewModels in the sample app.
/// </summary>
public abstract class ViewModelBase : RecordingViewModelBase
{
    private bool _isBusy;
    private string? _busyMessage;

    /// <summary>
    /// Gets or sets whether the ViewModel is busy.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Gets or sets the busy message.
    /// </summary>
    public string? BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }

    protected async Task RunBusyAsync(Func<Task> action, string? message = null)
    {
        IsBusy = true;
        BusyMessage = message;

        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    protected async Task<T?> RunBusyAsync<T>(Func<Task<T>> action, string? message = null)
    {
        IsBusy = true;
        BusyMessage = message;

        try
        {
            return await action();
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }
}
