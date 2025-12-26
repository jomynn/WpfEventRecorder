using System.Text;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Export;

/// <summary>
/// Exports events to xUnit test format.
/// </summary>
public class XUnitExporter : ExporterBase
{
    public override string FormatName => "xUnit";
    public override string FileExtension => ".cs";

    public override string Export(IEnumerable<RecordedEvent> events, RecordingSession? session = null)
    {
        var sb = new StringBuilder();
        var eventList = events.ToList();
        var className = ToIdentifier(session?.Name ?? "RecordedTest");

        sb.AppendLine("using Xunit;");
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
        sb.AppendLine($"public class {className}Tests");
        sb.AppendLine("{");

        // Generate main test method
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public async Task {className}_ReplayRecordedEvents()");
        sb.AppendLine("    {");
        sb.AppendLine("        // Arrange");
        sb.AppendLine("        var app = await StartApplicationAsync();");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine("            // Act - Replay recorded events");

        int index = 0;
        foreach (var @event in eventList)
        {
            var action = GenerateEventAction(@event, index++);
            if (!string.IsNullOrEmpty(action))
            {
                sb.AppendLine($"            {action}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("            // Assert");
        sb.AppendLine("            // TODO: Add assertions based on expected state");
        sb.AppendLine("        }");
        sb.AppendLine("        finally");
        sb.AppendLine("        {");
        sb.AppendLine("            await app.DisposeAsync();");
        sb.AppendLine("        }");
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
            CommandEvent ce => GenerateCommandAction(ce),
            ApiCallEvent ae => GenerateApiAction(ae),
            NavigationEvent ne => GenerateNavigationAction(ne),
            WindowEvent we => GenerateWindowAction(we),
            _ => $"// Unknown event type: {@event.EventType}"
        };
    }

    private static string GenerateInputAction(InputEvent ie)
    {
        var target = GetTarget(ie);

        return ie.InputType switch
        {
            InputEventType.TextChanged =>
                $"await EnterTextAsync({target}, \"{SanitizeForCode(ie.NewValue?.ToString())}\");",
            InputEventType.SelectionChanged =>
                $"await SelectItemAsync({target}, \"{SanitizeForCode(ie.NewValue?.ToString())}\");",
            InputEventType.CheckedChanged =>
                $"await SetCheckboxAsync({target}, {ie.NewValue?.ToString()?.ToLower() ?? "false"});",
            InputEventType.ButtonClicked =>
                $"await ClickButtonAsync({target});",
            InputEventType.SliderChanged =>
                $"await SetSliderAsync({target}, {ie.NewValue});",
            InputEventType.DateChanged =>
                $"await SetDateAsync({target}, DateTime.Parse(\"{ie.NewValue}\"));",
            _ => $"// {ie.InputType}: {ie.GetDescription()}"
        };
    }

    private static string GenerateCommandAction(CommandEvent ce)
    {
        var param = ce.CommandParameter != null
            ? $", \"{SanitizeForCode(ce.CommandParameter.ToString())}\""
            : "";
        return $"await ExecuteCommandAsync(\"{SanitizeForCode(ce.CommandName)}\"{param});";
    }

    private static string GenerateApiAction(ApiCallEvent ae)
    {
        return $"// API: {ae.HttpMethod} {ae.RequestUrl} → {ae.StatusCode} ({ae.DurationMs}ms)";
    }

    private static string GenerateNavigationAction(NavigationEvent ne)
    {
        return ne.NavigationType switch
        {
            NavigationType.TabChanged =>
                $"await SelectTabAsync(\"{SanitizeForCode(ne.TabHeader)}\");",
            NavigationType.ViewNavigation =>
                $"await NavigateToAsync(\"{SanitizeForCode(ne.ToView)}\");",
            _ => $"// Navigation: {ne.GetDescription()}"
        };
    }

    private static string GenerateWindowAction(WindowEvent we)
    {
        return we.WindowEventType switch
        {
            WindowEventType.Opened => $"// Window opened: {we.WindowTitle}",
            WindowEventType.Closed => $"// Window closed: {we.WindowTitle}",
            _ => $"// Window: {we.GetDescription()}"
        };
    }

    private static string GetTarget(RecordedEvent @event)
    {
        if (!string.IsNullOrEmpty(@event.AutomationId))
            return $"\"{@event.AutomationId}\"";
        if (!string.IsNullOrEmpty(@event.SourceElementName))
            return $"\"{@event.SourceElementName}\"";
        return "\"Unknown\"";
    }

    private static void GenerateHelperMethods(StringBuilder sb)
    {
        sb.AppendLine("    private static async Task EnterTextAsync(string elementId, string text)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Find element and enter text");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static async Task ClickButtonAsync(string elementId)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Find element and click");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static async Task SelectItemAsync(string elementId, string item)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Find element and select item");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static async Task SetCheckboxAsync(string elementId, bool isChecked)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Find checkbox and set state");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static async Task SetSliderAsync(string elementId, object value)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Find slider and set value");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static async Task SetDateAsync(string elementId, DateTime date)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Find date picker and set date");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static async Task SelectTabAsync(string tabHeader)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Find tab and select");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static async Task NavigateToAsync(string viewName)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Navigate to view");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static async Task ExecuteCommandAsync(string commandName, string? parameter = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: Execute command");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
    }
}
