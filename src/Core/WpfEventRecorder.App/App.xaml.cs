using System.Windows;
using WpfEventRecorder.Core;

namespace WpfEventRecorder.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize the WPF Recorder
        WpfRecorder.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Stop recording if active
        if (WpfRecorder.IsRecording)
        {
            WpfRecorder.Stop();
        }

        base.OnExit(e);
    }
}
