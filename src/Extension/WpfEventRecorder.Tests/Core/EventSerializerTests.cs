using FluentAssertions;
using WpfEventRecorder.Core.Communication;
using WpfEventRecorder.Core.Events;
using Xunit;

namespace WpfEventRecorder.Tests.Core;

public class EventSerializerTests
{
    [Fact]
    public void Serialize_InputEvent_ShouldProduceValidJson()
    {
        // Arrange
        var inputEvent = new InputEvent
        {
            Id = "test-id-123",
            InputType = InputEventType.TextChanged,
            SourceElementName = "TestBox",
            OldValue = "old",
            NewValue = "new"
        };

        // Act
        var json = EventSerializer.Serialize(inputEvent);

        // Assert
        json.Should().Contain("\"$type\":\"input\"");
        json.Should().Contain("\"id\":\"test-id-123\"");
        json.Should().Contain("\"inputType\":\"textChanged\"");
        json.Should().Contain("\"sourceElementName\":\"TestBox\"");
    }

    [Fact]
    public void Deserialize_InputEvent_ShouldRestoreCorrectType()
    {
        // Arrange
        var originalEvent = new InputEvent
        {
            Id = "test-id-456",
            InputType = InputEventType.ButtonClicked,
            SourceElementName = "SubmitBtn",
            NewValue = "Click"
        };
        var json = EventSerializer.Serialize(originalEvent);

        // Act
        var deserializedEvent = EventSerializer.Deserialize(json);

        // Assert
        deserializedEvent.Should().BeOfType<InputEvent>();
        var inputEvent = (InputEvent)deserializedEvent!;
        inputEvent.Id.Should().Be("test-id-456");
        inputEvent.InputType.Should().Be(InputEventType.ButtonClicked);
        inputEvent.SourceElementName.Should().Be("SubmitBtn");
    }

    [Fact]
    public void Serialize_CommandEvent_ShouldIncludeAllProperties()
    {
        // Arrange
        var commandEvent = new CommandEvent
        {
            CommandName = "SaveCommand",
            CommandParameter = "param1",
            IsSuccess = true,
            ExecutionDurationMs = 150
        };

        // Act
        var json = EventSerializer.Serialize(commandEvent);

        // Assert
        json.Should().Contain("\"$type\":\"command\"");
        json.Should().Contain("\"commandName\":\"SaveCommand\"");
        json.Should().Contain("\"commandParameter\":\"param1\"");
        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("\"executionDurationMs\":150");
    }

    [Fact]
    public void Serialize_ApiCallEvent_ShouldIncludeHttpDetails()
    {
        // Arrange
        var apiEvent = new ApiCallEvent
        {
            HttpMethod = "POST",
            RequestUrl = "https://api.example.com/users",
            StatusCode = 201,
            DurationMs = 250,
            RequestBody = "{\"name\":\"John\"}"
        };

        // Act
        var json = EventSerializer.Serialize(apiEvent);

        // Assert
        json.Should().Contain("\"$type\":\"api\"");
        json.Should().Contain("\"httpMethod\":\"POST\"");
        json.Should().Contain("\"requestUrl\":\"https://api.example.com/users\"");
        json.Should().Contain("\"statusCode\":201");
    }

    [Fact]
    public void SerializeMany_ShouldProduceValidJsonArray()
    {
        // Arrange
        var events = new RecordedEvent[]
        {
            new InputEvent { InputType = InputEventType.TextChanged },
            new CommandEvent { CommandName = "Test" },
            new ApiCallEvent { HttpMethod = "GET" }
        };

        // Act
        var json = EventSerializer.SerializeMany(events, true);

        // Assert
        json.Should().StartWith("[");
        json.Should().EndWith("]");
        json.Should().Contain("\"$type\":\"input\"");
        json.Should().Contain("\"$type\":\"command\"");
        json.Should().Contain("\"$type\":\"api\"");
    }

    [Fact]
    public void DeserializeMany_ShouldRestoreAllEvents()
    {
        // Arrange
        var originalEvents = new RecordedEvent[]
        {
            new InputEvent { Id = "1", InputType = InputEventType.TextChanged },
            new CommandEvent { Id = "2", CommandName = "Test" },
            new NavigationEvent { Id = "3", NavigationType = NavigationType.ViewNavigation }
        };
        var json = EventSerializer.SerializeMany(originalEvents);

        // Act
        var deserializedEvents = EventSerializer.DeserializeMany(json);

        // Assert
        deserializedEvents.Should().HaveCount(3);
        deserializedEvents![0].Should().BeOfType<InputEvent>();
        deserializedEvents[1].Should().BeOfType<CommandEvent>();
        deserializedEvents[2].Should().BeOfType<NavigationEvent>();
    }

    [Fact]
    public void CreateEventMessage_ShouldWrapEventCorrectly()
    {
        // Arrange
        var inputEvent = new InputEvent
        {
            InputType = InputEventType.SelectionChanged,
            SourceElementName = "CountryCombo"
        };

        // Act
        var message = EventSerializer.CreateEventMessage(inputEvent);

        // Assert
        message.MessageType.Should().Be(PipeMessageType.Event);
        message.EventTypeName.Should().Be("InputEvent");
        message.EventData.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ExtractEvent_ShouldRecoverOriginalEvent()
    {
        // Arrange
        var originalEvent = new CommandEvent
        {
            CommandName = "DeleteCommand",
            IsSuccess = false,
            ErrorMessage = "Not found"
        };
        var message = EventSerializer.CreateEventMessage(originalEvent);

        // Act
        var extractedEvent = EventSerializer.ExtractEvent(message);

        // Assert
        extractedEvent.Should().BeOfType<CommandEvent>();
        var command = (CommandEvent)extractedEvent!;
        command.CommandName.Should().Be("DeleteCommand");
        command.IsSuccess.Should().BeFalse();
        command.ErrorMessage.Should().Be("Not found");
    }

    [Fact]
    public void SerializeMessage_ShouldIncludeMessageType()
    {
        // Arrange
        var statusMessage = new StatusPipeMessage
        {
            IsRecording = true,
            EventCount = 42,
            ApplicationName = "TestApp"
        };

        // Act
        var json = EventSerializer.SerializeMessage(statusMessage);

        // Assert
        json.Should().Contain("\"type\":\"status\"");
        json.Should().Contain("\"isRecording\":true");
        json.Should().Contain("\"eventCount\":42");
    }
}
