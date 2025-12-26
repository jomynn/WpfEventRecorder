using FluentAssertions;
using WpfEventRecorder.Core.Events;
using WpfEventRecorder.Core.Recording;
using Xunit;

namespace WpfEventRecorder.Tests.Core;

public class ViewInstrumenterTests
{
    [Fact]
    public void RecordingConfiguration_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new RecordingConfiguration();

        // Assert
        config.RecordInputEvents.Should().BeTrue();
        config.RecordCommands.Should().BeTrue();
        config.RecordApiCalls.Should().BeTrue();
        config.RecordNavigation.Should().BeTrue();
        config.RecordWindowEvents.Should().BeTrue();
        config.CaptureApiPayloads.Should().BeTrue();
        config.EnableCorrelation.Should().BeTrue();
        config.PipeName.Should().Be("WpfEventRecorder");
    }

    [Fact]
    public void RecordingConfiguration_SensitiveFields_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new RecordingConfiguration();

        // Assert
        config.SensitiveFields.Should().Contain("password");
        config.SensitiveFields.Should().Contain("token");
        config.SensitiveFields.Should().Contain("apikey");
        config.SensitiveFields.Should().Contain("authorization");
    }

    [Fact]
    public void InputEvent_GetDescription_ShouldReturnCorrectDescription_ForTextChanged()
    {
        // Arrange
        var inputEvent = new InputEvent
        {
            InputType = InputEventType.TextChanged,
            SourceElementName = "FirstNameBox",
            NewValue = "John"
        };

        // Act
        var description = inputEvent.GetDescription();

        // Assert
        description.Should().Contain("Enter");
        description.Should().Contain("John");
        description.Should().Contain("FirstNameBox");
    }

    [Fact]
    public void InputEvent_GetDescription_ShouldReturnCorrectDescription_ForButtonClicked()
    {
        // Arrange
        var inputEvent = new InputEvent
        {
            InputType = InputEventType.ButtonClicked,
            AutomationId = "SubmitButton"
        };

        // Act
        var description = inputEvent.GetDescription();

        // Assert
        description.Should().Contain("Click");
        description.Should().Contain("SubmitButton");
    }

    [Fact]
    public void InputEvent_GetDescription_ShouldReturnCorrectDescription_ForCheckBox()
    {
        // Arrange
        var checkEvent = new InputEvent
        {
            InputType = InputEventType.CheckedChanged,
            SourceElementName = "IsActiveCheckbox",
            NewValue = true
        };

        var uncheckEvent = new InputEvent
        {
            InputType = InputEventType.CheckedChanged,
            SourceElementName = "IsActiveCheckbox",
            NewValue = false
        };

        // Act
        var checkDescription = checkEvent.GetDescription();
        var uncheckDescription = uncheckEvent.GetDescription();

        // Assert
        checkDescription.Should().Contain("Check");
        uncheckDescription.Should().Contain("Uncheck");
    }

    [Fact]
    public void CommandEvent_GetDescription_ShouldIncludeCommandName()
    {
        // Arrange
        var commandEvent = new CommandEvent
        {
            CommandName = "SaveCommand",
            IsSuccess = true
        };

        // Act
        var description = commandEvent.GetDescription();

        // Assert
        description.Should().Contain("Execute");
        description.Should().Contain("SaveCommand");
    }

    [Fact]
    public void CommandEvent_GetDescription_ShouldIndicateFailure()
    {
        // Arrange
        var commandEvent = new CommandEvent
        {
            CommandName = "SaveCommand",
            IsSuccess = false,
            ErrorMessage = "Validation failed"
        };

        // Act
        var description = commandEvent.GetDescription();

        // Assert
        description.Should().Contain("failed");
    }

    [Fact]
    public void NavigationEvent_GetDescription_ShouldDescribeTabChange()
    {
        // Arrange
        var navEvent = new NavigationEvent
        {
            NavigationType = NavigationType.TabChanged,
            TabHeader = "Settings"
        };

        // Act
        var description = navEvent.GetDescription();

        // Assert
        description.Should().Contain("Switch to tab");
        description.Should().Contain("Settings");
    }

    [Fact]
    public void WindowEvent_GetDescription_ShouldDescribeWindowAction()
    {
        // Arrange
        var openEvent = new WindowEvent
        {
            WindowEventType = WindowEventType.Opened,
            WindowTitle = "Customer Details"
        };

        var closeEvent = new WindowEvent
        {
            WindowEventType = WindowEventType.Closed,
            WindowTitle = "Customer Details"
        };

        // Act
        var openDescription = openEvent.GetDescription();
        var closeDescription = closeEvent.GetDescription();

        // Assert
        openDescription.Should().Contain("Open");
        openDescription.Should().Contain("Customer Details");
        closeDescription.Should().Contain("Close");
    }
}
