# WPF Event Recorder - Architecture Document

## Table of Contents

1. [System Overview](#system-overview)
2. [Component Architecture](#component-architecture)
3. [Data Flow](#data-flow)
4. [Class Diagrams](#class-diagrams)
5. [Design Patterns](#design-patterns)
6. [Extension Points](#extension-points)
7. [Threading Model](#threading-model)
8. [Serialization](#serialization)
9. [Security Considerations](#security-considerations)
10. [Performance Considerations](#performance-considerations)

---

## System Overview

WPF Event Recorder is designed as a modular, extensible system for capturing and recording user interactions in WPF applications. The architecture follows separation of concerns principles with three main layers:

```
┌─────────────────────────────────────────────────────────────────┐
│                    Visual Studio Extension                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │   Commands   │  │  ToolWindow  │  │   Package/Manifest   │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                        Core Library                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │   Services   │  │    Hooks     │  │       Models         │  │
│  │ RecordingHub │  │   UIHook     │  │    RecordEntry       │  │
│  │              │  │  HttpHandler │  │  RecordingSession    │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                     Target Application                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  WPF Controls│  │  HttpClient  │  │   Custom Events      │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Design Goals

1. **Non-intrusive**: Minimal impact on target application performance
2. **Extensible**: Easy to add new event types and hooks
3. **Decoupled**: Core library independent of VS extension
4. **Observable**: Real-time event streaming using Reactive Extensions
5. **Portable**: Core library targets .NET Standard 2.0

---

## Component Architecture

### Project Dependencies

```
WpfEventRecorder.sln
│
├── WpfEventRecorder (VSIX)
│   └── References: WpfEventRecorder.Core
│                   Microsoft.VisualStudio.SDK
│                   System.Reactive
│
├── WpfEventRecorder.Core (Library)
│   └── References: System.Reactive
│                   System.Text.Json
│
├── WpfEventRecorder.SampleApp (WPF App)
│   └── References: WpfEventRecorder.Core
│
└── WpfEventRecorder.Tests (Unit Tests)
    └── References: WpfEventRecorder.Core
                    xUnit, Moq
```

### Core Library Components

#### 1. Models Layer

```
Models/
├── RecordEntry.cs        # Base event container
├── RecordingSession.cs   # Session metadata + entries
├── UIInfo.cs             # UI event details
├── ApiInfo.cs            # HTTP event details
└── RecordEntryType.cs    # Event type enumeration
```

**Responsibilities:**
- Define data structures for recorded events
- Provide serialization attributes
- Immutable after creation

#### 2. Hooks Layer

```
Hooks/
├── UIHook.cs              # UI event capture
└── RecordingHttpHandler.cs # HTTP interception
```

**Responsibilities:**
- Intercept and capture events
- Transform raw events into RecordEntry objects
- Publish events to observers

#### 3. Services Layer

```
Services/
└── RecordingHub.cs        # Central coordinator
```

**Responsibilities:**
- Coordinate all hooks
- Manage recording sessions
- Aggregate events from multiple sources
- Handle persistence

#### 4. Public API

```
WpfRecorder.cs             # Static facade
```

**Responsibilities:**
- Provide simplified API
- Hide internal complexity
- Ensure proper initialization

### VSIX Extension Components

#### 1. Package

```
WpfEventRecorderPackage.cs  # VS package entry point
```

**Responsibilities:**
- Initialize extension
- Register commands and tool windows
- Manage VS integration lifecycle

#### 2. Commands

```
Commands/
├── RecorderCommandSet.vsct  # Command definitions
├── CommandIds.cs            # ID constants
├── StartRecordingCommand.cs
├── StopRecordingCommand.cs
├── SaveRecordingCommand.cs
├── ClearRecordingCommand.cs
└── OpenToolWindowCommand.cs
```

**Responsibilities:**
- Handle toolbar/menu commands
- Update command state (enabled/disabled)
- Execute recording operations

#### 3. Tool Windows

```
ToolWindows/
├── RecorderToolWindow.cs           # Tool window host
├── RecorderToolWindowControl.xaml  # UI definition
├── RecorderToolWindowControl.xaml.cs
└── RecordEntryViewModel.cs         # View model
```

**Responsibilities:**
- Display recorded events
- Provide real-time updates
- Enable event inspection

---

## Data Flow

### Recording Flow

```
User Interaction
       │
       ▼
┌─────────────────┐
│  Target App     │
│  (WPF/HTTP)     │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌───────┐ ┌───────────────┐
│UIHook │ │RecordingHttp  │
│       │ │Handler        │
└───┬───┘ └───────┬───────┘
    │             │
    ▼             ▼
┌─────────────────────────┐
│     RecordingHub        │
│  ┌──────────────────┐   │
│  │ List<RecordEntry>│   │
│  └──────────────────┘   │
│  ┌──────────────────┐   │
│  │ Subject<Entry>   │   │
│  └──────────────────┘   │
└───────────┬─────────────┘
            │
    ┌───────┴───────┐
    │               │
    ▼               ▼
┌───────────┐ ┌───────────┐
│ Observers │ │ Tool      │
│ (Rx)      │ │ Window    │
└───────────┘ └───────────┘
```

### Event Publication

```csharp
// Hook captures event
var entry = new RecordEntry { ... };

// Publish to Subject (async)
_eventSubject.OnNext(entry);

// Hub receives and stores
hub.AddEntry(entry);

// Hub republishes
_entriesSubject.OnNext(entry);

// Events fire
EntryRecorded?.Invoke(this, entry);

// Observers receive
subscription.OnNext(entry);
```

### HTTP Request/Response Correlation

```
HTTP Request                         HTTP Response
     │                                     │
     ▼                                     ▼
┌─────────────┐                    ┌─────────────┐
│ Create      │                    │ Create      │
│ CorrelationId                    │ Use Same    │
│ = Guid.New()│                    │ CorrelationId
└──────┬──────┘                    └──────┬──────┘
       │                                  │
       ▼                                  ▼
┌─────────────────────────────────────────────┐
│            RecordingHub                      │
│  ┌─────────────────────────────────────┐    │
│  │ Entries with matching CorrelationId │    │
│  │ can be linked for analysis          │    │
│  └─────────────────────────────────────┘    │
└─────────────────────────────────────────────┘
```

---

## Class Diagrams

### Core Classes

```
┌─────────────────────────────────────┐
│           RecordingHub              │
├─────────────────────────────────────┤
│ - _instance: Lazy<RecordingHub>     │
│ - _entries: List<RecordEntry>       │
│ - _entriesSubject: Subject<Entry>   │
│ - _uiHook: UIHook                   │
│ - _httpHandler: RecordingHttpHandler│
│ - _currentSession: RecordingSession │
│ - _isRecording: bool                │
├─────────────────────────────────────┤
│ + Instance: RecordingHub {get}      │
│ + Entries: IObservable<RecordEntry> │
│ + IsRecording: bool                 │
│ + EntryCount: int                   │
│ + CurrentSession: RecordingSession  │
├─────────────────────────────────────┤
│ + Start(sessionName?)               │
│ + Stop()                            │
│ + Clear()                           │
│ + AddEntry(entry)                   │
│ + GetEntries(): IReadOnlyList       │
│ + SaveToFile(path)                  │
│ + LoadFromFile(path)                │
│ + ExportAsJson(): string            │
│ + SetUIHook(hook)                   │
│ + SetHttpHandler(handler)           │
│ + CreateHttpHandler()               │
└─────────────────────────────────────┘
            △
            │ uses
┌───────────┴───────────┐
│                       │
▼                       ▼
┌─────────────────┐   ┌──────────────────────┐
│     UIHook      │   │ RecordingHttpHandler │
├─────────────────┤   ├──────────────────────┤
│ - _eventSubject │   │ - _requestSubject    │
│ - _isActive     │   │ - _responseSubject   │
├─────────────────┤   │ - _isActive          │
│ + Events        │   │ - _captureBody       │
│ + IsActive      │   ├──────────────────────┤
│ + Start()       │   │ + Requests           │
│ + Stop()        │   │ + Responses          │
│ + RecordClick() │   │ + AllEvents          │
│ + RecordText()  │   │ + IsActive           │
│ + ...           │   │ # SendAsync()        │
└─────────────────┘   └──────────────────────┘
```

### Model Classes

```
┌─────────────────────────────────────┐
│          RecordingSession           │
├─────────────────────────────────────┤
│ + SessionId: Guid                   │
│ + Name: string                      │
│ + StartTime: DateTime               │
│ + EndTime: DateTime?                │
│ + Duration: TimeSpan? {computed}    │
│ + ApplicationName: string           │
│ + MachineName: string               │
│ + Entries: List<RecordEntry>        │
│ + SchemaVersion: string             │
├─────────────────────────────────────┤
│ + Create(name): RecordingSession    │
└─────────────────────────────────────┘
           │
           │ contains
           ▼
┌─────────────────────────────────────┐
│           RecordEntry               │
├─────────────────────────────────────┤
│ + Id: Guid                          │
│ + Timestamp: DateTime               │
│ + EntryType: RecordEntryType        │
│ + UIInfo: UIInfo?                   │
│ + ApiInfo: ApiInfo?                 │
│ + CorrelationId: string?            │
│ + DurationMs: long?                 │
│ + Metadata: string?                 │
└─────────────────────────────────────┘
           │
    ┌──────┴──────┐
    │             │
    ▼             ▼
┌─────────┐   ┌─────────┐
│ UIInfo  │   │ ApiInfo │
├─────────┤   ├─────────┤
│Control  │   │Method   │
│Name     │   │Url      │
│Text     │   │Status   │
│Window   │   │Body     │
│Position │   │Headers  │
│...      │   │...      │
└─────────┘   └─────────┘
```

---

## Design Patterns

### 1. Singleton Pattern

**Used in:** `RecordingHub`

```csharp
public class RecordingHub
{
    private static readonly Lazy<RecordingHub> _instance =
        new Lazy<RecordingHub>(() => new RecordingHub(),
            LazyThreadSafetyMode.ExecutionAndPublication);

    public static RecordingHub Instance => _instance.Value;

    private RecordingHub() { }
}
```

**Rationale:** Single point of coordination for all recording activities.

### 2. Observer Pattern (via Rx)

**Used in:** Event streaming

```csharp
// Subject publishes events
private readonly Subject<RecordEntry> _entriesSubject;

// Observable for subscribers
public IObservable<RecordEntry> Entries =>
    _entriesSubject.AsObservable();

// Subscription
hub.Entries.Subscribe(entry => ProcessEntry(entry));
```

**Rationale:** Decoupled event distribution with composable operators.

### 3. Facade Pattern

**Used in:** `WpfRecorder` static class

```csharp
public static class WpfRecorder
{
    public static void Start(string? name = null) =>
        Hub.Start(name);

    public static HttpClient CreateHttpClient() =>
        new HttpClient(Hub.CreateHttpHandler());
}
```

**Rationale:** Simplified API hiding internal complexity.

### 4. Decorator Pattern

**Used in:** `RecordingHttpHandler`

```csharp
public class RecordingHttpHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken token)
    {
        RecordRequest(request);
        var response = await base.SendAsync(request, token);
        RecordResponse(response);
        return response;
    }
}
```

**Rationale:** Add recording capability without modifying existing HttpClient behavior.

### 5. Command Pattern

**Used in:** VS Extension Commands

```csharp
internal sealed class StartRecordingCommand
{
    private void Execute(object sender, EventArgs e)
    {
        RecordingHub.Instance.Start();
    }
}
```

**Rationale:** Encapsulate actions as objects for VS command system.

### 6. MVVM Pattern

**Used in:** Tool Window

```csharp
// ViewModel
public class RecordEntryViewModel : INotifyPropertyChanged
{
    private readonly RecordEntry _entry;
    public string Summary => GenerateSummary();
}

// View binds to ViewModel
<ListView ItemsSource="{Binding Entries}">
    <GridViewColumn DisplayMemberBinding="{Binding Summary}"/>
</ListView>
```

**Rationale:** Separation of UI and logic for testability.

---

## Extension Points

### Adding New Event Types

1. **Add enum value:**
```csharp
public enum RecordEntryType
{
    // Existing...
    UICustomGesture,  // New
}
```

2. **Create capture method:**
```csharp
public void RecordGesture(string gestureName, Point location)
{
    if (!_isActive) return;

    var entry = new RecordEntry
    {
        EntryType = RecordEntryType.UICustomGesture,
        UIInfo = new UIInfo
        {
            ControlType = "Gesture",
            Text = gestureName,
            ScreenPosition = new ScreenPoint { X = location.X, Y = location.Y }
        }
    };

    _eventSubject.OnNext(entry);
}
```

### Adding New Hooks

1. **Create hook class:**
```csharp
public class WebSocketHook : IDisposable
{
    private readonly Subject<RecordEntry> _messageSubject;

    public IObservable<RecordEntry> Messages =>
        _messageSubject.AsObservable();

    public void RecordMessage(string message, bool isIncoming)
    {
        // Create and publish entry
    }
}
```

2. **Register with hub:**
```csharp
public void SetWebSocketHook(WebSocketHook hook)
{
    var subscription = hook.Messages.Subscribe(AddEntry);
    _subscriptions.Add(subscription);
}
```

### Custom Export Formats

```csharp
public interface ISessionExporter
{
    string Extension { get; }
    void Export(RecordingSession session, Stream output);
}

public class CsvExporter : ISessionExporter
{
    public string Extension => ".csv";

    public void Export(RecordingSession session, Stream output)
    {
        using var writer = new StreamWriter(output);
        writer.WriteLine("Timestamp,Type,Summary");

        foreach (var entry in session.Entries)
        {
            writer.WriteLine($"{entry.Timestamp},{entry.EntryType},...");
        }
    }
}
```

---

## Threading Model

### Thread Safety

```csharp
public class RecordingHub
{
    private readonly object _lock = new object();
    private readonly List<RecordEntry> _entries = new List<RecordEntry>();

    public void AddEntry(RecordEntry entry)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }

        // Fire event outside lock
        _entriesSubject.OnNext(entry);
        EntryRecorded?.Invoke(this, entry);
    }

    public IReadOnlyList<RecordEntry> GetEntries()
    {
        lock (_lock)
        {
            return new List<RecordEntry>(_entries);
        }
    }
}
```

### UI Thread Marshaling

```csharp
// In Tool Window
private void OnEntryRecorded(object sender, RecordEntry entry)
{
    Dispatcher.Invoke(() =>
    {
        _entries.Add(new RecordEntryViewModel(entry));
        EventsList.ScrollIntoView(_entries.Last());
    });
}
```

### Async Operations

```csharp
protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
{
    // Async capture
    var requestEntry = await CreateRequestEntryAsync(request);
    _requestSubject.OnNext(requestEntry);

    // Async HTTP call
    var response = await base.SendAsync(request, cancellationToken);

    // Async response capture
    var responseEntry = await CreateResponseEntryAsync(response);
    _responseSubject.OnNext(responseEntry);

    return response;
}
```

---

## Serialization

### JSON Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "sessionId": { "type": "string", "format": "uuid" },
    "name": { "type": "string" },
    "startTime": { "type": "string", "format": "date-time" },
    "endTime": { "type": ["string", "null"], "format": "date-time" },
    "schemaVersion": { "type": "string" },
    "entries": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "timestamp": { "type": "string", "format": "date-time" },
          "entryType": {
            "type": "string",
            "enum": ["UIClick", "UITextInput", "ApiRequest", "ApiResponse", "..."]
          },
          "uiInfo": { "$ref": "#/definitions/UIInfo" },
          "apiInfo": { "$ref": "#/definitions/ApiInfo" },
          "correlationId": { "type": ["string", "null"] },
          "durationMs": { "type": ["integer", "null"] }
        },
        "required": ["id", "timestamp", "entryType"]
      }
    }
  },
  "required": ["sessionId", "name", "startTime", "entries", "schemaVersion"]
}
```

### Serialization Options

```csharp
private static readonly JsonSerializerOptions _jsonOptions = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() },
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

---

## Security Considerations

### Data Sensitivity

| Data Type | Risk Level | Mitigation |
|-----------|------------|------------|
| Passwords | High | Don't capture password fields |
| API Keys | High | Filter Authorization headers |
| PII | Medium | Sanitize before sharing |
| Request Bodies | Medium | Size limits, opt-out |

### Recommendations

1. **Never commit recordings** to source control
2. **Encrypt sensitive recordings** at rest
3. **Filter headers** like Authorization, Cookie
4. **Implement redaction** for known sensitive fields
5. **Add data retention** policies

---

## Performance Considerations

### Memory Management

| Concern | Strategy |
|---------|----------|
| Large recordings | Periodic flush to disk |
| Large HTTP bodies | Size limits (default 1MB) |
| Event accumulation | Clear periodically |
| Observable subscriptions | Proper disposal |

### Benchmarks (Approximate)

| Operation | Overhead |
|-----------|----------|
| UI event capture | < 1ms |
| HTTP request capture | < 5ms |
| HTTP body read | Depends on size |
| JSON serialization | ~10ms per 1000 entries |

### Optimization Tips

1. **Disable body capture** for high-volume APIs
2. **Use background threads** for file I/O
3. **Batch UI events** if needed
4. **Limit recording duration** for long sessions

---

## Future Architecture Considerations

### Planned Enhancements

1. **Plugin System**: Load hooks dynamically
2. **Remote Recording**: Record from attached processes
3. **Streaming Export**: Real-time file output
4. **Compression**: Reduce file sizes
5. **Encryption**: Secure sensitive recordings

### Scalability

```
Current: Single Process
┌─────────────────────┐
│ App + Recorder      │
└─────────────────────┘

Future: Multi-Process
┌─────────────┐     ┌─────────────┐
│ App Process │────▶│ Recorder    │
└─────────────┘     │ Service     │
┌─────────────┐     │             │
│ App Process │────▶│             │
└─────────────┘     └─────────────┘
```
