using System.Net;

namespace WpfEventRecorder.Core.Events;

/// <summary>
/// Represents an HTTP API call event.
/// </summary>
public class ApiCallEvent : RecordedEvent
{
    public override string EventType => "ApiCall";

    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Request URL.
    /// </summary>
    public string? RequestUrl { get; set; }

    /// <summary>
    /// Request headers.
    /// </summary>
    public Dictionary<string, string> RequestHeaders { get; set; } = new();

    /// <summary>
    /// Request body content.
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// Content type of the request.
    /// </summary>
    public string? RequestContentType { get; set; }

    /// <summary>
    /// HTTP status code of the response.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Response headers.
    /// </summary>
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();

    /// <summary>
    /// Response body content.
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Content type of the response.
    /// </summary>
    public string? ResponseContentType { get; set; }

    /// <summary>
    /// Duration of the API call in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Whether the request was successful (2xx status code).
    /// </summary>
    public bool IsSuccess => StatusCode.HasValue && StatusCode >= 200 && StatusCode < 300;

    /// <summary>
    /// Error message if the request failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the status phrase for the status code.
    /// </summary>
    public string? StatusPhrase => StatusCode.HasValue
        ? ((HttpStatusCode)StatusCode.Value).ToString()
        : null;

    public override string GetDescription()
    {
        var status = StatusCode.HasValue ? $" → {StatusCode} {StatusPhrase}" : "";
        var duration = $" ({DurationMs}ms)";
        return $"{HttpMethod} {RequestUrl}{status}{duration}";
    }
}
