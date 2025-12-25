# WPF Event Recorder

A library and toolset for recording WPF application interactions for automation testing. Captures UI events and HTTP API calls with correlation tracking.

## Features

- **UI Event Recording**: Captures clicks, text input, selections, toggles, keyboard shortcuts, and window events
- **HTTP API Recording**: Intercepts and records HTTP requests/responses with full headers and body content
- **Correlation Tracking**: Links UI actions to their resulting API calls
- **Tool Window**: Real-time event viewer with filtering and details panel
- **Export to JSON**: Save recordings for playback or analysis

## Solution Structure

This repository contains two solutions targeting different .NET versions:

```
WpfEventRecorder/
├── src/
│   ├── Framework/                        # .NET Framework 4.7.2 Solution
│   │   ├── WpfEventRecorder.Framework.sln
│   │   ├── WpfEventRecorder/             # VSIX Extension for VS2022
│   │   ├── WpfEventRecorder.Core/        # Core recording library
│   │   ├── WpfEventRecorder.SampleApp/   # Sample WPF app
│   │   └── WpfEventRecorder.Tests/       # Unit tests (xUnit)
│   │
│   └── Core/                             # .NET 8 Solution
│       ├── WpfEventRecorder.Core.sln
│       ├── WpfEventRecorder.Core/        # Core recording library
│       ├── WpfEventRecorder.App/         # Standalone WPF app
│       └── WpfEventRecorder.Tests/       # Unit tests (xUnit)
│
├── docs/                                 # Documentation
└── README.md
```

## Choosing a Solution

| Solution | Target Framework | Use Case |
|----------|------------------|----------|
| **Framework** | .NET Framework 4.7.2 | VS2022 Extension (VSIX), legacy WPF apps |
| **Core** | .NET 8 | Modern WPF apps, standalone recorder app |

## Installation

### .NET Framework 4.7.2 (VSIX Extension)

1. Open `src/Framework/WpfEventRecorder.Framework.sln` in Visual Studio 2022
2. Build in Release mode
3. Find `WpfEventRecorder.vsix` in the output folder
4. Double-click to install in Visual Studio 2022

### .NET 8 (Standalone App)

1. Open `src/Core/WpfEventRecorder.Core.sln` in Visual Studio 2022
2. Build and run `WpfEventRecorder.App`

## Usage

### In Visual Studio (Framework Solution)

1. Open a WPF solution
2. Use the **WPF Event Recorder** toolbar or go to **Tools > WPF Event Recorder**
3. Click **Start Recording** (or press `Ctrl+Alt+R`)
4. Run and interact with your WPF application
5. Click **Stop Recording** (or press `Ctrl+Alt+S`)
6. Click **Save Recording** (or press `Ctrl+Alt+E`) to export as JSON

### Standalone App (Core Solution)

1. Run `WpfEventRecorder.App`
2. Click **Start Recording**
3. Interact with your WPF application (requires integration)
4. Click **Stop Recording**
5. Click **Save** to export as JSON

### Keyboard Shortcuts (VSIX)

| Action | Shortcut |
|--------|----------|
| Start Recording | `Ctrl+Alt+R` |
| Stop Recording | `Ctrl+Alt+S` |
| Save Recording | `Ctrl+Alt+E` |

### Integrating with Your WPF App

Add a reference to `WpfEventRecorder.Core` and initialize recording:

```csharp
using WpfEventRecorder.Core;

// Initialize on app startup
WpfRecorder.Initialize();

// Start recording
WpfRecorder.Start("My Test Session");

// Create an HttpClient with recording enabled
var httpClient = WpfRecorder.CreateHttpClient();

// Record custom events
WpfRecorder.RecordClick("Button", "SubmitButton", "Submit");
WpfRecorder.RecordTextInput("TextBox", "EmailInput", "", "user@example.com");

// Stop and save
WpfRecorder.Stop();
WpfRecorder.SaveToFile("recording.json");
```

### Using the Recording HTTP Handler

```csharp
using WpfEventRecorder.Core;
using WpfEventRecorder.Core.Hooks;

// Option 1: Use the convenience method
var httpClient = WpfRecorder.CreateHttpClient();

// Option 2: Create handler manually for custom configuration
var handler = new RecordingHttpHandler(
    innerHandler: new HttpClientHandler(),
    captureRequestBody: true,
    captureResponseBody: true,
    maxBodySize: 1024 * 1024  // 1MB
);
var httpClient = new HttpClient(handler);
```

