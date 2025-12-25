# WPF Event Recorder - API Reference

## Table of Contents

1. [WpfRecorder Static Class](#wpfrecorder-static-class)
2. [RecordingHub Class](#recordinghub-class)
3. [UIHook Class](#uihook-class)
4. [RecordingHttpHandler Class](#recordinghttphandler-class)
5. [Models](#models)
   - [RecordEntry](#recordentry)
   - [RecordingSession](#recordingsession)
   - [UIInfo](#uiinfo)
   - [ApiInfo](#apiinfo)
   - [RecordEntryType Enum](#recordentrytype-enum)

---

## WpfRecorder Static Class

**Namespace**: `WpfEventRecorder.Core`

The main public API facade for the WPF Event Recorder. Provides simplified access to recording functionality.

### Properties

#### Hub
```csharp
public static RecordingHub Hub { get; }
```
Gets the central recording hub instance.

#### IsRecording
```csharp
public static bool IsRecording { get; }
```
Gets whether recording is currently active.

#### EntryCount
```csharp
public static int EntryCount { get; }
```
Gets the number of entries recorded in the current session.

### Events

#### RecordingStateChanged
```csharp
public static event EventHandler<bool>? RecordingStateChanged;
```
Raised when recording starts or stops. The event argument is `true` when recording starts, `false` when it stops.

#### EntryRecorded
```csharp
public static event EventHandler<RecordEntry>? EntryRecorded;
```
Raised when a new entry is recorded.

### Methods

#### Initialize
```csharp
public static void Initialize()
```
Initializes the recorder with default settings. Must be called before recording. Safe to call multiple times.

#### Start
```csharp
public static void Start(string? sessionName = null)
```
Starts a new recording session.

**Parameters:**
- `sessionName`: Optional name for the session. Defaults to timestamp-based name.

#### Stop
```csharp
public static void Stop()
```
Stops the current recording session.

#### Clear
```csharp
public static void Clear()
```
Clears all recorded entries from the current session.

#### SaveToFile
```csharp
public static void SaveToFile(string filePath)
```
Saves the current recording session to a JSON file.

**Parameters:**
- `filePath`: Full path to the output file.

**Exceptions:**
- `IOException`: If the file cannot be written.

#### ExportAsJson
```csharp
public static string ExportAsJson()
```
Exports the current recording session as a JSON string.

**Returns:** JSON string representation of the session.

#### CreateHttpClient
```csharp
public static HttpClient CreateHttpClient()
```
Creates an HttpClient configured with recording capabilities.

**Returns:** A new HttpClient instance with recording enabled.

#### CreateHttpHandler
```csharp
public static RecordingHttpHandler CreateHttpHandler(HttpMessageHandler? innerHandler = null)
```
Creates a recording HTTP handler for custom HttpClient configuration.

**Parameters:**
- `innerHandler`: Optional inner handler to wrap. Defaults to HttpClientHandler.

**Returns:** A configured RecordingHttpHandler instance.

#### RecordClick
```csharp
public static void RecordClick(string controlType, string? controlName, string? text = null)
```
Records a UI click event manually.

**Parameters:**
- `controlType`: Type of control (e.g., "Button", "MenuItem")
- `controlName`: Name of the control (x:Name in XAML)
- `text`: Optional text content of the control

#### RecordTextInput
```csharp
public static void RecordTextInput(string controlType, string? controlName, string? oldValue, string? newValue)
```
Records a text input event manually.

**Parameters:**
- `controlType`: Type of control (e.g., "TextBox")
- `controlName`: Name of the control
- `oldValue`: Previous text value
- `newValue`: New text value

#### RecordCustomEvent
```csharp
public static void RecordCustomEvent(string eventType, string? data = null)
```
Records a custom event.

**Parameters:**
- `eventType`: Type identifier for the custom event
- `data`: Optional JSON data associated with the event

#### SetCorrelationId
```csharp
public static void SetCorrelationId(string correlationId)
```
Sets the current correlation ID for linking events.

**Parameters:**
- `correlationId`: The correlation ID to use

#### NewCorrelationId
```csharp
public static string NewCorrelationId()
```
Generates and sets a new correlation ID.

**Returns:** The newly generated correlation ID.

---

## RecordingHub Class

**Namespace**: `WpfEventRecorder.Core.Services`

Central hub for managing recording sessions and coordinating hooks. Implements the Singleton pattern.

### Properties

#### Instance
```csharp
public static RecordingHub Instance { get; }
```
Gets the singleton instance of the recording hub.

#### Entries
```csharp
public IObservable<RecordEntry> Entries { get; }
```
Observable stream of all recorded entries. Useful for reactive programming patterns.

#### IsRecording
```csharp
public bool IsRecording { get; }
```
Gets whether recording is currently active.

#### EntryCount
```csharp
public int EntryCount { get; }
```
Gets the number of entries in the current session.

#### CurrentSession
```csharp
public RecordingSession? CurrentSession { get; }
```
Gets the current recording session, or null if not recording.

### Events

#### RecordingStateChanged
```csharp
public event EventHandler<bool>? RecordingStateChanged;
```
Raised when recording state changes.

#### EntryRecorded
```csharp
public event EventHandler<RecordEntry>? EntryRecorded;
```
Raised when a new entry is recorded.

### Methods

#### SetUIHook
```csharp
public void SetUIHook(UIHook hook)
```
Configures the UI hook for capturing UI events.

**Parameters:**
- `hook`: The UIHook instance to use.

**Exceptions:**
- `ArgumentNullException`: If hook is null.

#### SetHttpHandler
```csharp
public void SetHttpHandler(RecordingHttpHandler handler)
```
Configures the HTTP handler for capturing API calls.

**Parameters:**
- `handler`: The RecordingHttpHandler instance to use.

**Exceptions:**
- `ArgumentNullException`: If handler is null.

#### CreateHttpHandler
```csharp
public RecordingHttpHandler CreateHttpHandler()
```
Creates and configures a new HTTP handler.

**Returns:** A new RecordingHttpHandler registered with the hub.

#### Start
```csharp
public void Start(string? sessionName = null)
```
Starts a new recording session.

**Parameters:**
- `sessionName`: Optional session name.

#### Stop
```csharp
public void Stop()
```
Stops the current recording session.

#### Clear
```csharp
public void Clear()
```
Clears all recorded entries.

#### GetEntries
```csharp
public IReadOnlyList<RecordEntry> GetEntries()
```
Gets a copy of all recorded entries.

**Returns:** Read-only list of recorded entries.

#### AddEntry
```csharp
public void AddEntry(RecordEntry entry)
```
Adds a custom entry to the recording.

**Parameters:**
- `entry`: The entry to add.

**Exceptions:**
- `ArgumentNullException`: If entry is null.

#### SetCorrelationId
```csharp
public void SetCorrelationId(string correlationId)
```
Sets the current correlation ID.

**Parameters:**
- `correlationId`: The correlation ID to use.

#### NewCorrelationId
```csharp
public string NewCorrelationId()
```
Generates a new correlation ID and sets it as current.

**Returns:** The new correlation ID.

#### SaveToFile
```csharp
public void SaveToFile(string filePath)
```
Saves the current session to a JSON file.

**Parameters:**
- `filePath`: Path to the output file.

#### LoadFromFile
```csharp
public RecordingSession LoadFromFile(string filePath)
```
Loads a session from a JSON file.

**Parameters:**
- `filePath`: Path to the input file.

**Returns:** The loaded RecordingSession.

**Exceptions:**
- `InvalidOperationException`: If deserialization fails.

#### ExportAsJson
```csharp
public string ExportAsJson()
```
Exports the current session as a JSON string.

**Returns:** JSON representation of the session.

#### Dispose
```csharp
public void Dispose()
```
Disposes resources and stops recording.

---

## UIHook Class

**Namespace**: `WpfEventRecorder.Core.Hooks`

Hooks into WPF UI events and captures interactions.

### Properties

#### Events
```csharp
public IObservable<RecordEntry> Events { get; }
```
Observable stream of UI events.

#### IsActive
```csharp
public bool IsActive { get; }
```
Gets whether the hook is currently capturing events.

### Methods

#### Start
```csharp
public void Start()
```
Starts capturing UI events.

#### Stop
```csharp
public void Stop()
```
Stops capturing UI events.

#### RecordEvent
```csharp
public void RecordEvent(RecordEntry entry)
```
Records a custom UI event.

**Parameters:**
- `entry`: The entry to record.

#### RecordClick
```csharp
public void RecordClick(
    string controlType,
    string? controlName,
    string? automationId,
    string? text,
    string? windowTitle,
    ScreenPoint? position = null)
```
Records a click event.

**Parameters:**
- `controlType`: Type of control clicked
- `controlName`: Name of the control
- `automationId`: AutomationId property value
- `text`: Text content of the control
- `windowTitle`: Title of the parent window
- `position`: Optional screen coordinates

#### RecordTextInput
```csharp
public void RecordTextInput(
    string controlType,
    string? controlName,
    string? automationId,
    string? oldValue,
    string? newValue,
    string? windowTitle)
```
Records a text input event.

#### RecordSelectionChange
```csharp
public void RecordSelectionChange(
    string controlType,
    string? controlName,
    string? automationId,
    string? oldValue,
    string? newValue,
    string? windowTitle)
```
Records a selection change event.

#### RecordToggle
```csharp
public void RecordToggle(
    string controlType,
    string? controlName,
    string? automationId,
    bool oldValue,
    bool newValue,
    string? windowTitle)
```
Records a toggle (checkbox/radio) event.

#### RecordKeyboardShortcut
```csharp
public void RecordKeyboardShortcut(string keyCombination, string? windowTitle)
```
Records a keyboard shortcut event.

**Parameters:**
- `keyCombination`: Key combination string (e.g., "Ctrl+S")
- `windowTitle`: Title of the active window

#### RecordWindowOpen
```csharp
public void RecordWindowOpen(string windowType, string? windowTitle)
```
Records a window open event.

#### RecordWindowClose
```csharp
public void RecordWindowClose(string windowType, string? windowTitle)
```
Records a window close event.

#### Dispose
```csharp
public void Dispose()
```
Disposes resources and stops capturing.

---

## RecordingHttpHandler Class

**Namespace**: `WpfEventRecorder.Core.Hooks`

HTTP message handler that records API calls. Extends `DelegatingHandler`.

### Constructors

```csharp
public RecordingHttpHandler(
    HttpMessageHandler? innerHandler = null,
    bool captureRequestBody = true,
    bool captureResponseBody = true,
    int maxBodySize = 1048576)
```

**Parameters:**
- `innerHandler`: Inner handler to delegate to. Defaults to HttpClientHandler.
- `captureRequestBody`: Whether to capture request bodies. Default: true.
- `captureResponseBody`: Whether to capture response bodies. Default: true.
- `maxBodySize`: Maximum body size to capture in bytes. Default: 1MB.

### Properties

#### Requests
```csharp
public IObservable<RecordEntry> Requests { get; }
```
Observable stream of API request events.

#### Responses
```csharp
public IObservable<RecordEntry> Responses { get; }
```
Observable stream of API response events.

#### AllEvents
```csharp
public IObservable<RecordEntry> AllEvents { get; }
```
Combined observable of all API events (requests and responses).

#### IsActive
```csharp
public bool IsActive { get; set; }
```
Gets or sets whether recording is active. When false, requests pass through without recording.

---

## Models

### RecordEntry

**Namespace**: `WpfEventRecorder.Core.Models`

Represents a single recorded event.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique identifier for this entry |
| `Timestamp` | `DateTime` | When the event occurred (UTC) |
| `EntryType` | `RecordEntryType` | Type of the recorded event |
| `UIInfo` | `UIInfo?` | UI interaction info (null for API entries) |
| `ApiInfo` | `ApiInfo?` | API call info (null for UI entries) |
| `CorrelationId` | `string?` | ID for linking related events |
| `DurationMs` | `long?` | Duration in milliseconds (for API calls) |
| `Metadata` | `string?` | Additional JSON metadata |

---

### RecordingSession

**Namespace**: `WpfEventRecorder.Core.Models`

Represents a complete recording session.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `SessionId` | `Guid` | Unique session identifier |
| `Name` | `string` | Session name/description |
| `StartTime` | `DateTime` | When recording started |
| `EndTime` | `DateTime?` | When recording ended |
| `Duration` | `TimeSpan?` | Total duration (computed) |
| `ApplicationName` | `string?` | Application being recorded |
| `ApplicationVersion` | `string?` | Application version |
| `MachineName` | `string?` | Machine name |
| `OSVersion` | `string?` | OS version |
| `UserName` | `string?` | User who recorded |
| `Entries` | `List<RecordEntry>` | All recorded entries |
| `Metadata` | `Dictionary<string, string>?` | Additional metadata |
| `SchemaVersion` | `string` | Schema version for compatibility |

#### Static Methods

```csharp
public static RecordingSession Create(string name)
```
Creates a new session with current environment info.

---

### UIInfo

**Namespace**: `WpfEventRecorder.Core.Models`

Information about a UI element interaction.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ControlType` | `string` | Type of control (Button, TextBox, etc.) |
| `ControlName` | `string?` | Name of the control (x:Name) |
| `AutomationId` | `string?` | AutomationId for UI automation |
| `Text` | `string?` | Text content of the control |
| `OldValue` | `string?` | Value before interaction |
| `NewValue` | `string?` | Value after interaction |
| `VisualTreePath` | `string?` | Full path in visual tree |
| `WindowTitle` | `string?` | Parent window title |
| `WindowType` | `string?` | Parent window type name |
| `ScreenPosition` | `ScreenPoint?` | Screen coordinates |
| `RelativePosition` | `RelativePoint?` | Position within control (0-1) |
| `KeyCombination` | `string?` | Key combination for keyboard events |
| `Properties` | `Dictionary<string, string>?` | Additional properties |
| `Selector` | `string?` | CSS-like selector |

---

### ApiInfo

**Namespace**: `WpfEventRecorder.Core.Models`

Information about an HTTP API call.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Method` | `string` | HTTP method (GET, POST, etc.) |
| `Url` | `string` | Full request URL |
| `Path` | `string?` | URL path without query string |
| `QueryParameters` | `Dictionary<string, string>?` | Query string parameters |
| `RequestHeaders` | `Dictionary<string, string>?` | Request headers |
| `RequestBody` | `string?` | Request body content |
| `RequestContentType` | `string?` | Request content type |
| `StatusCode` | `int?` | HTTP status code |
| `ResponseHeaders` | `Dictionary<string, string>?` | Response headers |
| `ResponseBody` | `string?` | Response body content |
| `ResponseContentType` | `string?` | Response content type |
| `IsSuccess` | `bool` | Whether request succeeded (2xx) |
| `ErrorMessage` | `string?` | Error message if failed |
| `RequestTimestamp` | `DateTime` | When request was sent |
| `ResponseTimestamp` | `DateTime?` | When response was received |

---

### RecordEntryType Enum

**Namespace**: `WpfEventRecorder.Core.Models`

Types of recorded events.

| Value | Description |
|-------|-------------|
| `UIClick` | UI click event |
| `UIDoubleClick` | UI double-click event |
| `UITextInput` | Text input event |
| `UISelectionChange` | Selection change event |
| `UIToggle` | Checkbox/toggle change |
| `UIKeyboardShortcut` | Keyboard shortcut |
| `UIDragDrop` | Drag and drop operation |
| `UIWindowOpen` | Window opened |
| `UIWindowClose` | Window closed |
| `ApiRequest` | HTTP API request |
| `ApiResponse` | HTTP API response |
| `Custom` | Custom event |

---

### ScreenPoint

**Namespace**: `WpfEventRecorder.Core.Models`

Screen coordinates.

| Property | Type | Description |
|----------|------|-------------|
| `X` | `double` | X coordinate |
| `Y` | `double` | Y coordinate |

---

### RelativePoint

**Namespace**: `WpfEventRecorder.Core.Models`

Relative position within a control (0-1 range).

| Property | Type | Description |
|----------|------|-------------|
| `X` | `double` | X position (0-1) |
| `Y` | `double` | Y position (0-1) |
