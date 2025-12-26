using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.SampleApp.Services;

/// <summary>
/// Implementation of navigation service with recording.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Stack<string> _history = new();
    private string _currentView = "";

    public string CurrentView
    {
        get => _currentView;
        private set
        {
            var previousView = _currentView;
            _currentView = value;

            // Record navigation event
            RecordingBootstrapper.RecordCustomEvent(new NavigationEvent
            {
                NavigationType = NavigationType.ViewNavigation,
                FromView = previousView,
                ToView = value
            });

            Navigated?.Invoke(this, value);
        }
    }

    public event EventHandler<string>? Navigated;

    public bool CanGoBack => _history.Count > 0;

    public void NavigateTo(string viewName)
    {
        if (!string.IsNullOrEmpty(_currentView))
        {
            _history.Push(_currentView);
        }

        CurrentView = viewName;
    }

    public void GoBack()
    {
        if (CanGoBack)
        {
            var previousView = _history.Pop();
            CurrentView = previousView;
        }
    }
}
