using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace WpfEventRecorder.Services
{
    /// <summary>
    /// Service for injecting recording code into WPF applications
    /// </summary>
    public class CodeInjectionService
    {
        private readonly AsyncPackage _package;
        private readonly Dictionary<string, string> _backupFiles = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new code injection service
        /// </summary>
        public CodeInjectionService(AsyncPackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Injects recording initialization code into App.xaml.cs
        /// </summary>
        /// <param name="appXamlCsPath">Path to App.xaml.cs</param>
        /// <param name="sessionId">Session ID for IPC connection</param>
        /// <returns>True if injection was successful</returns>
        public async Task<bool> InjectRecordingInitializationAsync(string appXamlCsPath, string sessionId)
        {
            if (!File.Exists(appXamlCsPath))
            {
                return false;
            }

            // Create backup
            await BackupFileAsync(appXamlCsPath);

            var content = await ReadFileAsync(appXamlCsPath);

            // Check if already injected
            if (content.Contains("WpfEventRecorder.Core.Infrastructure"))
            {
                return true;
            }

            var modified = new StringBuilder();

            // Add using statement
            var usingInsertPoint = content.IndexOf("using ");
            if (usingInsertPoint >= 0)
            {
                modified.Append(content.Substring(0, usingInsertPoint));
                modified.AppendLine("using WpfEventRecorder.Core.Infrastructure;");
                modified.Append(content.Substring(usingInsertPoint));
            }
            else
            {
                modified.AppendLine("using WpfEventRecorder.Core.Infrastructure;");
                modified.Append(content);
            }

            content = modified.ToString();
            modified.Clear();

            // Find OnStartup method or constructor
            var startupPattern = new Regex(@"(protected\s+override\s+void\s+OnStartup\s*\([^)]*\)\s*\{)");
            var startupMatch = startupPattern.Match(content);

            if (startupMatch.Success)
            {
                // Insert after OnStartup opening brace
                var insertPoint = startupMatch.Index + startupMatch.Length;
                modified.Append(content.Substring(0, insertPoint));
                modified.AppendLine();
                modified.AppendLine("            // WPF Event Recorder - Injected Code");
                modified.AppendLine($"            RecordingBootstrapper.Instance.InitializeWithIpcAsync(this, \"{sessionId}\").ConfigureAwait(false);");
                modified.Append(content.Substring(insertPoint));
            }
            else
            {
                // No OnStartup found, try to add one
                var classPattern = new Regex(@"(public\s+partial\s+class\s+App\s*:\s*Application\s*\{)");
                var classMatch = classPattern.Match(content);

                if (classMatch.Success)
                {
                    var insertPoint = classMatch.Index + classMatch.Length;
                    modified.Append(content.Substring(0, insertPoint));
                    modified.AppendLine();
                    modified.AppendLine("        // WPF Event Recorder - Injected Code");
                    modified.AppendLine("        protected override void OnStartup(StartupEventArgs e)");
                    modified.AppendLine("        {");
                    modified.AppendLine("            base.OnStartup(e);");
                    modified.AppendLine($"            RecordingBootstrapper.Instance.InitializeWithIpcAsync(this, \"{sessionId}\").ConfigureAwait(false);");
                    modified.AppendLine("        }");
                    modified.Append(content.Substring(insertPoint));
                }
                else
                {
                    return false;
                }
            }

            await WriteFileAsync(appXamlCsPath, modified.ToString());
            return true;
        }

        /// <summary>
        /// Wraps HttpClient usages with RecordingHttpHandler
        /// </summary>
        /// <param name="usages">List of HttpClient usages to wrap</param>
        /// <returns>Number of files modified</returns>
        public async Task<int> WrapHttpClientUsagesAsync(IEnumerable<HttpClientUsageInfo> usages)
        {
            var modifiedFiles = new HashSet<string>();

            foreach (var usage in usages.Where(u => u.UsageType == "Instantiation"))
            {
                await BackupFileAsync(usage.FilePath);

                var content = await ReadFileAsync(usage.FilePath);

                // Add using statement if not present
                if (!content.Contains("WpfEventRecorder.Core.Infrastructure"))
                {
                    var usingInsertPoint = content.IndexOf("using ");
                    if (usingInsertPoint >= 0)
                    {
                        content = content.Insert(usingInsertPoint,
                            "using WpfEventRecorder.Core.Infrastructure;\r\n");
                    }
                }

                // Replace new HttpClient() with RecordingBootstrapper.Instance.CreateRecordingHttpClient()
                var pattern = new Regex(@"new\s+HttpClient\s*\(\s*\)");
                var modified = pattern.Replace(content,
                    "RecordingBootstrapper.Instance.CreateRecordingHttpClient()");

                if (modified != content)
                {
                    await WriteFileAsync(usage.FilePath, modified);
                    modifiedFiles.Add(usage.FilePath);
                }
            }

            return modifiedFiles.Count;
        }

        /// <summary>
        /// Adds RecordViewModel attribute to ViewModels
        /// </summary>
        /// <param name="viewModels">List of ViewModels to annotate</param>
        /// <returns>Number of files modified</returns>
        public async Task<int> AnnotateViewModelsAsync(IEnumerable<ViewModelInfo> viewModels)
        {
            var modifiedFiles = new HashSet<string>();

            foreach (var vm in viewModels)
            {
                await BackupFileAsync(vm.FilePath);

                var content = await ReadFileAsync(vm.FilePath);

                // Skip if already annotated
                if (content.Contains("[RecordViewModel]"))
                {
                    continue;
                }

                // Add using statement if not present
                if (!content.Contains("WpfEventRecorder.Core.Attributes"))
                {
                    var usingInsertPoint = content.IndexOf("using ");
                    if (usingInsertPoint >= 0)
                    {
                        content = content.Insert(usingInsertPoint,
                            "using WpfEventRecorder.Core.Attributes;\r\n");
                    }
                }

                // Add attribute before class declaration
                var classPattern = new Regex($@"(\s*)(public\s+(?:partial\s+)?class\s+{vm.ClassName})");
                var modified = classPattern.Replace(content, "$1[RecordViewModel]\r\n$1$2");

                if (modified != content)
                {
                    await WriteFileAsync(vm.FilePath, modified);
                    modifiedFiles.Add(vm.FilePath);
                }
            }

            return modifiedFiles.Count;
        }

        /// <summary>
        /// Restores all backed up files
        /// </summary>
        public async Task RestoreAllBackupsAsync()
        {
            foreach (var backup in _backupFiles)
            {
                if (File.Exists(backup.Value))
                {
                    await Task.Run(() =>
                    {
                        File.Copy(backup.Value, backup.Key, overwrite: true);
                        File.Delete(backup.Value);
                    });
                }
            }

            _backupFiles.Clear();
        }

        /// <summary>
        /// Removes all injected recording code
        /// </summary>
        /// <param name="filePath">Path to file to clean</param>
        public async Task RemoveInjectedCodeAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var content = await ReadFileAsync(filePath);

            // Remove injected using statements
            content = Regex.Replace(content,
                @"using WpfEventRecorder\.Core\.(Infrastructure|Attributes);\r?\n?",
                "");

            // Remove injected OnStartup code
            content = Regex.Replace(content,
                @"\s*// WPF Event Recorder - Injected Code\r?\n.*?RecordingBootstrapper\.Instance.*?\r?\n",
                "");

            // Remove RecordViewModel attributes
            content = Regex.Replace(content,
                @"\s*\[RecordViewModel\]\r?\n",
                "");

            // Remove CreateRecordingHttpClient replacements
            content = content.Replace(
                "RecordingBootstrapper.Instance.CreateRecordingHttpClient()",
                "new HttpClient()");

            await WriteFileAsync(filePath, content);
        }

        /// <summary>
        /// Generates a recording-enabled App.xaml.cs template
        /// </summary>
        /// <param name="namespace_">Namespace for the generated code</param>
        /// <param name="sessionId">Session ID for IPC</param>
        /// <returns>Generated code string</returns>
        public string GenerateRecordingAppTemplate(string namespace_, string sessionId)
        {
            return $@"using System.Windows;
using WpfEventRecorder.Core.Infrastructure;

namespace {namespace_}
{{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {{
        protected override async void OnStartup(StartupEventArgs e)
        {{
            base.OnStartup(e);

            // Initialize WPF Event Recorder
            await RecordingBootstrapper.Instance.InitializeWithIpcAsync(this, ""{sessionId}"");
        }}

        protected override void OnExit(ExitEventArgs e)
        {{
            // Cleanup recording
            RecordingBootstrapper.Instance.Dispose();
            base.OnExit(e);
        }}
    }}
}}
";
        }

        /// <summary>
        /// Generates code to add the Core library reference
        /// </summary>
        /// <param name="project">Project to modify</param>
        public async Task AddCoreReferenceAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (project == null) return;

            // This would typically add a NuGet package reference
            // For now, we'll generate the code that would be needed
            // In a full implementation, use the NuGet API to add the package
        }

        private async Task BackupFileAsync(string filePath)
        {
            if (_backupFiles.ContainsKey(filePath))
            {
                return;
            }

            var backupPath = filePath + ".wpfrecorder.bak";

            await Task.Run(() =>
            {
                File.Copy(filePath, backupPath, overwrite: true);
            });

            _backupFiles[filePath] = backupPath;
        }

        private Task<string> ReadFileAsync(string filePath)
        {
            return Task.Run(() => File.ReadAllText(filePath, Encoding.UTF8));
        }

        private Task WriteFileAsync(string filePath, string content)
        {
            return Task.Run(() => File.WriteAllText(filePath, content, Encoding.UTF8));
        }
    }
}
