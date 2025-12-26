using System.Text;
using System.Text.Json;

namespace WpfEventRecorder.Core.Http;

/// <summary>
/// Utilities for capturing and formatting HTTP content.
/// </summary>
public static class HttpContentCapture
{
    /// <summary>
    /// Formats JSON content for display.
    /// </summary>
    public static string? FormatJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Masks sensitive data in JSON content.
    /// </summary>
    public static string? MaskSensitiveData(string? json, IEnumerable<string> sensitiveFields)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var maskedJson = MaskJsonElement(doc.RootElement, sensitiveFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            return maskedJson;
        }
        catch
        {
            return json;
        }
    }

    private static string MaskJsonElement(JsonElement element, HashSet<string> sensitiveFields)
    {
        var options = new JsonWriterOptions { Indented = true };

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, options))
        {
            WriteElement(writer, element, sensitiveFields, null);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteElement(Utf8JsonWriter writer, JsonElement element, HashSet<string> sensitiveFields, string? currentProperty)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);

                    if (sensitiveFields.Contains(property.Name))
                    {
                        writer.WriteStringValue("***");
                    }
                    else
                    {
                        WriteElement(writer, property.Value, sensitiveFields, property.Name);
                    }
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteElement(writer, item, sensitiveFields, null);
                }
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteRawValue(element.GetRawText());
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;
        }
    }

    /// <summary>
    /// Truncates content to a maximum length with ellipsis.
    /// </summary>
    public static string Truncate(string content, int maxLength)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            return content;

        return content[..maxLength] + "...";
    }

    /// <summary>
    /// Creates a summary of the HTTP content.
    /// </summary>
    public static string CreateSummary(string? content, string? contentType, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(content))
            return "[Empty]";

        if (contentType?.Contains("json") == true)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    return $"[Array with {root.GetArrayLength()} items]";
                }

                if (root.ValueKind == JsonValueKind.Object)
                {
                    var propertyCount = root.EnumerateObject().Count();
                    return $"[Object with {propertyCount} properties]";
                }
            }
            catch
            {
                // Fall through to truncation
            }
        }

        return Truncate(content, maxLength);
    }
}
