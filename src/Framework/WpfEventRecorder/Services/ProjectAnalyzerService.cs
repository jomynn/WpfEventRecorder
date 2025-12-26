using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;

namespace WpfEventRecorder.Services
{
    /// <summary>
    /// Information about a ViewModel in the project
    /// </summary>
    public class ViewModelInfo
    {
        /// <summary>
        /// Full path to the file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Namespace
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Base class if any
        /// </summary>
        public string BaseClass { get; set; }

        /// <summary>
        /// Properties found in the ViewModel
        /// </summary>
        public List<string> Properties { get; set; } = new List<string>();

        /// <summary>
        /// Commands found in the ViewModel
        /// </summary>
        public List<string> Commands { get; set; } = new List<string>();
    }

    /// <summary>
    /// Information about a View in the project
    /// </summary>
    public class ViewInfo
    {
        /// <summary>
        /// Full path to the XAML file
        /// </summary>
        public string XamlPath { get; set; }

        /// <summary>
        /// Full path to the code-behind file
        /// </summary>
        public string CodeBehindPath { get; set; }

        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Namespace
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// DataContext type if set in XAML
        /// </summary>
        public string DataContextType { get; set; }

        /// <summary>
        /// Named controls found in the View
        /// </summary>
        public List<string> NamedControls { get; set; } = new List<string>();
    }

    /// <summary>
    /// Information about HttpClient usage
    /// </summary>
    public class HttpClientUsageInfo
    {
        /// <summary>
        /// Full path to the file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Line number where HttpClient is used
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Type of usage (new HttpClient, field, property, etc.)
        /// </summary>
        public string UsageType { get; set; }

        /// <summary>
        /// The code snippet
        /// </summary>
        public string CodeSnippet { get; set; }
    }

    /// <summary>
    /// Service for analyzing WPF projects to find ViewModels, Views, and HttpClient usages
    /// </summary>
    public class ProjectAnalyzerService
    {
        private readonly AsyncPackage _package;

        /// <summary>
        /// Creates a new project analyzer
        /// </summary>
        public ProjectAnalyzerService(AsyncPackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Gets the current solution
        /// </summary>
        public async Task<Solution> GetSolutionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await _package.GetServiceAsync(typeof(DTE)) as DTE2;
            return dte?.Solution;
        }

        /// <summary>
        /// Gets the startup project
        /// </summary>
        public async Task<Project> GetStartupProjectAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await _package.GetServiceAsync(typeof(DTE)) as DTE2;
            if (dte?.Solution == null) return null;

            var startupProjects = dte.Solution.SolutionBuild.StartupProjects as Array;
            if (startupProjects == null || startupProjects.Length == 0) return null;

            var startupProjectName = startupProjects.GetValue(0) as string;
            if (string.IsNullOrEmpty(startupProjectName)) return null;

            foreach (Project project in dte.Solution.Projects)
            {
                if (project.UniqueName == startupProjectName)
                {
                    return project;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds all ViewModels in the project
        /// </summary>
        public async Task<List<ViewModelInfo>> FindViewModelsAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var viewModels = new List<ViewModelInfo>();

            if (project?.ProjectItems == null) return viewModels;

            await ScanProjectItemsForViewModelsAsync(project.ProjectItems, viewModels);

            return viewModels;
        }

        /// <summary>
        /// Finds all Views in the project
        /// </summary>
        public async Task<List<ViewInfo>> FindViewsAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var views = new List<ViewInfo>();

            if (project?.ProjectItems == null) return views;

            await ScanProjectItemsForViewsAsync(project.ProjectItems, views);

            return views;
        }

        /// <summary>
        /// Finds all HttpClient usages in the project
        /// </summary>
        public async Task<List<HttpClientUsageInfo>> FindHttpClientUsagesAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var usages = new List<HttpClientUsageInfo>();

            if (project?.ProjectItems == null) return usages;

            await ScanProjectItemsForHttpClientAsync(project.ProjectItems, usages);

            return usages;
        }

        /// <summary>
        /// Finds App.xaml and App.xaml.cs
        /// </summary>
        public async Task<(string AppXamlPath, string AppXamlCsPath)> FindAppFilesAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (project?.ProjectItems == null) return (null, null);

            foreach (ProjectItem item in project.ProjectItems)
            {
                var name = item.Name;
                if (name.Equals("App.xaml", StringComparison.OrdinalIgnoreCase))
                {
                    var xamlPath = item.FileNames[1];
                    var csPath = xamlPath + ".cs";

                    if (File.Exists(csPath))
                    {
                        return (xamlPath, csPath);
                    }
                }
            }

