using FluentAssertions;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Export;
using WpfEventRecorder.Core.Recording;
using Xunit;

namespace WpfEventRecorder.Tests.Export;

public class JsonExporterTests
{
    [Fact]
    public void Export_ShouldProduceValidJson()
    {
        // Arrange
        var exporter = new JsonExporter();
        var events = new RecordedEvent[]
        {
            new InputEvent { InputType = InputEventType.TextChanged, SourceElementName = "Name", NewValue = "John" },
            new InputEvent { InputType = InputEventType.ButtonClicked, SourceElementName = "Save" }
        };

        // Act
        var json = exporter.Export(events);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"events\":");
        json.Should().Contain("\"inputType\":");
    }

    [Fact]
    public void Export_WithSession_ShouldIncludeSessionInfo()
    {
        // Arrange
        var exporter = new JsonExporter();
        var session = new RecordingSession
        {
            Name = "TestSession",
            ApplicationName = "TestApp"
        };
        session.AddEvent(new InputEvent { InputType = InputEventType.TextChanged });

        // Act
        var json = exporter.Export(session.Events, session);

        // Assert
        json.Should().Contain("\"session\":");
        json.Should().Contain("\"name\":\"TestSession\"");
        json.Should().Contain("\"applicationName\":\"TestApp\"");
    }

    [Fact]
    public void FormatName_ShouldReturnJson()
    {
        // Arrange
        var exporter = new JsonExporter();

        // Assert
        exporter.FormatName.Should().Be("JSON");
    }

    [Fact]
    public void FileExtension_ShouldReturnJsonExtension()
    {
        // Arrange
        var exporter = new JsonExporter();

        // Assert
        exporter.FileExtension.Should().Be(".json");
    }

    [Fact]
    public void Export_EmptyEvents_ShouldProduceValidEmptyArray()
    {
        // Arrange
        var exporter = new JsonExporter();
        var events = Array.Empty<RecordedEvent>();

        // Act
        var json = exporter.Export(events);

        // Assert
        json.Should().Contain("\"events\": []");
    }

    [Fact]
    public void Export_ApiCallEvent_ShouldIncludeAllDetails()
    {
        // Arrange
        var exporter = new JsonExporter();
        var events = new RecordedEvent[]
        {
            new ApiCallEvent
            {
                HttpMethod = "POST",
                RequestUrl = "https://api.test.com/users",
                StatusCode = 201,
                DurationMs = 150,
                RequestBody = "{\"name\":\"Test\"}",
                ResponseBody = "{\"id\":1}"
            }
        };

        // Act
        var json = exporter.Export(events);

        // Assert
        json.Should().Contain("\"httpMethod\":\"POST\"");
        json.Should().Contain("\"requestUrl\":\"https://api.test.com/users\"");
        json.Should().Contain("\"statusCode\":201");
    }
}
