using System.Windows;
using WpfEventRecorder.Core;

namespace WpfEventRecorder.SampleApp
{
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

            // Start recording automatically for demo purposes
            WpfRecorder.Start("SampleApp Demo Session");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Stop recording and save
            if (WpfRecorder.IsRecording)
            {
                WpfRecorder.Stop();
            }

            base.OnExit(e);
        }
    }
}
