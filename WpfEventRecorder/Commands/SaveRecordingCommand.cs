using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using WpfEventRecorder.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace WpfEventRecorder.Commands
{
    internal sealed class SaveRecordingCommand
    {
        private readonly AsyncPackage _package;
        private readonly OleMenuCommand _menuCommand;

        public static SaveRecordingCommand? Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SaveRecordingCommand(package, commandService);
        }

        private SaveRecordingCommand(AsyncPackage package, OleMenuCommandService? commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));

            var commandId = new CommandID(CommandIds.CommandSetGuid, CommandIds.SaveRecordingId);
            _menuCommand = new OleMenuCommand(Execute, commandId);
            _menuCommand.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService?.AddCommand(_menuCommand);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _menuCommand.Enabled = RecordingHub.Instance.EntryCount > 0;
            _menuCommand.Visible = true;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".json",
                FileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                Title = "Save Recording"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    RecordingHub.Instance.SaveToFile(dialog.FileName);

                    var pkg = _package as WpfEventRecorderPackage;
                    pkg?.SetStatusBarText($"Recording saved to {System.IO.Path.GetFileName(dialog.FileName)}");
                }
                catch (Exception ex)
                {
                    var pkg = _package as WpfEventRecorderPackage;
                    pkg?.SetStatusBarText($"Error saving: {ex.Message}");
                }
            }
        }
    }
}
