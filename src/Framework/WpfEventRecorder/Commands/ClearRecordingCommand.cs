using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Commands
{
    internal sealed class ClearRecordingCommand
    {
        private readonly AsyncPackage _package;
        private readonly OleMenuCommand _menuCommand;

        public static ClearRecordingCommand? Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ClearRecordingCommand(package, commandService);
        }

        private ClearRecordingCommand(AsyncPackage package, OleMenuCommandService? commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));

            var commandId = new CommandID(CommandIds.CommandSetGuid, CommandIds.ClearRecordingId);
            _menuCommand = new OleMenuCommand(Execute, commandId);
            _menuCommand.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService?.AddCommand(_menuCommand);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _menuCommand.Enabled = RecordingHub.Instance.EntryCount > 0 && !RecordingHub.Instance.IsRecording;
            _menuCommand.Visible = true;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RecordingHub.Instance.Clear();

            var pkg = _package as WpfEventRecorderPackage;
            pkg?.SetStatusBarText("Recording cleared");
            pkg?.RefreshCommands();
        }
    }
}
