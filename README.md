# WPF Event Recorder

A Visual Studio 2022 VSIX extension and library for recording WPF application interactions to support automation testing. Captures UI events, ViewModel property changes, and HTTP API calls with correlation tracking.

## Features

- **UI Event Recording**: Captures clicks, text input, selections, toggles, keyboard shortcuts, and window events
- **HTTP API Recording**: Intercepts and records HTTP requests/responses with full headers and body content
- **ViewModel Property Tracking**: Records property changes via data binding paths
- **Correlation Tracking**: Links UI actions to their resulting API calls
- **Named Pipe IPC**: Real-time event streaming from target app to VS Tool Window
- **Auto-Instrumentation**: Walk visual tree and attach handlers automatically
- **Multiple Export Formats**: JSON, CSV, Excel, MSTest, NUnit, xUnit, and Playwright-style output
- **Recording Attributes**: Mark ViewModels and properties for targeted recording
- **Tool Window Dashboard**: Real-time event viewer with filtering and details panel

## Solution Structure

```
WpfEventRecorder/
├── src/
│   ├── Framework/                        # .NET Framework 4.7.2 Solution
│   │   ├── WpfEventRecorder.Framework.sln
│   │   ├── WpfEventRecorder/             # VSIX Extension for VS2022
│   │   ├── WpfEventRecorder.Core/        # Core recording library
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
6. Click **Save Recording** to export (JSON, CSV, Excel, or Test Code)

### Standalone App (Core Solution)

1. Run `WpfEventRecorder.App`
2. Select a target WPF window from the window picker
3. Click **Start Recording**
4. Interact with the target application
5. Click **Stop Recording**
6. Click **Save** to export as JSON

### Keyboard Shortcuts (VSIX)

| Action | Shortcut |
|--------|----------|
| Start Recording | `Ctrl+Alt+R` |
| Stop Recording | `Ctrl+Alt+S` |
| Save Recording | `Ctrl+Alt+E` |

## Integration Guide

### Basic Integration

Add a reference to `WpfEventRecorder.Core` and initialize recording:

```csharp
using WpfEventRecorder.Core;
using WpfEventRecorder.Core.Infrastructure;

// In App.xaml.cs OnStartup
protected override async void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // Initialize recording for this application
    RecordingBootstrapper.Instance.Initialize(this);

    // Or connect to VSIX via Named Pipe
    await RecordingBootstrapper.Instance.InitializeWithIpcAsync(this, sessionId);
}

// Start/Stop recording programmatically
WpfRecorder.Start("My Test Session");
WpfRecorder.Stop();
WpfRecorder.SaveToFile("recording.json");
```

### Using Recording Attributes

Decorate your ViewModels to enable automatic property change recording:

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

### Using RecordingViewModelBase

Inherit from the recording-enabled base class:

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

### Using Recording Commands

Wrap commands to record their execution:

```csharp
using WpfEventRecorder.Core.Infrastructure;

public class MyViewModel
{
    public ICommand SaveCommand { get; }

    public MyViewModel()
    {
        // Use RecordingCommand for automatic command recording
        SaveCommand = new RecordingCommand(
            execute: SaveCustomer,
            commandName: "SaveCommand",
            viewModelType: nameof(MyViewModel));
    }
}
```

### Recording HTTP Calls

Use the recording-enabled HttpClient:

```csharp
using WpfEventRecorder.Core;

// Option 1: Use the convenience method
var httpClient = WpfRecorder.CreateHttpClient();

// Option 2: Use RecordingBootstrapper
var httpClient = RecordingBootstrapper.Instance.CreateRecordingHttpClient();

