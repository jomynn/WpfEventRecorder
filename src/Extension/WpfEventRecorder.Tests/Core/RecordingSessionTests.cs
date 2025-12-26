using FluentAssertions;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;
using Xunit;

namespace WpfEventRecorder.Tests.Core;

public class RecordingSessionTests
{
    [Fact]
    public void NewSession_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var session = new RecordingSession();

        // Assert
        session.Id.Should().NotBeNullOrEmpty();
        session.Name.Should().StartWith("Recording_");
        session.Events.Should().BeEmpty();
        session.StartTime.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        session.EndTime.Should().BeNull();
    }

    [Fact]
    public void AddEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var session = new RecordingSession();
        var inputEvent = new InputEvent
        {
            InputType = InputEventType.TextChanged,
            SourceElementName = "TestTextBox",
            NewValue = "Test Value"
        };

        // Act
        session.AddEvent(inputEvent);

        // Assert
        session.Events.Should().ContainSingle();
        session.Events.First().Should().BeSameAs(inputEvent);
    }

    [Fact]
    public void AddEvent_ShouldAssignSequenceNumber()
    {
        // Arrange
        var session = new RecordingSession();
        var event1 = new InputEvent { InputType = InputEventType.TextChanged };
        var event2 = new InputEvent { InputType = InputEventType.ButtonClicked };
        var event3 = new CommandEvent { CommandName = "TestCommand" };

        // Act
        session.AddEvent(event1);
        session.AddEvent(event2);
        session.AddEvent(event3);

        // Assert
        event1.SequenceNumber.Should().Be(1);
        event2.SequenceNumber.Should().Be(2);
        event3.SequenceNumber.Should().Be(3);
    }

    [Fact]
    public void AddEvent_ShouldRaiseEventRecordedEvent()
    {
        // Arrange
        var session = new RecordingSession();
        RecordedEvent? raisedEvent = null;
        session.EventRecorded += (_, e) => raisedEvent = e;

        var inputEvent = new InputEvent
        {
            InputType = InputEventType.ButtonClicked,
            SourceElementName = "Button1"
        };

        // Act
        session.AddEvent(inputEvent);

        // Assert
        raisedEvent.Should().NotBeNull();
        raisedEvent.Should().BeSameAs(inputEvent);
    }

    [Fact]
    public void Clear_ShouldRemoveAllEvents()
    {
        // Arrange
        var session = new RecordingSession();
        session.AddEvent(new InputEvent { InputType = InputEventType.TextChanged });
        session.AddEvent(new CommandEvent { CommandName = "Test" });
        session.AddEvent(new ApiCallEvent { HttpMethod = "GET", RequestUrl = "http://test.com" });

        // Act
        session.Clear();

        // Assert
        session.Events.Should().BeEmpty();
    }

    [Fact]
    public void GetEventsByType_ShouldReturnOnlyMatchingEvents()
    {
        // Arrange
        var session = new RecordingSession();
        session.AddEvent(new InputEvent { InputType = InputEventType.TextChanged });
        session.AddEvent(new CommandEvent { CommandName = "Test1" });
        session.AddEvent(new InputEvent { InputType = InputEventType.ButtonClicked });
        session.AddEvent(new CommandEvent { CommandName = "Test2" });
        session.AddEvent(new ApiCallEvent { HttpMethod = "GET" });

        // Act
        var inputEvents = session.GetEventsByType<InputEvent>().ToList();
        var commandEvents = session.GetEventsByType<CommandEvent>().ToList();
        var apiEvents = session.GetEventsByType<ApiCallEvent>().ToList();

        // Assert
        inputEvents.Should().HaveCount(2);
        commandEvents.Should().HaveCount(2);
        apiEvents.Should().HaveCount(1);
    }

    [Fact]
    public void GetCorrelatedGroups_ShouldGroupByCorrelationId()
    {
        // Arrange
        var session = new RecordingSession();
        var correlationId = "corr-123";

        session.AddEvent(new InputEvent { CorrelationId = correlationId });
        session.AddEvent(new CommandEvent { CorrelationId = correlationId });
        session.AddEvent(new ApiCallEvent { CorrelationId = correlationId });
        session.AddEvent(new InputEvent { CorrelationId = "other" });

        // Act
        var groups = session.GetCorrelatedGroups().ToList();

        // Assert
        groups.Should().HaveCount(2);
        groups.First(g => g.Key == correlationId).Should().HaveCount(3);
        groups.First(g => g.Key == "other").Should().HaveCount(1);
    }

    [Fact]
    public void Duration_ShouldCalculateCorrectly_WhenSessionActive()
    {
        // Arrange
        var session = new RecordingSession
        {
            StartTime = DateTimeOffset.Now.AddMinutes(-5)
        };

        // Act
        var duration = session.Duration;

        // Assert
        duration.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Duration_ShouldCalculateCorrectly_WhenSessionEnded()
    {
        // Arrange
        var session = new RecordingSession
        {
            StartTime = DateTimeOffset.Now.AddMinutes(-10),
            EndTime = DateTimeOffset.Now.AddMinutes(-5)
        };

        // Act
        var duration = session.Duration;

        // Assert
        duration.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1));
    }
}
