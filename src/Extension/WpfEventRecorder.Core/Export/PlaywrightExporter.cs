using System.Text;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Export;

/// <summary>
/// Exports events to Playwright test format (TypeScript).
/// Note: This generates a web-testing style format that can be adapted for WPF automation.
/// </summary>
public class PlaywrightExporter : ExporterBase
{
    public override string FormatName => "Playwright";
    public override string FileExtension => ".ts";

    public override string Export(IEnumerable<RecordedEvent> events, RecordingSession? session = null)
    {
        var sb = new StringBuilder();
        var eventList = events.ToList();
        var testName = ToIdentifier(session?.Name ?? "RecordedTest");

        sb.AppendLine("import { test, expect } from '@playwright/test';");
        sb.AppendLine();
        sb.AppendLine("/**");
        sb.AppendLine(" * Auto-generated test from WPF Event Recorder session.");
        if (session != null)
        {
            sb.AppendLine($" * Session: {session.Name}");
            sb.AppendLine($" * Recorded: {session.StartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($" * Events: {eventList.Count}");
        }
        sb.AppendLine(" * ");
        sb.AppendLine(" * Note: This test is designed for web-based UI testing.");
        sb.AppendLine(" * Adapt selectors and actions for your specific application.");
        sb.AppendLine(" */");
        sb.AppendLine();
        sb.AppendLine($"test.describe('{testName}', () => {{");
        sb.AppendLine();
        sb.AppendLine($"  test('replay recorded events', async ({{ page }}) => {{");
        sb.AppendLine("    // Navigate to the application");
        sb.AppendLine("    await page.goto('http://localhost:3000');");
        sb.AppendLine();
        sb.AppendLine("    // Replay recorded events");

        foreach (var @event in eventList)
        {
            var action = GenerateEventAction(@event);
            if (!string.IsNullOrEmpty(action))
            {
                sb.AppendLine($"    {action}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("    // Verify final state");
        sb.AppendLine("    // TODO: Add appropriate assertions");
        sb.AppendLine("  });");
        sb.AppendLine("});");

        return sb.ToString();
    }

    private static string GenerateEventAction(RecordedEvent @event)
    {
        return @event switch
        {
            InputEvent ie => GenerateInputAction(ie),
            CommandEvent ce => GenerateCommandAction(ce),
            NavigationEvent ne => GenerateNavigationAction(ne),
            ApiCallEvent ae => GenerateApiComment(ae),
            WindowEvent we => GenerateWindowComment(we),
            _ => $"// Unknown: {@event.GetDescription()}"
        };
    }

    private static string GenerateInputAction(InputEvent ie)
    {
        var selector = GetSelector(ie);

        return ie.InputType switch
        {
            InputEventType.TextChanged =>
                $"await page.locator({selector}).fill('{SanitizeForJs(ie.NewValue?.ToString())}');",
            InputEventType.SelectionChanged =>
                $"await page.locator({selector}).selectOption('{SanitizeForJs(ie.NewValue?.ToString())}');",
            InputEventType.CheckedChanged =>
                ie.NewValue is true
                    ? $"await page.locator({selector}).check();"
                    : $"await page.locator({selector}).uncheck();",
            InputEventType.ButtonClicked =>
                $"await page.locator({selector}).click();",
            InputEventType.SliderChanged =>
                $"// Set slider {selector} to {ie.NewValue}",
            InputEventType.DateChanged =>
                $"await page.locator({selector}).fill('{ie.NewValue}');",
            _ => $"// {ie.InputType}: {ie.GetDescription()}"
        };
    }

    private static string GenerateCommandAction(CommandEvent ce)
    {
        return $"// Execute command: {ce.CommandName}" +
               (ce.CommandParameter != null ? $" with parameter: {ce.CommandParameter}" : "");
    }

    private static string GenerateNavigationAction(NavigationEvent ne)
    {
        return ne.NavigationType switch
        {
            NavigationType.TabChanged =>
                $"await page.locator('text=\"{SanitizeForJs(ne.TabHeader)}\"').click();",
            NavigationType.ViewNavigation =>
                $"// Navigate to: {ne.ToView}",
            _ => $"// Navigation: {ne.GetDescription()}"
        };
    }

    private static string GenerateApiComment(ApiCallEvent ae)
    {
        return $"// API: {ae.HttpMethod} {ae.RequestUrl} -> {ae.StatusCode} ({ae.DurationMs}ms)";
    }

    private static string GenerateWindowComment(WindowEvent we)
    {
        return $"// Window {we.WindowEventType}: {we.WindowTitle}";
    }

    private static string GetSelector(RecordedEvent @event)
    {
        if (!string.IsNullOrEmpty(@event.AutomationId))
            return $"'[data-testid=\"{@event.AutomationId}\"]'";
        if (!string.IsNullOrEmpty(@event.SourceElementName))
            return $"'#{@event.SourceElementName}'";
        return "'[unknown-element]'";
    }

    private static string SanitizeForJs(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}