// All HTTP calls are now automatically recorded with correlation
var response = await httpClient.GetAsync("https://api.example.com/data");
```

## Export Formats

### JSON Export

```json
{
  "sessionId": "guid",
  "sessionName": "Test Session",
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": "2024-01-15T10:35:00Z",
  "entries": [...]
}
```

### MSTest Export

```csharp
[TestClass]
public class RecordingTests
{
    [TestMethod]
    public async Task Test_Session_20240115_103000()
    {
        // Arrange
        var vm = new CustomerViewModel();

        // Recorded inputs
        // Text input: TextBox#txtName
        // Binding: Text:Name
        // vm.Name = "John Doe";

        // Click: Button#btnSave
        // vm.SaveCommand.Execute(null);

        // API POST https://api.example.com/customers
        // Assert
    }
}
```

### NUnit Export

```csharp
[TestFixture]
public class RecordingTests
{
    [Test]
    public async Task Test_Session_20240115_103000()
    {
        // Recorded test steps...
    }
}
```

### Playwright-Style Export

```csharp
public class RecordingTests
{
    [Fact]
    public async Task Test_Session()
    {
        // await page.Locator("#txtName").FillAsync("John Doe");
        // await page.Locator("#btnSave").ClickAsync();
        // await Expect(response).ToHaveStatusCodeAsync(200);
    }
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
| `UIWindowOpen` | Window opened |
| `UIWindowClose` | Window closed |

### API Events

| Type | Description |
|------|-------------|
| `ApiRequest` | HTTP request sent |
| `ApiResponse` | HTTP response received |

### ViewModel Events

| Type | Description |
|------|-------------|
| `PropertyChange` | ViewModel property changed |
| `Command` | Command executed |
| `Navigation` | View navigation |

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
HttpClient CreateHttpClient();
```

### RecordingBootstrapper

```csharp
// Initialize recording for an application
void Initialize(Application app);
Task InitializeWithIpcAsync(Application app, string sessionId);

// Create recording-enabled HttpClient
HttpClient CreateRecordingHttpClient();
RecordingHttpHandler CreateRecordingHandler();

// Control recording
void StartRecording(string sessionName = null);
void StopRecording();
void ClearRecording();
void SaveRecording(string filePath);

// Instrument views
void InstrumentWindow(Window window);
```

### ExportService

```csharp
// Export to various formats
void Export(IEnumerable<RecordEntry> entries, string filePath, ExportFormat format);
void ExportToJson(entries, filePath, session);
void ExportToMSTest(entries, filePath, session);
void ExportToNUnit(entries, filePath, session);
void ExportToXUnit(entries, filePath, session);
void ExportToPlaywright(entries, filePath, session);
void ExportToCsv(entries, filePath);
void ExportToExcel(entries, filePath);
```

## Requirements

### Framework Solution (.NET Framework 4.7.2)
- Visual Studio 2022 (17.0 or later)
- .NET SDK 6.0 or later (for building)
- .NET Framework 4.7.2 Developer Pack
- Windows 10/11

### Core Solution (.NET 8)
- Visual Studio 2022 (17.8 or later) or VS Code
- .NET SDK 8.0 or later
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

## Sample Application

The `WpfEventRecorder.SampleApp` project demonstrates a complete MVVM application with:

- **CustomerViewModel**: Property recording with validation
- **CustomerListViewModel**: Command recording with async operations
- **CustomerService**: HTTP API calls with recording
- **Full MVVM Pattern**: ViewModelBase, RelayCommand, data binding

Run the sample app to see recording in action:

```bash
cd src/Framework
dotnet run --project WpfEventRecorder.SampleApp
```

## Architecture

### Named Pipe IPC

The VSIX extension and target application communicate via Named Pipes:

1. VSIX creates a `NamedPipeServer` with a unique session ID
2. Target app connects via `NamedPipeClient` using the session ID
3. Events are serialized as JSON and streamed in real-time
4. Auto-reconnect on disconnect

### View Instrumentation

The `ViewInstrumenter` walks the visual tree and attaches event handlers:

- TextBox: LostFocus, TextChanged
- ComboBox/ListBox: SelectionChanged
- CheckBox/RadioButton: Checked/Unchecked
- Button: Click
- DatePicker: SelectedDateChanged
- Slider: ValueChanged
- DataGrid: SelectionChanged, CellEditEnding
- TabControl: SelectionChanged

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Submit a pull request

## License

MIT License - see [LICENSE.txt](LICENSE.txt) for details.
