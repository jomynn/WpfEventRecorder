using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using WpfEventRecorder.Commands;
using WpfEventRecorder.ToolWindows;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder
{
    /// <summary>
    /// WPF Event Recorder VS2022 Extension Package
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CommandIds.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(RecorderToolWindow), Style = VsDockStyle.Tabbed,
                       Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class WpfEventRecorderPackage : AsyncPackage
    {
        /// <summary>
        /// Package initialization
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken,
                                                       IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Initialize commands
            await StartRecordingCommand.InitializeAsync(this);
            await StopRecordingCommand.InitializeAsync(this);
            await SaveRecordingCommand.InitializeAsync(this);
            await ClearRecordingCommand.InitializeAsync(this);
            await OpenToolWindowCommand.InitializeAsync(this);

            // Set initial status
            SetStatusBarText("WPF Recorder Ready");
        }

        /// <summary>
        /// Updates VS status bar text
        /// </summary>
        public void SetStatusBarText(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (GetService(typeof(SVsStatusbar)) is IVsStatusbar statusBar)
            {
                statusBar.SetText(text);
            }
        }

        /// <summary>
        /// Forces command UI refresh
        /// </summary>
        public void RefreshCommands()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (GetService(typeof(SVsUIShell)) is IVsUIShell shell)
            {
                shell.UpdateCommandUI(0);
            }
        }
    }
}
