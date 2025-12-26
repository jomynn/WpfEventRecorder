using System.Text;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Export;

/// <summary>
/// Exports events to NUnit test format.
/// </summary>
public class NUnitExporter : ExporterBase
{
    public override string FormatName => "NUnit";
    public override string FileExtension => ".cs";

    public override string Export(IEnumerable<RecordedEvent> events, RecordingSession? session = null)
    {
        var sb = new StringBuilder();
        var eventList = events.ToList();
        var className = ToIdentifier(session?.Name ?? "RecordedTest");

        sb.AppendLine("using NUnit.Framework;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine("namespace WpfEventRecorder.GeneratedTests;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Auto-generated test from WPF Event Recorder session.");
        if (session != null)
        {
            sb.AppendLine($"/// Session: {session.Name}");
            sb.AppendLine($"/// Recorded: {session.StartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"/// Events: {eventList.Count}");
        }
        sb.AppendLine($"/// </summary>");
        sb.AppendLine("[TestFixture]");
        sb.AppendLine($"public class {className}Tests");
        sb.AppendLine("{");
        sb.AppendLine("    private IAsyncDisposable? _app;");
        sb.AppendLine();
        sb.AppendLine("    [SetUp]");
        sb.AppendLine("    public async Task SetUp()");
        sb.AppendLine("    {");
        sb.AppendLine("        _app = await StartApplicationAsync();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [TearDown]");
        sb.AppendLine("    public async Task TearDown()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_app != null)");
        sb.AppendLine("        {");
        sb.AppendLine("            await _app.DisposeAsync();");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate main test method
        sb.AppendLine("    [Test]");
        sb.AppendLine($"    public async Task {className}_ReplayRecordedEvents()");
        sb.AppendLine("    {");
        sb.AppendLine("        // Replay recorded events");

        int index = 0;
        foreach (var @event in eventList)
        {
            var action = GenerateEventAction(@event, index++);
            if (!string.IsNullOrEmpty(action))
            {
                sb.AppendLine($"        {action}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("        // Assert");
        sb.AppendLine("        Assert.Pass(\"All events replayed successfully\");");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate helper methods
        sb.AppendLine("    private static async Task<IAsyncDisposable> StartApplicationAsync()");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Implement application startup");
        sb.AppendLine("        throw new NotImplementedException();");
        sb.AppendLine("    }");
        sb.AppendLine();

        GenerateHelperMethods(sb);

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateEventAction(RecordedEvent @event, int index)
    {
        return @event switch
        {
            InputEvent ie => GenerateInputAction(ie),
            CommandEvent ce => $"await ExecuteCommandAsync(\"{SanitizeForCode(ce.CommandName)}\");",
            NavigationEvent ne when ne.NavigationType == NavigationType.TabChanged =>
                $"await SelectTabAsync(\"{SanitizeForCode(ne.TabHeader)}\");",
            _ => $"// {index}: {@event.GetDescription()}"
        };
    }

    private static string GenerateInputAction(InputEvent ie)
    {
        var target = !string.IsNullOrEmpty(ie.AutomationId) ? ie.AutomationId :
                     !string.IsNullOrEmpty(ie.SourceElementName) ? ie.SourceElementName : "Unknown";

        return ie.InputType switch
        {
            InputEventType.TextChanged =>
                $"await EnterTextAsync(\"{target}\", \"{SanitizeForCode(ie.NewValue?.ToString())}\");",
            InputEventType.SelectionChanged =>
                $"await SelectItemAsync(\"{target}\", \"{SanitizeForCode(ie.NewValue?.ToString())}\");",
            InputEventType.CheckedChanged =>
                $"await SetCheckboxAsync(\"{target}\", {ie.NewValue?.ToString()?.ToLower() ?? "false"});",
            InputEventType.ButtonClicked =>
                $"await ClickButtonAsync(\"{target}\");",
            _ => $"// {ie.InputType}: {ie.GetDescription()}"
        };
    }

    private static void GenerateHelperMethods(StringBuilder sb)
    {
        sb.AppendLine("    private static Task EnterTextAsync(string elementId, string text) => Task.CompletedTask;");
        sb.AppendLine("    private static Task ClickButtonAsync(string elementId) => Task.CompletedTask;");
        sb.AppendLine("    private static Task SelectItemAsync(string elementId, string item) => Task.CompletedTask;");
        sb.AppendLine("    private static Task SetCheckboxAsync(string elementId, bool isChecked) => Task.CompletedTask;");
        sb.AppendLine("    private static Task SelectTabAsync(string tabHeader) => Task.CompletedTask;");
        sb.AppendLine("    private static Task ExecuteCommandAsync(string commandName) => Task.CompletedTask;");
    }
}
