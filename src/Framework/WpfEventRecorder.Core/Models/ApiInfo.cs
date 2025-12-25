using System;
using System.Collections.Generic;

namespace WpfEventRecorder.Core.Models
{
    /// <summary>
    /// Information about an HTTP API call
    /// </summary>
    public class ApiInfo
    {
        /// <summary>
        /// HTTP method (GET, POST, PUT, DELETE, etc.)
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Full request URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// URL path (without query string)
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Query string parameters
        /// </summary>
        public Dictionary<string, string>? QueryParameters { get; set; }

        /// <summary>
        /// Request headers
        /// </summary>
        public Dictionary<string, string>? RequestHeaders { get; set; }

        /// <summary>
        /// Request body content
        /// </summary>
        public string? RequestBody { get; set; }

        /// <summary>
        /// Request content type
        /// </summary>
        public string? RequestContentType { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Response headers
        /// </summary>
        public Dictionary<string, string>? ResponseHeaders { get; set; }

        /// <summary>
        /// Response body content
        /// </summary>
        public string? ResponseBody { get; set; }

        /// <summary>
        /// Response content type
        /// </summary>
        public string? ResponseContentType { get; set; }

        /// <summary>
        /// Whether the request was successful (2xx status)
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if the request failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Request timestamp
        /// </summary>
        public DateTime RequestTimestamp { get; set; }

        /// <summary>
        /// Response timestamp
        /// </summary>
        public DateTime? ResponseTimestamp { get; set; }
    }
}
