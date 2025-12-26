using FluentAssertions;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Export;
using WpfEventRecorder.Core.Recording;
using Xunit;

namespace WpfEventRecorder.Tests.Export;

public class XUnitExporterTests
{
    [Fact]
    public void Export_ShouldProduceValidCSharpCode()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new InputEvent { InputType = InputEventType.TextChanged, SourceElementName = "Name", NewValue = "John" }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("using Xunit;");
        code.Should().Contain("public class");
        code.Should().Contain("[Fact]");
    }

    [Fact]
    public void Export_WithSession_ShouldUseSessionNameForClass()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var session = new RecordingSession { Name = "LoginFlow" };
        session.AddEvent(new InputEvent { InputType = InputEventType.TextChanged });

        // Act
        var code = exporter.Export(session.Events, session);

        // Assert
        code.Should().Contain("public class LoginFlowTests");
    }

    [Fact]
    public void Export_TextChangedEvent_ShouldGenerateEnterTextCall()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new InputEvent
            {
                InputType = InputEventType.TextChanged,
                AutomationId = "UsernameBox",
                NewValue = "testuser"
            }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("EnterTextAsync");
        code.Should().Contain("UsernameBox");
        code.Should().Contain("testuser");
    }

    [Fact]
    public void Export_ButtonClickEvent_ShouldGenerateClickCall()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new InputEvent
            {
                InputType = InputEventType.ButtonClicked,
                SourceElementName = "SubmitButton"
            }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("ClickButtonAsync");
        code.Should().Contain("SubmitButton");
    }

    [Fact]
    public void Export_SelectionChangedEvent_ShouldGenerateSelectItemCall()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new InputEvent
            {
                InputType = InputEventType.SelectionChanged,
                AutomationId = "CountryCombo",
                NewValue = "United States"
            }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("SelectItemAsync");
        code.Should().Contain("CountryCombo");
        code.Should().Contain("United States");
    }

    [Fact]
    public void Export_CheckedChangedEvent_ShouldGenerateSetCheckboxCall()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new InputEvent
            {
                InputType = InputEventType.CheckedChanged,
                SourceElementName = "AcceptTerms",
                NewValue = true
            }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("SetCheckboxAsync");
        code.Should().Contain("AcceptTerms");
        code.Should().Contain("true");
    }

    [Fact]
    public void Export_CommandEvent_ShouldGenerateExecuteCommandCall()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new CommandEvent
            {
                CommandName = "SaveCommand",
                CommandParameter = "123"
            }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("ExecuteCommandAsync");
        code.Should().Contain("SaveCommand");
    }

    [Fact]
    public void Export_TabChangedEvent_ShouldGenerateSelectTabCall()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new NavigationEvent
            {
                NavigationType = NavigationType.TabChanged,
                TabHeader = "Settings"
            }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("SelectTabAsync");
        code.Should().Contain("Settings");
    }

    [Fact]
    public void Export_ShouldIncludeHelperMethods()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new InputEvent { InputType = InputEventType.TextChanged }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("private static async Task EnterTextAsync");
        code.Should().Contain("private static async Task ClickButtonAsync");
        code.Should().Contain("private static async Task SelectItemAsync");
    }

    [Fact]
    public void FileExtension_ShouldReturnCsExtension()
    {
        // Arrange
        var exporter = new XUnitExporter();

        // Assert
        exporter.FileExtension.Should().Be(".cs");
    }

    [Fact]
    public void FormatName_ShouldReturnXUnit()
    {
        // Arrange
        var exporter = new XUnitExporter();

        // Assert
        exporter.FormatName.Should().Be("xUnit");
    }

    [Fact]
    public void Export_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var exporter = new XUnitExporter();
        var events = new RecordedEvent[]
        {
            new InputEvent
            {
                InputType = InputEventType.TextChanged,
                SourceElementName = "Notes",
                NewValue = "Line1\nLine2\tTabbed"
            }
        };

        // Act
        var code = exporter.Export(events);

        // Assert
        code.Should().Contain("\\n");
        code.Should().Contain("\\t");
    }
}
