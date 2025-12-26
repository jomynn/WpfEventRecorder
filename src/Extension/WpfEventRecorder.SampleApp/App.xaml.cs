using System.Windows;
using WpfEventRecorder.Core.Recording;
using WpfEventRecorder.SampleApp.Services;

namespace WpfEventRecorder.SampleApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IApiService? _apiService;

    public IApiService ApiService => _apiService ??= new ApiService();

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize WPF Event Recorder
        await RecordingBootstrapper.InitializeAsync(this);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await RecordingBootstrapper.ShutdownAsync();
        base.OnExit(e);
    }
}
