using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Services
{
    /// <summary>
    /// Export format enumeration
    /// </summary>
    public enum ExportFormat
    {
        Json,
        Csv,
        Excel,
        MSTest,
        NUnit,
        XUnit,
        Playwright
    }

    /// <summary>
    /// Service for exporting recorded events to various formats
    /// </summary>
    public static class ExportService
    {
        /// <summary>
        /// Exports entries to the specified format
        /// </summary>
        public static void Export(IEnumerable<RecordEntry> entries, string filePath, ExportFormat format,
            RecordingSession session = null)
        {
            switch (format)
            {
                case ExportFormat.Json:
                    ExportToJson(entries, filePath, session);
                    break;
                case ExportFormat.Csv:
                    ExportToCsv(entries, filePath);
                    break;
                case ExportFormat.Excel:
                    ExportToExcel(entries, filePath);
                    break;
                case ExportFormat.MSTest:
                    ExportToMSTest(entries, filePath, session);
                    break;
                case ExportFormat.NUnit:
                    ExportToNUnit(entries, filePath, session);
                    break;
                case ExportFormat.XUnit:
                    ExportToXUnit(entries, filePath, session);
                    break;
                case ExportFormat.Playwright:
                    ExportToPlaywright(entries, filePath, session);
                    break;
                default:
                    throw new ArgumentException($"Unsupported format: {format}");
            }
        }

        /// <summary>
        /// Exports entries to JSON format
        /// </summary>
        public static void ExportToJson(IEnumerable<RecordEntry> entries, string filePath,
            RecordingSession session = null)
        {
            var exportSession = session ?? new RecordingSession
            {
                SessionId = Guid.NewGuid().ToString(),
                SessionName = "Export",
                StartTime = entries.FirstOrDefault()?.Timestamp ?? DateTime.UtcNow,
                EndTime = entries.LastOrDefault()?.Timestamp ?? DateTime.UtcNow,
                Entries = entries.ToList()
            };

            if (exportSession.Entries == null || exportSession.Entries.Count == 0)
            {
                exportSession.Entries = entries.ToList();
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var json = JsonSerializer.Serialize(exportSession, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// Exports entries to MSTest format
        /// </summary>
        public static void ExportToMSTest(IEnumerable<RecordEntry> entries, string filePath,
            RecordingSession session = null)
        {
            var entryList = entries.ToList();
            var sessionName = session?.SessionName ?? "Recording";
            var className = SanitizeIdentifier(sessionName) + "Tests";
            var testName = $"Test_{SanitizeIdentifier(sessionName)}_{DateTime.Now:yyyyMMdd_HHmmss}";

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
            sb.AppendLine();
            sb.AppendLine("namespace RecordedTests");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Tests generated from WPF Event Recorder session: {sessionName}");
            sb.AppendLine($"    /// Recorded on: {session?.StartTime ?? DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    [TestClass]");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            sb.AppendLine("        private HttpClient _httpClient;");
            sb.AppendLine();
            sb.AppendLine("        [TestInitialize]");
            sb.AppendLine("        public void Setup()");
            sb.AppendLine("        {");
            sb.AppendLine("            _httpClient = new HttpClient();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [TestCleanup]");
            sb.AppendLine("        public void Cleanup()");
            sb.AppendLine("        {");
            sb.AppendLine("            _httpClient?.Dispose();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [TestMethod]");
            sb.AppendLine($"        public async Task {testName}()");
            sb.AppendLine("        {");
            sb.AppendLine("            // Arrange");
            sb.AppendLine("            // TODO: Initialize your ViewModels and services here");
            sb.AppendLine();
            sb.AppendLine("            // Act - Recorded UI Interactions");

            GenerateTestBody(sb, entryList, "            ");

            sb.AppendLine();
            sb.AppendLine("            // Assert");
            sb.AppendLine("            // TODO: Add your assertions here");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Exports entries to NUnit format
        /// </summary>
        public static void ExportToNUnit(IEnumerable<RecordEntry> entries, string filePath,
            RecordingSession session = null)
        {
            var entryList = entries.ToList();
            var sessionName = session?.SessionName ?? "Recording";
            var className = SanitizeIdentifier(sessionName) + "Tests";
            var testName = $"Test_{SanitizeIdentifier(sessionName)}_{DateTime.Now:yyyyMMdd_HHmmss}";

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using NUnit.Framework;");
            sb.AppendLine();
            sb.AppendLine("namespace RecordedTests");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Tests generated from WPF Event Recorder session: {sessionName}");
            sb.AppendLine($"    /// Recorded on: {session?.StartTime ?? DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    [TestFixture]");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            sb.AppendLine("        private HttpClient _httpClient;");
            sb.AppendLine();
            sb.AppendLine("        [SetUp]");
            sb.AppendLine("        public void Setup()");
            sb.AppendLine("        {");
            sb.AppendLine("            _httpClient = new HttpClient();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [TearDown]");
            sb.AppendLine("        public void TearDown()");
            sb.AppendLine("        {");
            sb.AppendLine("            _httpClient?.Dispose();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [Test]");
            sb.AppendLine($"        public async Task {testName}()");
            sb.AppendLine("        {");
            sb.AppendLine("            // Arrange");
            sb.AppendLine("            // TODO: Initialize your ViewModels and services here");
            sb.AppendLine();
            sb.AppendLine("            // Act - Recorded UI Interactions");

            GenerateTestBody(sb, entryList, "            ");

            sb.AppendLine();
            sb.AppendLine("            // Assert");
            sb.AppendLine("            // TODO: Add your assertions here");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Exports entries to xUnit format
        /// </summary>
        public static void ExportToXUnit(IEnumerable<RecordEntry> entries, string filePath,
            RecordingSession session = null)
        {
            var entryList = entries.ToList();
            var sessionName = session?.SessionName ?? "Recording";
            var className = SanitizeIdentifier(sessionName) + "Tests";
            var testName = $"Test_{SanitizeIdentifier(sessionName)}_{DateTime.Now:yyyyMMdd_HHmmss}";

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Xunit;");
            sb.AppendLine();
            sb.AppendLine("namespace RecordedTests");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Tests generated from WPF Event Recorder session: {sessionName}");
            sb.AppendLine($"    /// Recorded on: {session?.StartTime ?? DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public class {className} : IDisposable");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly HttpClient _httpClient;");
            sb.AppendLine();
            sb.AppendLine($"        public {className}()");
            sb.AppendLine("        {");
            sb.AppendLine("            _httpClient = new HttpClient();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public void Dispose()");
            sb.AppendLine("        {");
            sb.AppendLine("            _httpClient?.Dispose();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [Fact]");
            sb.AppendLine($"        public async Task {testName}()");
            sb.AppendLine("        {");
            sb.AppendLine("            // Arrange");
            sb.AppendLine("            // TODO: Initialize your ViewModels and services here");
            sb.AppendLine();
            sb.AppendLine("            // Act - Recorded UI Interactions");

            GenerateTestBody(sb, entryList, "            ");

            sb.AppendLine();
            sb.AppendLine("            // Assert");
            sb.AppendLine("            // TODO: Add your assertions here");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Exports entries to Playwright-style format
        /// </summary>
        public static void ExportToPlaywright(IEnumerable<RecordEntry> entries, string filePath,
            RecordingSession session = null)
        {
            var entryList = entries.ToList();
            var sessionName = session?.SessionName ?? "Recording";
            var className = SanitizeIdentifier(sessionName) + "Tests";
            var testName = $"Test_{SanitizeIdentifier(sessionName)}";

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Microsoft.Playwright;");
            sb.AppendLine("using Xunit;");
            sb.AppendLine();
            sb.AppendLine("namespace RecordedTests");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Playwright-style tests generated from WPF Event Recorder session: {sessionName}");
            sb.AppendLine($"    /// Recorded on: {session?.StartTime ?? DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("    /// Note: These are conceptual Playwright-style assertions for WPF automation");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            sb.AppendLine("        [Fact]");
            sb.AppendLine($"        public async Task {testName}()");
            sb.AppendLine("        {");
            sb.AppendLine("            // Note: This is a Playwright-style representation of the recorded WPF interactions");
            sb.AppendLine("            // For actual WPF automation, consider using FlaUI or similar libraries");
            sb.AppendLine();

            foreach (var entry in entryList)
            {
                GeneratePlaywrightAction(sb, entry, "            ");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static void GenerateTestBody(StringBuilder sb, List<RecordEntry> entries, string indent)
        {
            foreach (var entry in entries)
            {
                switch (entry.EntryType)
                {
                    case RecordEntryType.UITextInput:
                        var textTarget = GetTargetSelector(entry.UIInfo);
                        sb.AppendLine($"{indent}// Text input: {textTarget}");
                        if (!string.IsNullOrEmpty(entry.UIInfo?.VisualTreePath))
                        {
                            sb.AppendLine($"{indent}// Binding: {entry.UIInfo.VisualTreePath}");
                            var propName = ExtractPropertyName(entry.UIInfo.VisualTreePath);
                            if (!string.IsNullOrEmpty(propName))
                            {
                                sb.AppendLine($"{indent}// vm.{propName} = \"{EscapeString(entry.UIInfo?.NewValue)}\";");
                            }
                        }
                        break;

                    case RecordEntryType.UISelectionChange:
                        var selTarget = GetTargetSelector(entry.UIInfo);
                        sb.AppendLine($"{indent}// Selection changed: {selTarget} -> \"{EscapeString(entry.UIInfo?.NewValue)}\"");
                        break;

                    case RecordEntryType.UIClick:
                        var clickTarget = GetTargetSelector(entry.UIInfo);
                        sb.AppendLine($"{indent}// Click: {clickTarget}");
                        if (entry.UIInfo?.ControlType == "Command")
                        {
                            sb.AppendLine($"{indent}// vm.{entry.UIInfo?.ControlName}Command.Execute(null);");
                        }
                        break;

                    case RecordEntryType.UIToggle:
                        var toggleTarget = GetTargetSelector(entry.UIInfo);
                        sb.AppendLine($"{indent}// Toggle: {toggleTarget} = {entry.UIInfo?.NewValue}");
                        break;

                    case RecordEntryType.ApiRequest:
                        sb.AppendLine($"{indent}// API {entry.ApiInfo?.Method} {entry.ApiInfo?.Url}");
                        if (!string.IsNullOrEmpty(entry.ApiInfo?.RequestBody))
                        {
                            sb.AppendLine($"{indent}// Request: {TruncateString(entry.ApiInfo.RequestBody, 100)}");
                        }
                        break;

                    case RecordEntryType.ApiResponse:
                        sb.AppendLine($"{indent}// API Response: {entry.ApiInfo?.StatusCode} ({entry.DurationMs}ms)");
                        if (!string.IsNullOrEmpty(entry.ApiInfo?.ResponseBody))
                        {
                            sb.AppendLine($"{indent}// Response: {TruncateString(entry.ApiInfo.ResponseBody, 100)}");
                        }
                        break;
                }
            }
        }

        private static void GeneratePlaywrightAction(StringBuilder sb, RecordEntry entry, string indent)
        {
            switch (entry.EntryType)
            {
                case RecordEntryType.UITextInput:
                    var textSelector = GetPlaywrightSelector(entry.UIInfo);
                    sb.AppendLine($"{indent}// await page.Locator(\"{textSelector}\").FillAsync(\"{EscapeString(entry.UIInfo?.NewValue)}\");");
                    break;

                case RecordEntryType.UISelectionChange:
                    var selSelector = GetPlaywrightSelector(entry.UIInfo);
                    sb.AppendLine($"{indent}// await page.Locator(\"{selSelector}\").SelectOptionAsync(\"{EscapeString(entry.UIInfo?.NewValue)}\");");
                    break;

                case RecordEntryType.UIClick:
                    var clickSelector = GetPlaywrightSelector(entry.UIInfo);
                    sb.AppendLine($"{indent}// await page.Locator(\"{clickSelector}\").ClickAsync();");
                    break;

                case RecordEntryType.UIToggle:
                    var toggleSelector = GetPlaywrightSelector(entry.UIInfo);
                    var isChecked = entry.UIInfo?.NewValue == "True";
                    sb.AppendLine($"{indent}// await page.Locator(\"{toggleSelector}\").SetCheckedAsync({isChecked.ToString().ToLower()});");
                    break;

                case RecordEntryType.ApiRequest:
                    sb.AppendLine($"{indent}// await Expect(page).ToHaveRequestAsync(request => ");
                    sb.AppendLine($"{indent}//     request.Method == \"{entry.ApiInfo?.Method}\" && ");
                    sb.AppendLine($"{indent}//     request.Url.Contains(\"{entry.ApiInfo?.Url}\"));");
                    break;

                case RecordEntryType.ApiResponse:
                    sb.AppendLine($"{indent}// await Expect(response).ToHaveStatusCodeAsync({entry.ApiInfo?.StatusCode});");
                    break;
            }
        }

        private static string GetTargetSelector(UIInfo info)
        {
            if (info == null) return "unknown";

            if (!string.IsNullOrEmpty(info.AutomationId))
                return $"[AutomationId={info.AutomationId}]";

            if (!string.IsNullOrEmpty(info.ControlName))
                return $"{info.ControlType}#{info.ControlName}";

            return info.ControlType ?? "unknown";
        }

        private static string GetPlaywrightSelector(UIInfo info)
        {
            if (info == null) return "*";

            if (!string.IsNullOrEmpty(info.AutomationId))
                return $"[data-automation-id='{info.AutomationId}']";

            if (!string.IsNullOrEmpty(info.ControlName))
                return $"#{info.ControlName}";

            if (!string.IsNullOrEmpty(info.ContentText))
                return $"text={info.ContentText}";

            return info.ControlType?.ToLower() ?? "*";
        }

        private static string ExtractPropertyName(string bindingPath)
        {
            if (string.IsNullOrEmpty(bindingPath)) return null;

            // Extract property name from binding path like "Text:PropertyName"
            var parts = bindingPath.Split(':');
            return parts.Length > 1 ? parts[1] : parts[0];
        }

        private static string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Test";

            var sanitized = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c))
                    sanitized.Append(c);
                else if (c == ' ' || c == '_' || c == '-')
                    sanitized.Append('_');
            }

            var result = sanitized.ToString();
            if (result.Length > 0 && char.IsDigit(result[0]))
                result = "_" + result;

            return result;
        }

        private static string EscapeString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Length <= maxLength) return value;
            return value.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Exports entries to CSV format
        /// </summary>
        public static void ExportToCsv(IEnumerable<RecordEntry> entries, string filePath)
        {
            var sb = new StringBuilder();

            // Header row - includes new properties
            sb.AppendLine("Timestamp,Type,ControlType,ControlName,AutomationId,Text,ContentText,OldValue,NewValue,WindowTitle,VisualTreePath,ScreenX,ScreenY,KeyCombination,Properties,Method,URL,StatusCode,Duration,CorrelationId");

            foreach (var entry in entries)
            {
                var timestamp = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var type = entry.EntryType.ToString();
                var controlType = EscapeCsv(entry.UIInfo?.ControlType ?? "");
                var controlName = EscapeCsv(entry.UIInfo?.ControlName ?? "");
                var automationId = EscapeCsv(entry.UIInfo?.AutomationId ?? "");
                var text = EscapeCsv(entry.UIInfo?.Text ?? "");
                var contentText = EscapeCsv(entry.UIInfo?.ContentText ?? "");
                var oldValue = EscapeCsv(entry.UIInfo?.OldValue ?? "");
                var newValue = EscapeCsv(entry.UIInfo?.NewValue ?? "");
                var windowTitle = EscapeCsv(entry.UIInfo?.WindowTitle ?? "");
                var visualTreePath = EscapeCsv(entry.UIInfo?.VisualTreePath ?? "");
                var screenX = entry.UIInfo?.ScreenPosition?.X.ToString() ?? "";
                var screenY = entry.UIInfo?.ScreenPosition?.Y.ToString() ?? "";
                var keyCombination = EscapeCsv(entry.UIInfo?.KeyCombination ?? "");
                var properties = EscapeCsv(FormatProperties(entry.UIInfo?.Properties));
                var method = EscapeCsv(entry.ApiInfo?.Method ?? "");
                var url = EscapeCsv(entry.ApiInfo?.Url ?? "");
                var statusCode = entry.ApiInfo?.StatusCode?.ToString() ?? "";
                var duration = entry.DurationMs?.ToString() ?? "";
                var correlationId = EscapeCsv(entry.CorrelationId ?? "");

                sb.AppendLine($"{timestamp},{type},{controlType},{controlName},{automationId},{text},{contentText},{oldValue},{newValue},{windowTitle},{visualTreePath},{screenX},{screenY},{keyCombination},{properties},{method},{url},{statusCode},{duration},{correlationId}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string FormatProperties(Dictionary<string, string>? properties)
        {
            if (properties == null || properties.Count == 0)
                return "";

            return string.Join("; ", properties.Select(p => $"{p.Key}={p.Value}"));
        }

        /// <summary>
        /// Exports entries to Excel XML format (can be opened by Excel)
        /// </summary>
        public static void ExportToExcel(IEnumerable<RecordEntry> entries, string filePath)
        {
            var entryList = entries.ToList();
            var sb = new StringBuilder();

            // Excel XML header
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine("          xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");

            // Styles
            sb.AppendLine("  <Styles>");
            sb.AppendLine("    <Style ss:ID=\"Header\">");
            sb.AppendLine("      <Font ss:Bold=\"1\"/>");
            sb.AppendLine("      <Interior ss:Color=\"#CCCCCC\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("    </Style>");
            sb.AppendLine("    <Style ss:ID=\"DateTime\">");
            sb.AppendLine("      <NumberFormat ss:Format=\"yyyy-mm-dd hh:mm:ss.000\"/>");
            sb.AppendLine("    </Style>");
            sb.AppendLine("    <Style ss:ID=\"Error\">");
            sb.AppendLine("      <Font ss:Color=\"#FF0000\"/>");
            sb.AppendLine("    </Style>");
            sb.AppendLine("  </Styles>");

            // Worksheet
            sb.AppendLine("  <Worksheet ss:Name=\"Recorded Events\">");
            sb.AppendLine($"    <Table ss:ExpandedColumnCount=\"20\" ss:ExpandedRowCount=\"{entryList.Count + 1}\">");

            // Column widths - includes new columns
            sb.AppendLine("      <Column ss:Width=\"140\"/>"); // Timestamp
            sb.AppendLine("      <Column ss:Width=\"80\"/>");  // Type
            sb.AppendLine("      <Column ss:Width=\"80\"/>");  // ControlType
            sb.AppendLine("      <Column ss:Width=\"120\"/>"); // ControlName
            sb.AppendLine("      <Column ss:Width=\"120\"/>"); // AutomationId
            sb.AppendLine("      <Column ss:Width=\"150\"/>"); // Text
            sb.AppendLine("      <Column ss:Width=\"150\"/>"); // ContentText
            sb.AppendLine("      <Column ss:Width=\"100\"/>"); // OldValue
            sb.AppendLine("      <Column ss:Width=\"100\"/>"); // NewValue
            sb.AppendLine("      <Column ss:Width=\"150\"/>"); // WindowTitle
            sb.AppendLine("      <Column ss:Width=\"300\"/>"); // VisualTreePath
            sb.AppendLine("      <Column ss:Width=\"60\"/>");  // ScreenX
            sb.AppendLine("      <Column ss:Width=\"60\"/>");  // ScreenY
            sb.AppendLine("      <Column ss:Width=\"100\"/>"); // KeyCombination
            sb.AppendLine("      <Column ss:Width=\"300\"/>"); // Properties
            sb.AppendLine("      <Column ss:Width=\"60\"/>");  // Method
            sb.AppendLine("      <Column ss:Width=\"200\"/>"); // URL
            sb.AppendLine("      <Column ss:Width=\"60\"/>");  // StatusCode
            sb.AppendLine("      <Column ss:Width=\"60\"/>");  // Duration
            sb.AppendLine("      <Column ss:Width=\"200\"/>"); // CorrelationId

            // Header row
            sb.AppendLine("      <Row>");
            WriteExcelCell(sb, "Timestamp", "Header");
            WriteExcelCell(sb, "Type", "Header");
            WriteExcelCell(sb, "ControlType", "Header");
            WriteExcelCell(sb, "ControlName", "Header");
            WriteExcelCell(sb, "AutomationId", "Header");
            WriteExcelCell(sb, "Text", "Header");
            WriteExcelCell(sb, "ContentText", "Header");
            WriteExcelCell(sb, "OldValue", "Header");
            WriteExcelCell(sb, "NewValue", "Header");
            WriteExcelCell(sb, "WindowTitle", "Header");
            WriteExcelCell(sb, "VisualTreePath", "Header");
            WriteExcelCell(sb, "ScreenX", "Header");
            WriteExcelCell(sb, "ScreenY", "Header");
            WriteExcelCell(sb, "KeyCombination", "Header");
            WriteExcelCell(sb, "Properties", "Header");
            WriteExcelCell(sb, "Method", "Header");
            WriteExcelCell(sb, "URL", "Header");
            WriteExcelCell(sb, "StatusCode", "Header");
            WriteExcelCell(sb, "Duration", "Header");
            WriteExcelCell(sb, "CorrelationId", "Header");
            sb.AppendLine("      </Row>");

            // Data rows
            foreach (var entry in entryList)
            {
                var isError = entry.ApiInfo != null && !entry.ApiInfo.IsSuccess;
                var style = isError ? "Error" : null;

                sb.AppendLine("      <Row>");
                WriteExcelCell(sb, entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), style);
                WriteExcelCell(sb, entry.EntryType.ToString(), style);
                WriteExcelCell(sb, entry.UIInfo?.ControlType ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.ControlName ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.AutomationId ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.Text ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.ContentText ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.OldValue ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.NewValue ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.WindowTitle ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.VisualTreePath ?? "", style);
                WriteExcelCell(sb, entry.UIInfo?.ScreenPosition?.X.ToString() ?? "", style, isNumber: true);
                WriteExcelCell(sb, entry.UIInfo?.ScreenPosition?.Y.ToString() ?? "", style, isNumber: true);
                WriteExcelCell(sb, entry.UIInfo?.KeyCombination ?? "", style);
                WriteExcelCell(sb, FormatProperties(entry.UIInfo?.Properties), style);
                WriteExcelCell(sb, entry.ApiInfo?.Method ?? "", style);
                WriteExcelCell(sb, entry.ApiInfo?.Url ?? "", style);
                WriteExcelCell(sb, entry.ApiInfo?.StatusCode?.ToString() ?? "", style, isNumber: true);
                WriteExcelCell(sb, entry.DurationMs?.ToString() ?? "", style, isNumber: true);
                WriteExcelCell(sb, entry.CorrelationId ?? "", style);
                sb.AppendLine("      </Row>");
            }

            sb.AppendLine("    </Table>");
            sb.AppendLine("  </Worksheet>");
            sb.AppendLine("</Workbook>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static void WriteExcelCell(StringBuilder sb, string value, string style = null, bool isNumber = false)
        {
            var escapedValue = EscapeXml(value);
            var type = isNumber && !string.IsNullOrEmpty(value) ? "Number" : "String";
            var styleAttr = style != null ? $" ss:StyleID=\"{style}\"" : "";

            sb.AppendLine($"        <Cell{styleAttr}><Data ss:Type=\"{type}\">{escapedValue}</Data></Cell>");
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // If value contains comma, quotes, or newlines, wrap in quotes and escape internal quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}
