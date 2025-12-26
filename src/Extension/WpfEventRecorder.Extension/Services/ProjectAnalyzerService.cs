using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Implementation of project analyzer service.
/// </summary>
public class ProjectAnalyzerService : IProjectAnalyzerService
{
    private readonly WpfEventRecorderPackage _package;

    public ProjectAnalyzerService(WpfEventRecorderPackage package)
    {
        _package = package;
    }

    public async Task<IEnumerable<ProjectInfo>> GetWpfProjectsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var projects = new List<ProjectInfo>();
        var solution = await VS.Solutions.GetCurrentSolutionAsync();

        if (solution == null) return projects;

        foreach (var project in await VS.Solutions.GetAllProjectsAsync())
        {
            var info = await AnalyzeProjectAsync(project);
            if (info?.IsWpfApplication == true)
            {
                projects.Add(info);
            }
        }

        return projects;
    }

    public async Task<IEnumerable<string>> GetViewModelsAsync(ProjectInfo project)
    {
        var viewModels = new List<string>();
        var projectDir = Path.GetDirectoryName(project.FilePath);

        if (string.IsNullOrEmpty(projectDir)) return viewModels;

        var files = Directory.GetFiles(projectDir, "*ViewModel.cs", SearchOption.AllDirectories);
        viewModels.AddRange(files);

        // Also look for files in ViewModels folder
        var vmFolder = Path.Combine(projectDir, "ViewModels");
        if (Directory.Exists(vmFolder))
        {
            files = Directory.GetFiles(vmFolder, "*.cs", SearchOption.AllDirectories);
            viewModels.AddRange(files.Where(f => !viewModels.Contains(f)));
        }

        return await Task.FromResult(viewModels.Distinct());
    }

    public async Task<IEnumerable<string>> GetViewsAsync(ProjectInfo project)
    {
        var views = new List<string>();
        var projectDir = Path.GetDirectoryName(project.FilePath);

        if (string.IsNullOrEmpty(projectDir)) return views;

        // Find XAML files
        var xamlFiles = Directory.GetFiles(projectDir, "*.xaml", SearchOption.AllDirectories);
        views.AddRange(xamlFiles.Where(f =>
            !f.Contains("App.xaml") &&
            !f.Contains("Themes\\") &&
            !f.Contains("Resources\\")));

        return await Task.FromResult(views);
    }

    public async Task<bool> HasRecorderPackageAsync(ProjectInfo project)
    {
        var projectDir = Path.GetDirectoryName(project.FilePath);
        if (string.IsNullOrEmpty(projectDir)) return false;

        // Check project file for reference
        var projectContent = await File.ReadAllTextAsync(project.FilePath);
        return projectContent.Contains("WpfEventRecorder.Core") ||
               projectContent.Contains("WpfEventRecorder");
    }

    public async Task<ProjectInfo?> GetStartupProjectAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var startupProjects = await VS.Solutions.GetStartupProjectsAsync();
        var startupProject = startupProjects?.FirstOrDefault();

        if (startupProject == null) return null;

        return await AnalyzeProjectAsync(startupProject);
    }

    private async Task<ProjectInfo?> AnalyzeProjectAsync(Project project)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var filePath = project.FullPath;
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return null;

        var info = new ProjectInfo
        {
            Name = project.Name ?? Path.GetFileNameWithoutExtension(filePath),
            FilePath = filePath
        };

        try
        {
            var content = await File.ReadAllTextAsync(filePath);

            // Check for WPF
            info.IsWpfApplication = content.Contains("<UseWPF>true</UseWPF>") ||
                                   content.Contains("WindowsDesktop.App.WPF");

            // Get target framework
            var tfMatch = System.Text.RegularExpressions.Regex.Match(content,
                @"<TargetFramework>([^<]+)</TargetFramework>");
            if (tfMatch.Success)
            {
                info.TargetFramework = tfMatch.Groups[1].Value;
            }

            // Check for recorder reference
            info.HasRecorderReference = content.Contains("WpfEventRecorder");

            // Get output path
            var projectDir = Path.GetDirectoryName(filePath);
            info.OutputPath = Path.Combine(projectDir!, "bin", "Debug", info.TargetFramework ?? "net8.0-windows");
        }
        catch
        {
            // Ignore parsing errors
        }

        return info;
    }
}
