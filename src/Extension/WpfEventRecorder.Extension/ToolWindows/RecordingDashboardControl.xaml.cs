using System.Windows.Controls;

namespace WpfEventRecorder.Extension.ToolWindows;

/// <summary>
/// Interaction logic for RecordingDashboardControl.xaml
/// </summary>
public partial class RecordingDashboardControl : UserControl
{
    public RecordingDashboardControl(WpfEventRecorderPackage package)
    {
        InitializeComponent();
        DataContext = new RecordingDashboardViewModel(package);
    }
}
