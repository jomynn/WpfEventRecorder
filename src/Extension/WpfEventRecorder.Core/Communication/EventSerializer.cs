using System.Text.Json;
using System.Text.Json.Serialization;
using WpfEventRecorder.Core.Events;

namespace WpfEventRecorder.Core.Communication;

/// <summary>
/// Serializer for recorded events.
/// </summary>
public static class EventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Serializes an event to JSON.
    /// </summary>
    public static string Serialize(RecordedEvent @event)
    {
        return JsonSerializer.Serialize(@event, Options);
    }

    /// <summary>
    /// Serializes an event to indented JSON.
    /// </summary>
    public static string SerializeIndented(RecordedEvent @event)
    {
        return JsonSerializer.Serialize(@event, IndentedOptions);
    }

    /// <summary>
    /// Deserializes an event from JSON.
    /// </summary>
    public static RecordedEvent? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<RecordedEvent>(json, Options);
    }

    /// <summary>
    /// Serializes a collection of events to JSON.
    /// </summary>
    public static string SerializeMany(IEnumerable<RecordedEvent> events, bool indented = false)
    {
        return JsonSerializer.Serialize(events, indented ? IndentedOptions : Options);
    }

    /// <summary>
    /// Deserializes a collection of events from JSON.
    /// </summary>
    public static List<RecordedEvent>? DeserializeMany(string json)
    {
        return JsonSerializer.Deserialize<List<RecordedEvent>>(json, Options);
    }

    /// <summary>
    /// Serializes a pipe message to JSON.
    /// </summary>
    public static string SerializeMessage(PipeMessage message)
    {
        return JsonSerializer.Serialize(message, Options);
    }

    /// <summary>
    /// Deserializes a pipe message from JSON.
    /// </summary>
    public static PipeMessage? DeserializeMessage(string json)
    {
        return JsonSerializer.Deserialize<PipeMessage>(json, Options);
    }

    /// <summary>
    /// Creates an EventPipeMessage from a recorded event.
    /// </summary>
    public static EventPipeMessage CreateEventMessage(RecordedEvent @event)
    {
        return new EventPipeMessage
        {
            EventData = Serialize(@event),
            EventTypeName = @event.GetType().Name
        };
    }

    /// <summary>
    /// Extracts the recorded event from an EventPipeMessage.
    /// </summary>
    public static RecordedEvent? ExtractEvent(EventPipeMessage message)
    {
        if (string.IsNullOrEmpty(message.EventData))
            return null;

        return Deserialize(message.EventData);
    }
}
