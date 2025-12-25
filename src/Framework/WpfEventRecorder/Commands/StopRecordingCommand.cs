using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Commands
{
    internal sealed class StopRecordingCommand
    {
        private readonly AsyncPackage _package;
        private readonly OleMenuCommand _menuCommand;

        public static StopRecordingCommand? Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new StopRecordingCommand(package, commandService);
        }

        private StopRecordingCommand(AsyncPackage package, OleMenuCommandService? commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));

            var commandId = new CommandID(CommandIds.CommandSetGuid, CommandIds.StopRecordingId);
            _menuCommand = new OleMenuCommand(Execute, commandId);
            _menuCommand.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService?.AddCommand(_menuCommand);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _menuCommand.Enabled = RecordingHub.Instance.IsRecording;
            _menuCommand.Visible = true;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RecordingHub.Instance.Stop();

            var count = RecordingHub.Instance.EntryCount;
            var pkg = _package as WpfEventRecorderPackage;
            pkg?.SetStatusBarText($"Recording Stopped - {count} events captured");
            pkg?.RefreshCommands();
        }
    }
}
