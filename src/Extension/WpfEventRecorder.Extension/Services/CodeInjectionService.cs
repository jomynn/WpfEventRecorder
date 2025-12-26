using System.Text.RegularExpressions;
using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Implementation of code injection service.
/// </summary>
public class CodeInjectionService : ICodeInjectionService
{
    private readonly WpfEventRecorderPackage _package;

    public CodeInjectionService(WpfEventRecorderPackage package)
    {
        _package = package;
    }

    public async Task<bool> AddRecorderPackageAsync(ProjectInfo project)
    {
        // This would typically use NuGet APIs to add the package
        // For now, we'll modify the project file directly

        var content = await File.ReadAllTextAsync(project.FilePath);

        if (content.Contains("WpfEventRecorder.Core"))
            return true; // Already added

        // Find the ItemGroup with PackageReferences
        var packageRefPattern = @"(<ItemGroup>\s*<PackageReference)";
        var match = Regex.Match(content, packageRefPattern);

        if (match.Success)
        {
            var insertPoint = match.Index;
            var packageRef = @"  <ItemGroup>
    <PackageReference Include=""WpfEventRecorder.Core"" Version=""1.0.0"" />
  </ItemGroup>

";
            content = content.Insert(insertPoint, packageRef);
            await File.WriteAllTextAsync(project.FilePath, content);
            return true;
        }

        return false;
    }

    public async Task<bool> InjectInitializationCodeAsync(ProjectInfo project)
    {
        var projectDir = Path.GetDirectoryName(project.FilePath);
        if (string.IsNullOrEmpty(projectDir)) return false;

        var appXamlCs = Path.Combine(projectDir, "App.xaml.cs");
        if (!File.Exists(appXamlCs)) return false;

        var content = await File.ReadAllTextAsync(appXamlCs);

        if (content.Contains("RecordingBootstrapper"))
            return true; // Already initialized

        // Add using statement
        if (!content.Contains("using WpfEventRecorder.Core.Recording;"))
        {
            var usingInsertPoint = content.IndexOf("namespace");
            if (usingInsertPoint > 0)
            {
                content = content.Insert(usingInsertPoint, "using WpfEventRecorder.Core.Recording;\n");
            }
        }

        // Find OnStartup or constructor
        var startupPattern = @"(protected override void OnStartup\(StartupEventArgs e\)\s*\{)";
        var startupMatch = Regex.Match(content, startupPattern);

        if (startupMatch.Success)
        {
            var initCode = @"
            // Initialize WPF Event Recorder
            _ = RecordingBootstrapper.InitializeAsync(this);

";
            content = content.Insert(startupMatch.Index + startupMatch.Length, initCode);
        }
        else
        {
            // Add OnStartup method if it doesn't exist
            var classPattern = @"(public partial class App\s*:\s*Application\s*\{)";
            var classMatch = Regex.Match(content, classPattern);

            if (classMatch.Success)
            {
                var onStartupMethod = @"
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize WPF Event Recorder
            _ = RecordingBootstrapper.InitializeAsync(this);
        }
";
                content = content.Insert(classMatch.Index + classMatch.Length, onStartupMethod);
            }
        }

        await File.WriteAllTextAsync(appXamlCs, content);
        return true;
    }

    public async Task<int> WrapHttpClientUsagesAsync(ProjectInfo project)
    {
        var projectDir = Path.GetDirectoryName(project.FilePath);
        if (string.IsNullOrEmpty(projectDir)) return 0;

        var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories);
        var count = 0;

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);

            if (!content.Contains("new HttpClient("))
                continue;

            // Replace direct HttpClient instantiation
            var originalContent = content;
            content = Regex.Replace(content,
                @"new\s+HttpClient\(\s*\)",
                "RecordingBootstrapper.CreateRecordingHttpClient()");

            if (content != originalContent)
            {
                // Add using if needed
                if (!content.Contains("using WpfEventRecorder.Core.Recording;"))
                {
                    var usingInsertPoint = content.IndexOf("namespace");
                    if (usingInsertPoint > 0)
                    {
                        content = content.Insert(usingInsertPoint, "using WpfEventRecorder.Core.Recording;\n");
                    }
                }

                await File.WriteAllTextAsync(file, content);
                count++;
            }
        }

        return count;
    }

    public async Task<int> AnnotateViewModelsAsync(ProjectInfo project)
    {
        var viewModels = await _package.ProjectAnalyzerService.GetViewModelsAsync(project);
        var count = 0;

        foreach (var vmFile in viewModels)
        {
            var content = await File.ReadAllTextAsync(vmFile);

            if (content.Contains("[RecordViewModel]"))
                continue;

            // Find class declaration
            var classPattern = @"(\s*public\s+(partial\s+)?class\s+\w+ViewModel)";
            var match = Regex.Match(content, classPattern);

            if (match.Success)
            {
                // Add attribute before class
                content = content.Insert(match.Index, "\n    [RecordViewModel]");

                // Add using if needed
                if (!content.Contains("using WpfEventRecorder.Core.Attributes;"))
                {
                    var usingInsertPoint = content.IndexOf("namespace");
                    if (usingInsertPoint > 0)
                    {
                        content = content.Insert(usingInsertPoint, "using WpfEventRecorder.Core.Attributes;\n");
                    }
                }

                await File.WriteAllTextAsync(vmFile, content);
                count++;
            }
        }

        return count;
    }
}
