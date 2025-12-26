using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Extension.Commands;
using WpfEventRecorder.Extension.Options;
using WpfEventRecorder.Extension.Services;
using WpfEventRecorder.Extension.ToolWindows;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Extension;

/// <summary>
/// Main package class for WPF Event Recorder extension.
/// </summary>
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuidString)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideToolWindow(typeof(RecordingDashboardWindow), Style = VsDockStyle.Tabbed,
    DockedWidth = 400, Window = "DocumentWell", Orientation = ToolWindowOrientation.Right)]
[ProvideOptionPage(typeof(GeneralOptionsPage), "WPF Event Recorder", "General", 0, 0, true)]
[ProvideOptionPage(typeof(RecordingOptionsPage), "WPF Event Recorder", "Recording", 0, 0, true)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class WpfEventRecorderPackage : ToolkitPackage
{
    public const string PackageGuidString = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

    // Services
    private IProjectAnalyzerService? _projectAnalyzerService;
    private ICodeInjectionService? _codeInjectionService;
    private IRecordingSessionService? _recordingSessionService;
    private IExportService? _exportService;
    private IPipeServerService? _pipeServerService;

    public IProjectAnalyzerService ProjectAnalyzerService => _projectAnalyzerService ??= new ProjectAnalyzerService(this);
    public ICodeInjectionService CodeInjectionService => _codeInjectionService ??= new CodeInjectionService(this);
    public IRecordingSessionService RecordingSessionService => _recordingSessionService ??= new RecordingSessionService(this);
    public IExportService ExportService => _exportService ??= new ExportService();
    public IPipeServerService PipeServerService => _pipeServerService ??= new PipeServerService(RecordingSessionService);

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);

        // Switch to main thread for UI operations
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        // Register commands
        await StartRecordingCommand.InitializeAsync(this);
        await StopRecordingCommand.InitializeAsync(this);
        await PauseRecordingCommand.InitializeAsync(this);
        await ExportRecordingCommand.InitializeAsync(this);
        await ClearRecordingCommand.InitializeAsync(this);
        await OpenDashboardCommand.InitializeAsync(this);

        // Initialize pipe server
        await PipeServerService.StartAsync();

        VS.Events.SolutionEvents.OnAfterCloseSolution += OnSolutionClosed;
    }

    private void OnSolutionClosed()
    {
        RecordingSessionService.StopRecording();
        PipeServerService.StopAsync().FireAndForget();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PipeServerService.StopAsync().FireAndForget();
            VS.Events.SolutionEvents.OnAfterCloseSolution -= OnSolutionClosed;
        }
        base.Dispose(disposing);
    }
}