            return (null, null);
        }

        private async Task ScanProjectItemsForViewModelsAsync(ProjectItems items, List<ViewModelInfo> viewModels)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (ProjectItem item in items)
            {
                var name = item.Name;

                // Check if it's a C# file that might be a ViewModel
                if (name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = item.FileNames[1];

                    // Check if filename contains "ViewModel"
                    if (name.Contains("ViewModel", StringComparison.OrdinalIgnoreCase))
                    {
                        var vmInfo = await AnalyzeViewModelFileAsync(filePath);
                        if (vmInfo != null)
                        {
                            viewModels.Add(vmInfo);
                        }
                    }
                }

                // Recurse into sub-items
                if (item.ProjectItems?.Count > 0)
                {
                    await ScanProjectItemsForViewModelsAsync(item.ProjectItems, viewModels);
                }
            }
        }

        private async Task ScanProjectItemsForViewsAsync(ProjectItems items, List<ViewInfo> views)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (ProjectItem item in items)
            {
                var name = item.Name;

                // Check if it's a XAML file
                if (name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) &&
                    !name.Equals("App.xaml", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = item.FileNames[1];
                    var viewInfo = await AnalyzeViewFileAsync(filePath);
                    if (viewInfo != null)
                    {
                        views.Add(viewInfo);
                    }
                }

                // Recurse into sub-items
                if (item.ProjectItems?.Count > 0)
                {
                    await ScanProjectItemsForViewsAsync(item.ProjectItems, views);
                }
            }
        }

        private async Task ScanProjectItemsForHttpClientAsync(ProjectItems items, List<HttpClientUsageInfo> usages)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (ProjectItem item in items)
            {
                var name = item.Name;

                // Check if it's a C# file
                if (name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = item.FileNames[1];
                    var fileUsages = await AnalyzeFileForHttpClientAsync(filePath);
                    usages.AddRange(fileUsages);
                }

                // Recurse into sub-items
                if (item.ProjectItems?.Count > 0)
                {
                    await ScanProjectItemsForHttpClientAsync(item.ProjectItems, usages);
                }
            }
        }

        private Task<ViewModelInfo> AnalyzeViewModelFileAsync(string filePath)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(filePath)) return null;

                var content = File.ReadAllText(filePath);
                var vmInfo = new ViewModelInfo { FilePath = filePath };

                // Simple regex-based parsing (for production, use Roslyn)
                var lines = content.Split('\n');
                string currentNamespace = null;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    // Find namespace
                    if (trimmed.StartsWith("namespace "))
                    {
                        currentNamespace = trimmed.Substring(10).TrimEnd('{', ' ');
                    }

                    // Find class declaration
                    if (trimmed.Contains("class ") && trimmed.Contains("ViewModel"))
                    {
                        var classMatch = System.Text.RegularExpressions.Regex.Match(
                            trimmed, @"class\s+(\w+)(?:\s*:\s*(\w+))?");
                        if (classMatch.Success)
                        {
                            vmInfo.ClassName = classMatch.Groups[1].Value;
                            vmInfo.BaseClass = classMatch.Groups[2].Value;
                            vmInfo.Namespace = currentNamespace;
                        }
                    }

                    // Find properties
                    var propMatch = System.Text.RegularExpressions.Regex.Match(
                        trimmed, @"public\s+\w+\s+(\w+)\s*\{");
                    if (propMatch.Success)
                    {
                        vmInfo.Properties.Add(propMatch.Groups[1].Value);
                    }

                    // Find commands
                    if (trimmed.Contains("ICommand") || trimmed.Contains("Command"))
                    {
                        var cmdMatch = System.Text.RegularExpressions.Regex.Match(
                            trimmed, @"(\w+Command)\s*\{");
                        if (cmdMatch.Success)
                        {
                            vmInfo.Commands.Add(cmdMatch.Groups[1].Value);
                        }
                    }
                }

                return string.IsNullOrEmpty(vmInfo.ClassName) ? null : vmInfo;
            });
        }

        private Task<ViewInfo> AnalyzeViewFileAsync(string filePath)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(filePath)) return null;

                try
                {
                    var viewInfo = new ViewInfo
                    {
                        XamlPath = filePath,
                        CodeBehindPath = filePath + ".cs"
                    };

                    var doc = XDocument.Load(filePath);
                    var root = doc.Root;
                    if (root == null) return null;

                    // Get class name from x:Class attribute
                    var xNs = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");
                    var classAttr = root.Attribute(xNs + "Class");
                    if (classAttr != null)
                    {
                        var fullName = classAttr.Value;
                        var lastDot = fullName.LastIndexOf('.');
                        if (lastDot > 0)
                        {
                            viewInfo.Namespace = fullName.Substring(0, lastDot);
                            viewInfo.ClassName = fullName.Substring(lastDot + 1);
                        }
                        else
                        {
                            viewInfo.ClassName = fullName;
                        }
                    }

                    // Find DataContext
                    var dataContextAttr = root.Attribute("DataContext");
                    if (dataContextAttr != null)
                    {
                        viewInfo.DataContextType = dataContextAttr.Value;
                    }

                    // Find named controls
                    foreach (var element in root.DescendantsAndSelf())
                    {
                        var nameAttr = element.Attribute(xNs + "Name") ?? element.Attribute("Name");
                        if (nameAttr != null)
                        {
                            viewInfo.NamedControls.Add($"{element.Name.LocalName}:{nameAttr.Value}");
                        }
                    }

                    return viewInfo;
                }
                catch
                {
                    return null;
                }
            });
        }

        private Task<List<HttpClientUsageInfo>> AnalyzeFileForHttpClientAsync(string filePath)
        {
            return Task.Run(() =>
            {
                var usages = new List<HttpClientUsageInfo>();

                if (!File.Exists(filePath)) return usages;

                var lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (line.Contains("HttpClient"))
                    {
                        var usage = new HttpClientUsageInfo
                        {
                            FilePath = filePath,
                            LineNumber = i + 1,
                            CodeSnippet = line.Trim()
                        };

                        if (line.Contains("new HttpClient"))
                        {
                            usage.UsageType = "Instantiation";
                        }
                        else if (line.Contains("HttpClient ") && line.Contains("{"))
                        {
                            usage.UsageType = "Property";
                        }
                        else if (line.Contains("HttpClient ") && line.Contains(";"))
                        {
                            usage.UsageType = "Field";
                        }
                        else
                        {
                            usage.UsageType = "Reference";
                        }

                        usages.Add(usage);
                    }
                }

                return usages;
            });
        }
    }
}
