using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Service for analyzing WPF projects in the solution.
/// </summary>
public interface IProjectAnalyzerService
{
    /// <summary>
    /// Gets all WPF projects in the current solution.
    /// </summary>
    Task<IEnumerable<ProjectInfo>> GetWpfProjectsAsync();

    /// <summary>
    /// Gets ViewModels in a project.
    /// </summary>
    Task<IEnumerable<string>> GetViewModelsAsync(ProjectInfo project);

    /// <summary>
    /// Gets Views in a project.
    /// </summary>
    Task<IEnumerable<string>> GetViewsAsync(ProjectInfo project);

    /// <summary>
    /// Checks if a project has the recorder NuGet package.
    /// </summary>
    Task<bool> HasRecorderPackageAsync(ProjectInfo project);

    /// <summary>
    /// Gets the startup project.
    /// </summary>
    Task<ProjectInfo?> GetStartupProjectAsync();
}
