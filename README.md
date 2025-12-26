# WPF Event Recorder

[![Visual Studio 2022](https://img.shields.io/badge/VS2022-17.0+-blue.svg)](https://visualstudio.microsoft.com/)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple.svg)](https://dotnet.microsoft.com/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-green.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

A Visual Studio 2022 VSIX extension and library for recording WPF application interactions to support automation testing. Captures UI events, ViewModel property changes, and HTTP API calls with correlation tracking.

## Quick Start

```csharp
// 1. Add reference to WpfEventRecorder.Core

// 2. Initialize in App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    RecordingBootstrapper.Instance.Initialize(this);
}

// 3. Start recording
WpfRecorder.Start("MySession");

// 4. Use recording-enabled HttpClient
var client = WpfRecorder.CreateHttpClient();

// 5. Stop and save
WpfRecorder.Stop();
WpfRecorder.SaveToFile("recording.json");
```

## Features

| Feature | Description |
|---------|-------------|
| **UI Event Recording** | Captures clicks, text input, selections, toggles, keyboard shortcuts, window events |
| **HTTP API Recording** | Intercepts HTTP requests/responses with headers and body content |
| **ViewModel Tracking** | Records property changes via data binding paths |
| **Visual Tree Inspector** | Browse and inspect element properties in real-time |
| **Correlation Tracking** | Links UI actions to their resulting API calls |
| **Named Pipe IPC** | Real-time event streaming from target app to VS Tool Window |
| **Auto-Instrumentation** | Walks visual tree and attaches handlers automatically |
| **Multiple Export Formats** | JSON, CSV, Excel, MSTest, NUnit, xUnit, Playwright |
| **Recording Attributes** | Mark ViewModels and properties for targeted recording |

## Solution Structure

```
WpfEventRecorder/
├── src/
│   ├── Framework/                        # .NET Framework 4.7.2 Solution
│   │   ├── WpfEventRecorder.Framework.sln
│   │   ├── WpfEventRecorder/             # VSIX Extension for VS2022
│   │   │   ├── Commands/                 # Menu commands
│   │   │   ├── ToolWindows/              # Recording dashboard
│   │   │   └── Services/                 # Project analyzer, code injection
│   │   ├── WpfEventRecorder.Core/        # Core recording library
│   │   │   ├── Attributes/               # Recording attributes
│   │   │   ├── Hooks/                    # UI and HTTP hooks
│   │   │   ├── Infrastructure/           # Bootstrapper, commands
│   │   │   ├── Ipc/                      # Named pipe communication
│   │   │   ├── Models/                   # Event models
│   │   │   └── Services/                 # Recording hub
│   │   ├── WpfEventRecorder.SampleApp/   # Sample WPF MVVM app
│   │   └── WpfEventRecorder.Tests/       # Unit tests (xUnit)
│   │
│   └── Core/                             # .NET 8 Solution
│       ├── WpfEventRecorder.Core.sln
│       ├── WpfEventRecorder.Core/        # Core recording library
│       ├── WpfEventRecorder.App/         # Standalone WPF recorder app
│       └── WpfEventRecorder.Tests/       # Unit tests (xUnit)
│
├── docs/                                 # Documentation
│   ├── Architecture.md
│   ├── APIReference.md
│   ├── DeveloperGuide.md
│   ├── UserManual.md
│   └── export-formats.md
│
└── README.md
```

## Installation

### Option 1: VSIX Extension (Visual Studio 2022)

```bash
cd src/Framework
dotnet build -c Release
# Install the generated .vsix file
```

### Option 2: Standalone App (.NET 8)

```bash
cd src/Core
dotnet run --project WpfEventRecorder.App
```

### Option 3: NuGet Package (for integration)

```bash
# Coming soon
dotnet add package WpfEventRecorder.Core
```

## Usage

### VSIX Extension

| Action | Menu | Shortcut |
|--------|------|----------|
| Start Recording | Tools → WPF Event Recorder → Start | `Ctrl+Alt+R` |
| Stop Recording | Tools → WPF Event Recorder → Stop | `Ctrl+Alt+S` |
| Save Recording | Tools → WPF Event Recorder → Save | `Ctrl+Alt+E` |
| Open Dashboard | Tools → WPF Event Recorder → Event Viewer | - |

### Standalone App

1. Launch `WpfEventRecorder.App`
2. Click **Select Window** to choose a target WPF application
3. Click **Start Recording** to begin capturing events
4. Interact with the target application
5. Click **Stop Recording** when finished
6. Use **Save**, **CSV**, or **Excel** to export

### Visual Tree Inspector

The Visual Tree panel allows you to:
- Browse the element hierarchy of the target window
- View element properties (Name, AutomationId, Type, Bindings)
- Identify elements for UI automation scripts

## Integration Guide

### Basic Setup

```csharp
using WpfEventRecorder.Core;
using WpfEventRecorder.Core.Infrastructure;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize recording
        RecordingBootstrapper.Instance.Initialize(this);

        // Or connect to VSIX via Named Pipe (for debugging)
        // await RecordingBootstrapper.Instance.InitializeWithIpcAsync(this, sessionId);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        RecordingBootstrapper.Instance.Dispose();
        base.OnExit(e);
    }
}
```

### Recording Attributes

```csharp
using WpfEventRecorder.Core.Attributes;

[RecordViewModel("CustomerEditor")]
public class CustomerViewModel : INotifyPropertyChanged
{
    [RecordProperty("Customer Name")]
    public string Name { get; set; }

    [RecordProperty]
    public string Email { get; set; }

    [IgnoreRecording(Reason = "Sensitive data")]
    public string Password { get; set; }
}
```

### RecordingViewModelBase

```csharp
using WpfEventRecorder.Core.Infrastructure;

public class MyViewModel : RecordingViewModelBase
{
    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value); // Automatically recorded!
    }
}
```

### Recording Commands

```csharp
using WpfEventRecorder.Core.Infrastructure;

public class MyViewModel
{
    public ICommand SaveCommand { get; }

    public MyViewModel()
    {
        SaveCommand = new RecordingCommand(
            execute: _ => Save(),
            commandName: "SaveCommand",
            viewModelType: nameof(MyViewModel));
    }
}
```

### Recording HTTP Calls

```csharp
using WpfEventRecorder.Core;

// All HTTP calls automatically recorded with correlation
var httpClient = WpfRecorder.CreateHttpClient();
var response = await httpClient.GetAsync("https://api.example.com/data");
```

## Export Formats

| Format | Extension | Use Case |
|--------|-----------|----------|
| **JSON** | `.json` | Data exchange, playback, analysis |
| **CSV** | `.csv` | Spreadsheet analysis |
| **Excel** | `.xml` | Excel-compatible spreadsheet |
| **MSTest** | `.cs` | Visual Studio MSTest unit tests |
| **NUnit** | `.cs` | NUnit framework tests |
| **xUnit** | `.cs` | xUnit framework tests |
| **Playwright** | `.cs` | Playwright-style selectors |

### Example: MSTest Export

```csharp
[TestClass]
public class CustomerFormTests
{
    [TestMethod]
    public async Task Test_CreateCustomer()
    {
        // Arrange
        var vm = new CustomerViewModel();

        // Act - Recorded interactions
        vm.Name = "John Doe";
        vm.Email = "john@example.com";
        vm.SaveCommand.Execute(null);

        // Assert
        // API POST https://api.example.com/customers -> 201 Created
    }
}
```

See [docs/export-formats.md](docs/export-formats.md) for detailed format documentation.

## Event Types

### UI Events

| Type | Description | Captured Data |
|------|-------------|---------------|
| `UIClick` | Button/control click | Control info, position |
| `UIDoubleClick` | Double-click | Control info, position |
| `UITextInput` | Text entry | Old/new value, binding path |
| `UISelectionChange` | Selection changed | Selected item(s) |
| `UIToggle` | Check/uncheck | Boolean state |
| `UIKeyboardShortcut` | Key combination | Key + modifiers |
| `UIWindowOpen/Close` | Window lifecycle | Window title, type |

### API Events

| Type | Description | Captured Data |
|------|-------------|---------------|
| `ApiRequest` | HTTP request sent | Method, URL, headers, body |
| `ApiResponse` | HTTP response received | Status, headers, body, duration |

### ViewModel Events

| Type | Description | Captured Data |
|------|-------------|---------------|
| `PropertyChange` | Property changed | Property name, old/new value |
| `Command` | Command executed | Command name, parameter |

## API Reference

### WpfRecorder

```csharp
// State
bool IsRecording { get; }
int EntryCount { get; }
RecordingHub Hub { get; }

// Events
event EventHandler<bool> RecordingStateChanged;
event EventHandler<RecordEntry> EntryRecorded;

// Control
void Initialize();
void Start(string sessionName = null);
void Stop();
void Clear();
void SaveToFile(string filePath);

// HTTP
HttpClient CreateHttpClient();
RecordingHttpHandler CreateHttpHandler();

// Manual recording
void RecordClick(string controlType, string controlName, string text);
void RecordTextInput(string controlType, string controlName, string oldValue, string newValue);
```

### RecordingBootstrapper

```csharp
// Singleton
RecordingBootstrapper.Instance

// Initialize
void Initialize(Application app);
Task InitializeWithIpcAsync(Application app, string sessionId);

// HTTP
HttpClient CreateRecordingHttpClient();
RecordingHttpHandler CreateRecordingHandler();

// Control
void StartRecording(string sessionName = null);
void StopRecording();
void InstrumentWindow(Window window);
```

### ExportService

```csharp
// Unified export
void Export(entries, filePath, ExportFormat.MSTest, session);

// Format-specific
void ExportToJson(entries, filePath, session);
void ExportToMSTest(entries, filePath, session);
void ExportToNUnit(entries, filePath, session);
void ExportToXUnit(entries, filePath, session);
void ExportToPlaywright(entries, filePath, session);
void ExportToCsv(entries, filePath);
void ExportToExcel(entries, filePath);
```

## Requirements

| Solution | Requirements |
|----------|--------------|
| **Framework** | Visual Studio 2022 17.0+, .NET Framework 4.7.2, Windows 10/11 |
| **Core** | .NET SDK 8.0+, Windows 10/11 |

## Building

```bash
# Framework Solution (VSIX)
cd src/Framework
dotnet restore
dotnet build
dotnet test

# Core Solution (Standalone)
cd src/Core
dotnet restore
dotnet build
dotnet test
dotnet run --project WpfEventRecorder.App
```

## Sample Application

The `WpfEventRecorder.SampleApp` demonstrates:

- **CustomerViewModel**: Property recording with validation
- **CustomerListViewModel**: Command recording with async operations
- **CustomerService**: HTTP API call recording
- **Full MVVM Pattern**: ViewModelBase, RelayCommand, data binding

```bash
cd src/Framework
dotnet run --project WpfEventRecorder.SampleApp
```

## Architecture

### Named Pipe IPC

```
┌─────────────────┐     Named Pipe     ┌─────────────────┐
│  Target WPF     │ ─────────────────► │  VSIX Extension │
│  Application    │   JSON Events      │  Tool Window    │
│                 │                    │                 │
│ NamedPipeClient │                    │ NamedPipeServer │
└─────────────────┘                    └─────────────────┘
```

### View Instrumentation

The `ViewInstrumenter` walks the visual tree and attaches event handlers:

| Control | Events Captured |
|---------|-----------------|
| TextBox | LostFocus, TextChanged |
| ComboBox/ListBox | SelectionChanged |
| CheckBox/RadioButton | Checked, Unchecked |
| Button | Click |
| DatePicker | SelectedDateChanged |
| Slider | ValueChanged |
| DataGrid | SelectionChanged, CellEditEnding |
| TabControl | SelectionChanged |

## Documentation

- [Architecture](docs/Architecture.md) - System design and components
- [API Reference](docs/APIReference.md) - Complete API documentation
- [Developer Guide](docs/DeveloperGuide.md) - Contributing and extending
- [User Manual](docs/UserManual.md) - End-user guide
- [Export Formats](docs/export-formats.md) - Export format details

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see [LICENSE.txt](LICENSE.txt) for details.

## Acknowledgments

- [Microsoft Visual Studio SDK](https://docs.microsoft.com/en-us/visualstudio/extensibility/)
- [System.Reactive](https://github.com/dotnet/reactive)
- [UI Automation](https://docs.microsoft.com/en-us/dotnet/framework/ui-automation/)
