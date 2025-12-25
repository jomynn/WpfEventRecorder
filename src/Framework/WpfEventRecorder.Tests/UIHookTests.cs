using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using WpfEventRecorder.Core.Hooks;
using WpfEventRecorder.Core.Models;
using Xunit;

namespace WpfEventRecorder.Tests
{
    public class UIHookTests
    {
        [Fact]
        public void Start_SetsIsActiveToTrue()
        {
            // Arrange
            using var hook = new UIHook();

            // Act
            hook.Start();

            // Assert
            Assert.True(hook.IsActive);

            // Cleanup
            hook.Stop();
        }

        [Fact]
        public void Stop_SetsIsActiveToFalse()
        {
            // Arrange
            using var hook = new UIHook();
            hook.Start();

            // Act
            hook.Stop();

            // Assert
            Assert.False(hook.IsActive);
        }

        [Fact]
        public void RecordClick_EmitsUIClickEvent()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            hook.Start();

            // Act
            hook.RecordClick("Button", "TestButton", "btnTest", "Click Me", "Main Window");

            // Assert
            Assert.Single(receivedEntries);
            var entry = receivedEntries[0];
            Assert.Equal(RecordEntryType.UIClick, entry.EntryType);
            Assert.NotNull(entry.UIInfo);
            Assert.Equal("Button", entry.UIInfo.ControlType);
            Assert.Equal("TestButton", entry.UIInfo.ControlName);
            Assert.Equal("btnTest", entry.UIInfo.AutomationId);
            Assert.Equal("Click Me", entry.UIInfo.Text);
            Assert.Equal("Main Window", entry.UIInfo.WindowTitle);
        }

        [Fact]
        public void RecordTextInput_EmitsUITextInputEvent()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            hook.Start();

            // Act
            hook.RecordTextInput("TextBox", "NameTextBox", "txtName", "old value", "new value", "Form Window");

            // Assert
            Assert.Single(receivedEntries);
            var entry = receivedEntries[0];
            Assert.Equal(RecordEntryType.UITextInput, entry.EntryType);
            Assert.NotNull(entry.UIInfo);
            Assert.Equal("TextBox", entry.UIInfo.ControlType);
            Assert.Equal("old value", entry.UIInfo.OldValue);
            Assert.Equal("new value", entry.UIInfo.NewValue);
        }

        [Fact]
        public void RecordSelectionChange_EmitsUISelectionChangeEvent()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            hook.Start();

            // Act
            hook.RecordSelectionChange("ComboBox", "CategoryCombo", "cmbCategory", "Option1", "Option2", "Settings");

            // Assert
            Assert.Single(receivedEntries);
            var entry = receivedEntries[0];
            Assert.Equal(RecordEntryType.UISelectionChange, entry.EntryType);
            Assert.NotNull(entry.UIInfo);
            Assert.Equal("ComboBox", entry.UIInfo.ControlType);
            Assert.Equal("Option1", entry.UIInfo.OldValue);
            Assert.Equal("Option2", entry.UIInfo.NewValue);
        }

        [Fact]
        public void RecordToggle_EmitsUIToggleEvent()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            hook.Start();

            // Act
            hook.RecordToggle("CheckBox", "EnableFeature", "chkEnable", false, true, "Options");

            // Assert
            Assert.Single(receivedEntries);
            var entry = receivedEntries[0];
            Assert.Equal(RecordEntryType.UIToggle, entry.EntryType);
            Assert.NotNull(entry.UIInfo);
            Assert.Equal("CheckBox", entry.UIInfo.ControlType);
            Assert.Equal("False", entry.UIInfo.OldValue);
            Assert.Equal("True", entry.UIInfo.NewValue);
        }

        [Fact]
        public void RecordKeyboardShortcut_EmitsUIKeyboardShortcutEvent()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            hook.Start();

            // Act
            hook.RecordKeyboardShortcut("Ctrl+S", "Editor");

            // Assert
            Assert.Single(receivedEntries);
            var entry = receivedEntries[0];
            Assert.Equal(RecordEntryType.UIKeyboardShortcut, entry.EntryType);
            Assert.NotNull(entry.UIInfo);
            Assert.Equal("Keyboard", entry.UIInfo.ControlType);
            Assert.Equal("Ctrl+S", entry.UIInfo.KeyCombination);
        }

        [Fact]
        public void RecordWindowOpen_EmitsUIWindowOpenEvent()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            hook.Start();

            // Act
            hook.RecordWindowOpen("SettingsDialog", "Application Settings");

            // Assert
            Assert.Single(receivedEntries);
            var entry = receivedEntries[0];
            Assert.Equal(RecordEntryType.UIWindowOpen, entry.EntryType);
            Assert.NotNull(entry.UIInfo);
            Assert.Equal("Window", entry.UIInfo.ControlType);
            Assert.Equal("SettingsDialog", entry.UIInfo.WindowType);
            Assert.Equal("Application Settings", entry.UIInfo.WindowTitle);
        }

        [Fact]
        public void RecordWindowClose_EmitsUIWindowCloseEvent()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            hook.Start();

            // Act
            hook.RecordWindowClose("SettingsDialog", "Application Settings");

            // Assert
            Assert.Single(receivedEntries);
            var entry = receivedEntries[0];
            Assert.Equal(RecordEntryType.UIWindowClose, entry.EntryType);
        }

        [Fact]
        public void RecordEvent_WhenNotActive_DoesNotEmit()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            // Note: Not calling hook.Start()

            // Act
            hook.RecordClick("Button", "TestButton", null, "Click", null);

            // Assert
            Assert.Empty(receivedEntries);
        }

        [Fact]
        public void RecordEvent_CustomEntry_EmitsCorrectly()
        {
            // Arrange
            using var hook = new UIHook();
            var receivedEntries = new List<RecordEntry>();
            hook.Events.Subscribe(entry => receivedEntries.Add(entry));
            hook.Start();

            var customEntry = new RecordEntry
            {
                EntryType = RecordEntryType.Custom,
                Metadata = "{\"custom\":\"data\"}"
            };

            // Act
            hook.RecordEvent(customEntry);

            // Assert
            Assert.Single(receivedEntries);
            Assert.Equal(RecordEntryType.Custom, receivedEntries[0].EntryType);
            Assert.Equal("{\"custom\":\"data\"}", receivedEntries[0].Metadata);
        }

        [Fact]
        public void Dispose_StopsRecording()
        {
            // Arrange
            var hook = new UIHook();
            hook.Start();
            Assert.True(hook.IsActive);

            // Act
            hook.Dispose();

            // Assert
            Assert.False(hook.IsActive);
        }
    }
}
