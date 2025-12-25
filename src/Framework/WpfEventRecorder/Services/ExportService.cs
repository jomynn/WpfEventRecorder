using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Services
{
    /// <summary>
    /// Service for exporting recorded events to various formats
    /// </summary>
    public static class ExportService
    {
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
