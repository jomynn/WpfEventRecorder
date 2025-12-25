using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using WpfEventRecorder.ToolWindows;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Commands
{
    internal sealed class OpenToolWindowCommand
    {
        private readonly AsyncPackage _package;
        private readonly OleMenuCommand _menuCommand;

        public static OpenToolWindowCommand? Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenToolWindowCommand(package, commandService);
        }

        private OpenToolWindowCommand(AsyncPackage package, OleMenuCommandService? commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));

            var commandId = new CommandID(CommandIds.CommandSetGuid, CommandIds.OpenToolWindowId);
            _menuCommand = new OleMenuCommand(Execute, commandId);
            _menuCommand.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService?.AddCommand(_menuCommand);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _menuCommand.Enabled = true;
            _menuCommand.Visible = true;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _package.JoinableTaskFactory.RunAsync(async delegate
            {
                var window = await _package.ShowToolWindowAsync(
                    typeof(RecorderToolWindow),
                    0,
                    create: true,
                    _package.DisposalToken);

                if (window?.Frame is IVsWindowFrame windowFrame)
                {
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
                }
            });
        }
    }
}
