using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Hooks
{
    /// <summary>
    /// HTTP message handler that records API calls
    /// </summary>
    public class RecordingHttpHandler : DelegatingHandler
    {
        private readonly Subject<RecordEntry> _requestSubject = new Subject<RecordEntry>();
        private readonly Subject<RecordEntry> _responseSubject = new Subject<RecordEntry>();
        private readonly bool _captureRequestBody;
        private readonly bool _captureResponseBody;
        private readonly int _maxBodySize;
        private bool _isActive;

        /// <summary>
        /// Observable stream of API request events
        /// </summary>
        public IObservable<RecordEntry> Requests => _requestSubject.AsObservable();

        /// <summary>
        /// Observable stream of API response events
        /// </summary>
        public IObservable<RecordEntry> Responses => _responseSubject.AsObservable();

        /// <summary>
        /// Combined observable of all API events (requests and responses)
        /// </summary>
        public IObservable<RecordEntry> AllEvents => _requestSubject.Merge(_responseSubject);

        /// <summary>
        /// Whether recording is active
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        /// <summary>
        /// Creates a new recording HTTP handler
        /// </summary>
        /// <param name="innerHandler">The inner handler to delegate to</param>
        /// <param name="captureRequestBody">Whether to capture request bodies</param>
        /// <param name="captureResponseBody">Whether to capture response bodies</param>
        /// <param name="maxBodySize">Maximum body size to capture in bytes</param>
        public RecordingHttpHandler(
            HttpMessageHandler? innerHandler = null,
            bool captureRequestBody = true,
            bool captureResponseBody = true,
            int maxBodySize = 1024 * 1024) // 1MB default
        {
            InnerHandler = innerHandler ?? new HttpClientHandler();
            _captureRequestBody = captureRequestBody;
            _captureResponseBody = captureResponseBody;
            _maxBodySize = maxBodySize;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!_isActive)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var correlationId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();
            var requestTimestamp = DateTime.UtcNow;

            // Record request
            var requestEntry = await CreateRequestEntryAsync(request, correlationId, requestTimestamp);
            _requestSubject.OnNext(requestEntry);

            HttpResponseMessage? response = null;
            Exception? exception = null;

            try
            {
                response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Record response
                var responseEntry = await CreateResponseEntryAsync(
                    request, response, exception, correlationId,
                    requestTimestamp, stopwatch.ElapsedMilliseconds);
                _responseSubject.OnNext(responseEntry);
            }
        }

        private async Task<RecordEntry> CreateRequestEntryAsync(
            HttpRequestMessage request,
            string correlationId,
            DateTime timestamp)
        {
            string? requestBody = null;
            string? contentType = null;

            if (_captureRequestBody && request.Content != null)
            {
                contentType = request.Content.Headers.ContentType?.ToString();
                var length = request.Content.Headers.ContentLength ?? 0;

                if (length <= _maxBodySize)
                {
                    requestBody = await request.Content.ReadAsStringAsync();
                }
                else
                {
                    requestBody = $"[Content too large: {length} bytes]";
                }
            }

            var uri = request.RequestUri;
            var queryParams = uri != null ? ParseQueryString(uri.Query) : null;

            return new RecordEntry
            {
                EntryType = RecordEntryType.ApiRequest,
                CorrelationId = correlationId,
                Timestamp = timestamp,
                ApiInfo = new ApiInfo
                {
                    Method = request.Method.ToString(),
                    Url = uri?.ToString() ?? string.Empty,
                    Path = uri?.AbsolutePath,
                    QueryParameters = queryParams,
                    RequestHeaders = ExtractHeaders(request.Headers),
                    RequestBody = requestBody,
                    RequestContentType = contentType,
                    RequestTimestamp = timestamp
                }
            };
        }

        private async Task<RecordEntry> CreateResponseEntryAsync(
            HttpRequestMessage request,
            HttpResponseMessage? response,
            Exception? exception,
            string correlationId,
            DateTime requestTimestamp,
            long durationMs)
        {
            var responseTimestamp = DateTime.UtcNow;
            string? responseBody = null;
            string? contentType = null;
            int? statusCode = null;
            bool isSuccess = false;
            string? errorMessage = null;
            Dictionary<string, string>? responseHeaders = null;

            if (response != null)
            {
                statusCode = (int)response.StatusCode;
                isSuccess = response.IsSuccessStatusCode;
                responseHeaders = ExtractHeaders(response.Headers);
                contentType = response.Content.Headers.ContentType?.ToString();

                if (_captureResponseBody && response.Content != null)
                {
                    var length = response.Content.Headers.ContentLength ?? 0;

                    if (length <= _maxBodySize)
                    {
                        try
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                        }
                        catch
                        {
                            responseBody = "[Failed to read response body]";
                        }
                    }
                    else
                    {
                        responseBody = $"[Content too large: {length} bytes]";
                    }
                }
            }

            if (exception != null)
            {
                errorMessage = exception.Message;
                isSuccess = false;
            }

            var uri = request.RequestUri;

            return new RecordEntry
            {
                EntryType = RecordEntryType.ApiResponse,
                CorrelationId = correlationId,
                Timestamp = responseTimestamp,
                DurationMs = durationMs,
                ApiInfo = new ApiInfo
                {
                    Method = request.Method.ToString(),
                    Url = uri?.ToString() ?? string.Empty,
                    Path = uri?.AbsolutePath,
                    StatusCode = statusCode,
                    ResponseHeaders = responseHeaders,
                    ResponseBody = responseBody,
                    ResponseContentType = contentType,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage,
                    RequestTimestamp = requestTimestamp,
                    ResponseTimestamp = responseTimestamp
                }
            };
        }

        private static Dictionary<string, string>? ExtractHeaders(
            System.Net.Http.Headers.HttpHeaders headers)
        {
            if (headers == null || !headers.Any())
                return null;

            var result = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                result[header.Key] = string.Join(", ", header.Value);
            }
            return result;
        }

        private static Dictionary<string, string>? ParseQueryString(string query)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            var result = new Dictionary<string, string>();
            var queryTrimmed = query.TrimStart('?');
            var pairs = queryTrimmed.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var parts = pair.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    result[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1]);
                }
                else if (parts.Length == 1)
                {
                    result[Uri.UnescapeDataString(parts[0])] = string.Empty;
                }
            }

            return result.Count > 0 ? result : null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _requestSubject.Dispose();
                _responseSubject.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
