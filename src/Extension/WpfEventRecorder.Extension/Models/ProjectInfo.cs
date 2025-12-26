namespace WpfEventRecorder.Extension.Models;

/// <summary>
/// Information about a WPF project in the solution.
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// Project name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Full path to the project file.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Target framework (e.g., "net8.0-windows").
    /// </summary>
    public string? TargetFramework { get; set; }

    /// <summary>
    /// Whether this is a WPF application project.
    /// </summary>
    public bool IsWpfApplication { get; set; }

    /// <summary>
    /// Output assembly path.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// List of ViewModel files in the project.
    /// </summary>
    public List<string> ViewModelFiles { get; set; } = new();

    /// <summary>
    /// List of View files in the project.
    /// </summary>
    public List<string> ViewFiles { get; set; } = new();

    /// <summary>
    /// Whether the project references WpfEventRecorder.Core.
    /// </summary>
    public bool HasRecorderReference { get; set; }
}
