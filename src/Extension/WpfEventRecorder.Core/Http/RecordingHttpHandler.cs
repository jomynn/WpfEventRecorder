using System.Diagnostics;
using System.Net.Http.Headers;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Http;

/// <summary>
/// HTTP message handler that records API calls.
/// </summary>
public class RecordingHttpHandler : DelegatingHandler
{
    private readonly RecordingSession? _session;
    private readonly int _maxPayloadSize;

    public RecordingHttpHandler(RecordingSession? session, HttpMessageHandler? innerHandler = null)
        : base(innerHandler ?? new HttpClientHandler())
    {
        _session = session;
        _maxPayloadSize = session?.Configuration.MaxPayloadSize ?? 1024 * 1024;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        // Capture request details
        var apiEvent = new ApiCallEvent
        {
            CorrelationId = correlationId,
            HttpMethod = request.Method.Method,
            RequestUrl = request.RequestUri?.ToString(),
            RequestContentType = request.Content?.Headers.ContentType?.MediaType
        };

        // Capture request headers
        CaptureHeaders(request.Headers, apiEvent.RequestHeaders);
        if (request.Content != null)
        {
            CaptureHeaders(request.Content.Headers, apiEvent.RequestHeaders);
        }

        // Capture request body
        if (_session?.Configuration.CaptureApiPayloads == true && request.Content != null)
        {
            apiEvent.RequestBody = await CaptureContentAsync(request.Content, cancellationToken);
        }

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            apiEvent.DurationMs = stopwatch.ElapsedMilliseconds;
            apiEvent.ErrorMessage = ex.Message;
            _session?.AddEvent(apiEvent);
            throw;
        }

        stopwatch.Stop();
        apiEvent.DurationMs = stopwatch.ElapsedMilliseconds;
        apiEvent.StatusCode = (int)response.StatusCode;

        // Capture response headers
        CaptureHeaders(response.Headers, apiEvent.ResponseHeaders);
        if (response.Content != null)
        {
            CaptureHeaders(response.Content.Headers, apiEvent.ResponseHeaders);
            apiEvent.ResponseContentType = response.Content.Headers.ContentType?.MediaType;

            // Capture response body
            if (_session?.Configuration.CaptureApiPayloads == true)
            {
                apiEvent.ResponseBody = await CaptureContentAsync(response.Content, cancellationToken);
            }
        }

        _session?.AddEvent(apiEvent);

        return response;
    }

    private static void CaptureHeaders(HttpHeaders headers, Dictionary<string, string> target)
    {
        foreach (var header in headers)
        {
            // Skip sensitive headers
            if (IsSensitiveHeader(header.Key))
            {
                target[header.Key] = "***";
            }
            else
            {
                target[header.Key] = string.Join(", ", header.Value);
            }
        }
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitive = new[]
        {
            "authorization", "x-api-key", "api-key", "x-auth-token",
            "cookie", "set-cookie", "x-csrf-token"
        };
        return sensitive.Contains(headerName.ToLowerInvariant());
    }

    private async Task<string?> CaptureContentAsync(HttpContent content, CancellationToken cancellationToken)
    {
        try
        {
            // Check content length
            if (content.Headers.ContentLength > _maxPayloadSize)
            {
                return $"[Content too large: {content.Headers.ContentLength} bytes]";
            }

            // Only capture text-based content
            var mediaType = content.Headers.ContentType?.MediaType;
            if (!IsTextContent(mediaType))
            {
                return $"[Binary content: {mediaType}]";
            }

            var body = await content.ReadAsStringAsync(cancellationToken);

            if (body.Length > _maxPayloadSize)
            {
                return body[.._maxPayloadSize] + "... [truncated]";
            }

            return body;
        }
        catch
        {
            return "[Failed to read content]";
        }
    }

    private static bool IsTextContent(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType)) return false;

        return mediaType.Contains("json") ||
               mediaType.Contains("xml") ||
               mediaType.Contains("text") ||
               mediaType.Contains("html") ||
               mediaType.Contains("javascript");
    }
}
