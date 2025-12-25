using System;
using System.Threading;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;
using Xunit;

namespace WpfEventRecorder.Tests
{
    public class RecordingHubTests
    {
        [Fact]
        public void Instance_ReturnsSameInstance()
        {
            // Arrange & Act
            var instance1 = RecordingHub.Instance;
            var instance2 = RecordingHub.Instance;

            // Assert
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void Start_SetsIsRecordingToTrue()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Clear();

            // Act
            hub.Start();

            // Assert
            Assert.True(hub.IsRecording);

            // Cleanup
            hub.Stop();
        }

        [Fact]
        public void Stop_SetsIsRecordingToFalse()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Start();

            // Act
            hub.Stop();

            // Assert
            Assert.False(hub.IsRecording);
        }

        [Fact]
        public void AddEntry_IncreasesEntryCount()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Clear();
            hub.Start();
            var initialCount = hub.EntryCount;

            // Act
            hub.AddEntry(new RecordEntry
            {
                EntryType = RecordEntryType.Custom,
                Metadata = "Test entry"
            });

            // Assert
            Assert.Equal(initialCount + 1, hub.EntryCount);

            // Cleanup
            hub.Stop();
            hub.Clear();
        }

        [Fact]
        public void Clear_ResetsEntryCount()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Start();
            hub.AddEntry(new RecordEntry { EntryType = RecordEntryType.Custom });
            hub.AddEntry(new RecordEntry { EntryType = RecordEntryType.Custom });
            hub.Stop();

            // Act
            hub.Clear();

            // Assert
            Assert.Equal(0, hub.EntryCount);
        }

        [Fact]
        public void Start_CreatesNewSession()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Clear();
            var sessionName = "Test Session";

            // Act
            hub.Start(sessionName);

            // Assert
            Assert.NotNull(hub.CurrentSession);
            Assert.Equal(sessionName, hub.CurrentSession.Name);

            // Cleanup
            hub.Stop();
        }

        [Fact]
        public void Stop_SetsSessionEndTime()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Clear();
            hub.Start();

            // Act
            hub.Stop();

            // Assert
            Assert.NotNull(hub.CurrentSession?.EndTime);
        }

        [Fact]
        public void GetEntries_ReturnsAllEntries()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Clear();
            hub.Start();

            var entry1 = new RecordEntry { EntryType = RecordEntryType.UIClick };
            var entry2 = new RecordEntry { EntryType = RecordEntryType.ApiRequest };
            hub.AddEntry(entry1);
            hub.AddEntry(entry2);

            // Act
            var entries = hub.GetEntries();

            // Assert
            Assert.Equal(2, entries.Count);
            Assert.Contains(entries, e => e.Id == entry1.Id);
            Assert.Contains(entries, e => e.Id == entry2.Id);

            // Cleanup
            hub.Stop();
            hub.Clear();
        }

        [Fact]
        public void NewCorrelationId_ReturnsUniqueId()
        {
            // Arrange
            var hub = RecordingHub.Instance;

            // Act
            var id1 = hub.NewCorrelationId();
            var id2 = hub.NewCorrelationId();

            // Assert
            Assert.NotNull(id1);
            Assert.NotNull(id2);
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void RecordingStateChanged_FiresOnStart()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Clear();
            var eventFired = false;
            var recordingState = false;

            hub.RecordingStateChanged += (sender, isRecording) =>
            {
                eventFired = true;
                recordingState = isRecording;
            };

            // Act
            hub.Start();

            // Assert
            Assert.True(eventFired);
            Assert.True(recordingState);

            // Cleanup
            hub.Stop();
        }

        [Fact]
        public void EntryRecorded_FiresOnAddEntry()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Clear();
            hub.Start();

            RecordEntry? recordedEntry = null;
            hub.EntryRecorded += (sender, entry) =>
            {
                recordedEntry = entry;
            };

            var testEntry = new RecordEntry
            {
                EntryType = RecordEntryType.Custom,
                Metadata = "Test"
            };

            // Act
            hub.AddEntry(testEntry);

            // Assert
            Assert.NotNull(recordedEntry);
            Assert.Equal(testEntry.Id, recordedEntry.Id);

            // Cleanup
            hub.Stop();
            hub.Clear();
        }

        [Fact]
        public void ExportAsJson_ReturnsValidJson()
        {
            // Arrange
            var hub = RecordingHub.Instance;
            hub.Clear();
            hub.Start("Export Test");
            hub.AddEntry(new RecordEntry
            {
                EntryType = RecordEntryType.UIClick,
                UIInfo = new UIInfo
                {
                    ControlType = "Button",
                    ControlName = "TestButton"
                }
            });
            hub.Stop();

            // Act
            var json = hub.ExportAsJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Export Test", json);
            Assert.Contains("UIClick", json);
            Assert.Contains("TestButton", json);

            // Cleanup
            hub.Clear();
        }
    }
}