## Recording Output

Recordings are saved as JSON with the following structure:

```json
{
  "sessionId": "guid",
  "name": "Session Name",
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": "2024-01-15T10:35:00Z",
  "entries": [
    {
      "id": "guid",
      "timestamp": "2024-01-15T10:30:01Z",
      "entryType": "UIClick",
      "correlationId": "guid",
      "uiInfo": {
        "controlType": "Button",
        "controlName": "SubmitButton",
        "automationId": "btnSubmit",
        "text": "Submit",
        "windowTitle": "Main Window"
      }
    },
    {
      "id": "guid",
      "timestamp": "2024-01-15T10:30:01Z",
      "entryType": "ApiRequest",
      "correlationId": "guid",
      "apiInfo": {
        "method": "POST",
        "url": "https://api.example.com/submit",
        "requestBody": "{\"data\":\"value\"}",
        "requestContentType": "application/json"
      }
    },
    {
      "id": "guid",
      "timestamp": "2024-01-15T10:30:02Z",
      "entryType": "ApiResponse",
      "correlationId": "guid",
      "durationMs": 150,
      "apiInfo": {
        "method": "POST",
        "url": "https://api.example.com/submit",
        "statusCode": 200,
        "responseBody": "{\"success\":true}",
        "isSuccess": true
      }
    }
  ]
}
```

## Event Types

### UI Events

| Type | Description |
|------|-------------|
| `UIClick` | Button or control click |
| `UIDoubleClick` | Double-click event |
| `UITextInput` | Text entry in TextBox |
| `UISelectionChange` | ComboBox/ListBox selection |
| `UIToggle` | CheckBox/RadioButton change |
| `UIKeyboardShortcut` | Keyboard shortcut pressed |
| `UIDragDrop` | Drag and drop operation |
| `UIWindowOpen` | Window opened |
| `UIWindowClose` | Window closed |

### API Events

| Type | Description |
|------|-------------|
| `ApiRequest` | HTTP request sent |
| `ApiResponse` | HTTP response received |

## API Reference

### WpfRecorder (Static Class)

```csharp
// Properties
bool IsRecording { get; }
int EntryCount { get; }
RecordingHub Hub { get; }

// Events
event EventHandler<bool> RecordingStateChanged;
event EventHandler<RecordEntry> EntryRecorded;

// Methods
void Initialize();
void Start(string? sessionName = null);
void Stop();
void Clear();
void SaveToFile(string filePath);
string ExportAsJson();
HttpClient CreateHttpClient();
RecordingHttpHandler CreateHttpHandler(HttpMessageHandler? innerHandler = null);
void RecordClick(string controlType, string? controlName, string? text = null);
void RecordTextInput(string controlType, string? controlName, string? oldValue, string? newValue);
void RecordCustomEvent(string eventType, string? data = null);
void SetCorrelationId(string correlationId);
string NewCorrelationId();
```

### RecordingHub (Singleton)

```csharp
// Access singleton
RecordingHub.Instance

// Observable stream of events
IObservable<RecordEntry> Entries { get; }

// Session management
RecordingSession? CurrentSession { get; }
void Start(string? sessionName = null);
void Stop();
void Clear();
IReadOnlyList<RecordEntry> GetEntries();

// File operations
void SaveToFile(string filePath);
RecordingSession LoadFromFile(string filePath);
string ExportAsJson();
```

## Requirements

### Framework Solution (.NET Framework 4.7.2)
- Visual Studio 2022 (17.0 or later)
- .NET Framework 4.7.2 SDK
- Windows 10/11

### Core Solution (.NET 8)
- Visual Studio 2022 (17.8 or later) or VS Code
- .NET 8 SDK
- Windows 10/11

## Building

### Framework Solution

```bash
cd src/Framework
dotnet restore
dotnet build

# Run tests
dotnet test

# Create VSIX package
msbuild /t:Build /p:Configuration=Release WpfEventRecorder/WpfEventRecorder.csproj
```

### Core Solution

```bash
cd src/Core
dotnet restore
dotnet build

# Run tests
dotnet test

# Run the app
dotnet run --project WpfEventRecorder.App
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Submit a pull request

## License

MIT License - see [LICENSE.txt](LICENSE.txt) for details.
