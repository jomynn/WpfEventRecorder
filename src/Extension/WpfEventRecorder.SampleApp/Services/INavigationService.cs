namespace WpfEventRecorder.SampleApp.Services;

/// <summary>
/// Navigation service interface.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the current view name.
    /// </summary>
    string CurrentView { get; }

    /// <summary>
    /// Event raised when navigation occurs.
    /// </summary>
    event EventHandler<string>? Navigated;

    /// <summary>
    /// Navigates to a view.
    /// </summary>
    void NavigateTo(string viewName);

    /// <summary>
    /// Navigates back.
    /// </summary>
    bool CanGoBack { get; }
    void GoBack();
}
