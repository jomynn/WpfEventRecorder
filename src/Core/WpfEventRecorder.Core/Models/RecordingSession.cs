using System;
using System.Collections.Generic;

namespace WpfEventRecorder.Core.Models
{
    /// <summary>
    /// Represents a complete recording session with metadata
    /// </summary>
    public class RecordingSession
    {
        /// <summary>
        /// Unique session identifier
        /// </summary>
        public Guid SessionId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Session name/description
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// When the recording started
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// When the recording ended
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Total duration of the recording
        /// </summary>
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;

        /// <summary>
        /// Application name being recorded
        /// </summary>
        public string? ApplicationName { get; set; }

        /// <summary>
        /// Application version
        /// </summary>
        public string? ApplicationVersion { get; set; }

        /// <summary>
        /// Machine name where recording occurred
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// OS version
        /// </summary>
        public string? OSVersion { get; set; }

        /// <summary>
        /// User who performed the recording
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// All recorded entries in this session
        /// </summary>
        public List<RecordEntry> Entries { get; set; } = new List<RecordEntry>();

        /// <summary>
        /// The target window being recorded
        /// </summary>
        public WindowInfo? TargetWindow { get; set; }

        /// <summary>
        /// Additional session metadata
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Schema version for forward compatibility
        /// </summary>
        public string SchemaVersion { get; set; } = "1.0";

        /// <summary>
        /// Creates a new session with current environment info
        /// </summary>
        public static RecordingSession Create(string name)
        {
            return new RecordingSession
            {
                Name = name,
                StartTime = DateTime.UtcNow,
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                UserName = Environment.UserName
            };
        }
    }
}
