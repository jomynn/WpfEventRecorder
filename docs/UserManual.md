# WPF Event Recorder - User Manual

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Getting Started](#getting-started)
4. [Recording Events](#recording-events)
5. [Using the Event Viewer](#using-the-event-viewer)
6. [Saving and Exporting](#saving-and-exporting)
7. [Keyboard Shortcuts](#keyboard-shortcuts)
8. [Understanding Recordings](#understanding-recordings)
9. [Troubleshooting](#troubleshooting)
10. [FAQ](#faq)

---

## Introduction

WPF Event Recorder is a Visual Studio 2022 extension designed to capture and record user interactions within WPF applications. It's an essential tool for:

- **Test Automation**: Generate test scripts based on recorded user actions
- **Bug Reproduction**: Capture exact steps to reproduce issues
- **Documentation**: Record user workflows for training materials
- **Performance Analysis**: Track API calls and their response times

### What Gets Recorded

| Category | Events |
|----------|--------|
| **Mouse Actions** | Clicks, double-clicks, drag-and-drop |
| **Keyboard** | Text input, keyboard shortcuts |
| **Controls** | Selection changes, checkbox toggles, button presses |
| **Windows** | Window open/close events |
| **HTTP API** | Requests, responses, headers, body content |

---

## Installation

### Prerequisites

- Visual Studio 2022 (Community, Professional, or Enterprise)
- .NET Framework 4.8 or later
- Windows 10/11

### Installing from VSIX

1. Download the `WpfEventRecorder.vsix` file
2. Close all Visual Studio instances
3. Double-click the VSIX file
4. Follow the installation wizard
5. Restart Visual Studio

### Installing from Visual Studio Marketplace

1. Open Visual Studio 2022
2. Go to **Extensions > Manage Extensions**
3. Search for "WPF Event Recorder"
4. Click **Download**
5. Restart Visual Studio when prompted

### Verifying Installation

After installation, verify the extension is active:

1. Open Visual Studio
2. Go to **Tools** menu
3. Look for **WPF Event Recorder** submenu
4. Check for the **WPF Event Recorder** toolbar

---

## Getting Started

### Enabling the Toolbar

If the toolbar isn't visible:

1. Go to **View > Toolbars**
2. Check **WPF Event Recorder**

The toolbar contains five buttons:

| Button | Description |
|--------|-------------|
| ▶ Start | Begin recording events |
| ⏹ Stop | Stop recording |
| 💾 Save | Save recording to file |
| 🗑 Clear | Clear all recorded events |
| 📋 Event Viewer | Open the tool window |

### Opening the Event Viewer

The Event Viewer window shows recorded events in real-time:

1. Click **Event Viewer** on the toolbar, or
2. Go to **Tools > WPF Event Recorder > Open Event Viewer**, or
3. Go to **View > Other Windows > WPF Event Recorder**

---

## Recording Events

### Starting a Recording Session

1. Open your WPF solution in Visual Studio
2. Click **Start Recording** (or press `Ctrl+Alt+R`)
3. The status indicator turns red and shows "Recording..."
4. Run your WPF application (F5 or Ctrl+F5)
5. Interact with your application normally

### What Happens During Recording

- All UI interactions are captured automatically
- HTTP requests made through instrumented HttpClient are recorded
- Events appear in real-time in the Event Viewer
- The event counter updates as new events are captured

### Stopping a Recording Session

1. Click **Stop Recording** (or press `Ctrl+Alt+S`)
2. The status indicator shows "Stopped"
3. The status bar displays the total number of captured events

### Best Practices

- **Close unnecessary windows** before recording to reduce noise
- **Plan your test scenario** before starting to capture clean recordings
- **Use meaningful control names** in your XAML for better event identification
- **Set AutomationId** properties on important controls

---

## Using the Event Viewer

### Event List

The main area displays recorded events with the following columns:

| Column | Description |
|--------|-------------|
| **Time** | Timestamp when the event occurred (HH:mm:ss.fff) |
| **Type** | Event type icon/label |
| **Summary** | Brief description of the event |
| **Duration** | Response time for API calls (milliseconds) |

### Event Details Panel

Selecting an event shows detailed information:

**For UI Events:**
- Control type and name
- AutomationId
- Text content
- Window title
- Old and new values (for input events)

**For API Events:**
- HTTP method and URL
- Status code
- Request/response headers
- Request/response body

### Filtering and Navigation

- **Click** an event to view its details
- **Scroll** through the list to navigate history
- Events auto-scroll to show the latest during recording
- Failed API calls are highlighted in red
- API events appear in italics

---

## Saving and Exporting

### Saving to JSON File

1. Click **Save Recording** (or press `Ctrl+Alt+E`)
2. Choose a location and filename
3. Click **Save**

The default filename format is: `recording_YYYYMMDD_HHmmss.json`

### JSON File Structure

```json
{
  "sessionId": "unique-session-id",
  "name": "Session Name",
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": "2024-01-15T10:35:00Z",
  "duration": "00:05:00",
  "machineName": "WORKSTATION",
  "userName": "Developer",
  "schemaVersion": "1.0",
  "entries": [...]
}
```

### Using Recordings

Saved recordings can be used for:

- **Test Generation**: Parse JSON to create automated UI tests
- **Documentation**: Convert to step-by-step instructions
- **Analysis**: Review API performance and error patterns
- **Debugging**: Share exact reproduction steps with team members

---

## Keyboard Shortcuts

| Action | Shortcut | Description |
|--------|----------|-------------|
| Start Recording | `Ctrl+Alt+R` | Begin capturing events |
| Stop Recording | `Ctrl+Alt+S` | Stop capturing events |
| Save Recording | `Ctrl+Alt+E` | Export to JSON file |

### Customizing Shortcuts

1. Go to **Tools > Options**
2. Navigate to **Environment > Keyboard**
3. Search for "WpfRecorder"
4. Select a command and assign a new shortcut

---

## Understanding Recordings

### Event Types Explained

#### UI Click (`UIClick`)
Triggered when a user clicks on a control.

```json
{
  "entryType": "UIClick",
  "uiInfo": {
    "controlType": "Button",
    "controlName": "SubmitButton",
    "text": "Submit",
    "windowTitle": "Order Form"
  }
}
```

#### Text Input (`UITextInput`)
Triggered when text is entered or modified.

```json
{
  "entryType": "UITextInput",
  "uiInfo": {
    "controlType": "TextBox",
    "controlName": "EmailTextBox",
    "oldValue": "",
    "newValue": "user@example.com"
  }
}
```

#### Selection Change (`UISelectionChange`)
Triggered when a selection changes in ComboBox, ListBox, etc.

```json
{
  "entryType": "UISelectionChange",
  "uiInfo": {
    "controlType": "ComboBox",
    "controlName": "CategoryCombo",
    "oldValue": "Option A",
    "newValue": "Option B"
  }
}
```

#### API Request/Response
Paired events for HTTP communications.

```json
{
  "entryType": "ApiRequest",
  "correlationId": "abc-123",
  "apiInfo": {
    "method": "POST",
    "url": "https://api.example.com/orders",
    "requestBody": "{\"item\":\"Widget\"}"
  }
}
```

### Correlation IDs

Correlation IDs link related events together:

- UI actions that trigger API calls share the same correlation ID
- Request/Response pairs have matching correlation IDs
- Use correlation IDs to trace cause-and-effect relationships

---

## Troubleshooting

### Toolbar Not Visible

**Problem**: The WPF Event Recorder toolbar doesn't appear.

**Solution**:
1. Go to **View > Toolbars**
2. Enable **WPF Event Recorder**
3. If not listed, verify the extension is installed via **Extensions > Manage Extensions**

### Events Not Being Recorded

**Problem**: Interactions aren't captured during recording.

**Solutions**:
- Ensure recording is started (red indicator visible)
- Verify your app uses the recording-enabled HttpClient for API calls
- Check that UI controls have proper names or AutomationIds
- Ensure the app is running in the same VS instance

### HTTP Calls Not Recorded

**Problem**: API requests don't appear in recordings.

**Solution**:
Your application must use the recording-enabled HttpClient:

```csharp
// Use this instead of new HttpClient()
var client = WpfRecorder.CreateHttpClient();
```

### Extension Not Loading

**Problem**: Extension doesn't appear after installation.

**Solutions**:
1. Check **Extensions > Manage Extensions** for errors
2. Try **Help > About** and verify the extension is listed
3. Reset VS settings: `devenv /resetsettings`
4. Repair VS installation if needed

### Performance Issues

**Problem**: Recording slows down the application.

**Solutions**:
- Disable response body capture for large payloads
- Clear recordings periodically during long sessions
- Close the Event Viewer if not actively monitoring

---

## FAQ

### Q: Can I record multiple applications simultaneously?

**A**: The recorder captures events from any WPF application that's instrumented with the recording library. Each application's events are captured in the same session.

### Q: How large can recordings get?

**A**: Recording size depends on the number of events and API body sizes. Large API responses are truncated at 1MB by default. For long sessions, save and clear periodically.

### Q: Can I edit recordings?

**A**: Recordings are saved as JSON files and can be edited with any text editor. However, be careful to maintain valid JSON structure.

### Q: Does recording affect application performance?

**A**: The recorder is designed to be lightweight. UI event capture has minimal overhead. HTTP recording may add slight latency due to body capture.

### Q: Can I use recordings with test frameworks?

**A**: Yes! The JSON format is designed to be parsed easily. You can create converters for popular frameworks like:
- Selenium
- Appium
- WinAppDriver
- Coded UI Tests

### Q: Is sensitive data captured?

**A**: Yes, recordings capture all data including passwords and API keys. Handle recording files securely and avoid committing them to source control.

### Q: Can I filter what gets recorded?

**A**: Currently, all events are recorded. Filtering can be applied when processing the JSON output. Future versions may include recording filters.

---

## Support

For issues and feature requests, please visit the GitHub repository or contact support.

**Resources**:
- [GitHub Repository](https://github.com/yourname/WpfEventRecorder)
- [Issue Tracker](https://github.com/yourname/WpfEventRecorder/issues)
- [Release Notes](https://github.com/yourname/WpfEventRecorder/releases)
