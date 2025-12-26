using WpfEventRecorder.Extension.Models;

namespace WpfEventRecorder.Extension.Services;

/// <summary>
/// Service for injecting recording code into target projects.
/// </summary>
public interface ICodeInjectionService
{
    /// <summary>
    /// Adds the recorder NuGet package to a project.
    /// </summary>
    Task<bool> AddRecorderPackageAsync(ProjectInfo project);

    /// <summary>
    /// Injects initialization code into App.xaml.cs.
    /// </summary>
    Task<bool> InjectInitializationCodeAsync(ProjectInfo project);

    /// <summary>
    /// Wraps HttpClient usages with recording handler.
    /// </summary>
    Task<int> WrapHttpClientUsagesAsync(ProjectInfo project);

    /// <summary>
    /// Adds recording attributes to ViewModels.
    /// </summary>
    Task<int> AnnotateViewModelsAsync(ProjectInfo project);
}
