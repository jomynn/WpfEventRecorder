using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Commands
{
    internal sealed class StartRecordingCommand
    {
        private readonly AsyncPackage _package;
        private readonly OleMenuCommand _menuCommand;

        public static StartRecordingCommand? Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new StartRecordingCommand(package, commandService);
        }

        private StartRecordingCommand(AsyncPackage package, OleMenuCommandService? commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));

            var commandId = new CommandID(CommandIds.CommandSetGuid, CommandIds.StartRecordingId);
            _menuCommand = new OleMenuCommand(Execute, commandId);
            _menuCommand.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService?.AddCommand(_menuCommand);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _menuCommand.Enabled = !RecordingHub.Instance.IsRecording;
            _menuCommand.Visible = true;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RecordingHub.Instance.Start();

            var pkg = _package as WpfEventRecorderPackage;
            pkg?.SetStatusBarText("Recording WPF Events...");
            pkg?.RefreshCommands();
        }
    }
}
