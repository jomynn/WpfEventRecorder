# WPF Event Recorder - Developer Guide

## Table of Contents

1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Setting Up Development Environment](#setting-up-development-environment)
4. [Core Library Integration](#core-library-integration)
5. [Recording UI Events](#recording-ui-events)
6. [Recording HTTP Calls](#recording-http-calls)
7. [Working with the RecordingHub](#working-with-the-recordinghub)
8. [Extending the Recorder](#extending-the-recorder)
9. [Building and Debugging](#building-and-debugging)
10. [Testing](#testing)
11. [Deployment](#deployment)
12. [Best Practices](#best-practices)

---

## Overview

WPF Event Recorder consists of three main components:

1. **WpfEventRecorder.Core**: A .NET Standard 2.0 library providing the recording functionality
2. **WpfEventRecorder**: A Visual Studio 2022 VSIX extension
3. **WpfEventRecorder.SampleApp**: A demonstration WPF application

This guide covers how to integrate the Core library into your applications and extend the recorder functionality.

---

## Project Structure

```
WpfEventRecorder/
├── WpfEventRecorder/                    # VSIX Extension
│   ├── Commands/                        # VS command implementations
│   │   ├── RecorderCommandSet.vsct      # Command table definition
│   │   ├── CommandIds.cs                # Command ID constants
│   │   ├── StartRecordingCommand.cs
│   │   ├── StopRecordingCommand.cs
│   │   ├── SaveRecordingCommand.cs
│   │   ├── ClearRecordingCommand.cs
│   │   └── OpenToolWindowCommand.cs
│   ├── ToolWindows/                     # VS tool window
│   │   ├── RecorderToolWindow.cs
│   │   ├── RecorderToolWindowControl.xaml
│   │   ├── RecorderToolWindowControl.xaml.cs
│   │   └── RecordEntryViewModel.cs
│   ├── Resources/                       # Icons and images
│   ├── WpfEventRecorderPackage.cs       # VS package entry point
│   └── source.extension.vsixmanifest    # VSIX manifest
│
├── WpfEventRecorder.Core/               # Core library
│   ├── Models/                          # Data models
│   │   ├── RecordEntry.cs               # Single recorded event
│   │   ├── RecordingSession.cs          # Session container
│   │   ├── UIInfo.cs                    # UI event details
│   │   └── ApiInfo.cs                   # HTTP event details
│   ├── Hooks/                           # Event capture mechanisms
│   │   ├── UIHook.cs                    # UI event capture
│   │   └── RecordingHttpHandler.cs      # HTTP interception
│   ├── Services/
│   │   └── RecordingHub.cs              # Central coordination
│   └── WpfRecorder.cs                   # Public API facade
│
├── WpfEventRecorder.SampleApp/          # Demo application
└── WpfEventRecorder.Tests/              # Unit tests
```

---

## Setting Up Development Environment

### Prerequisites

- Visual Studio 2022 (17.0+)
- .NET 6.0 SDK or later
- Visual Studio SDK workload
- .NET Framework 4.8 Developer Pack

### Installing VS SDK Workload

1. Open Visual Studio Installer
2. Click **Modify** on VS 2022
3. Go to **Workloads** tab
4. Check **Visual Studio extension development**
5. Click **Modify**

### Cloning and Building

```bash
# Clone the repository
git clone https://github.com/yourname/WpfEventRecorder.git
cd WpfEventRecorder

# Restore NuGet packages
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test
```

### Opening in Visual Studio

1. Open `WpfEventRecorder.sln`
2. Set `WpfEventRecorder` as startup project for extension debugging
3. Set `WpfEventRecorder.SampleApp` for testing the sample app

---

## Core Library Integration

### Adding the Reference

**Option 1: Project Reference**

```xml
<ItemGroup>
  <ProjectReference Include="..\WpfEventRecorder.Core\WpfEventRecorder.Core.csproj" />
</ItemGroup>
```

**Option 2: NuGet Package** (when published)

```xml
<PackageReference Include="WpfEventRecorder.Core" Version="1.0.0" />
```

### Basic Integration

```csharp
using WpfEventRecorder.Core;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize the recorder
        WpfRecorder.Initialize();

        // Optionally start recording automatically
        WpfRecorder.Start("My Application Session");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Stop recording and optionally save
        if (WpfRecorder.IsRecording)
        {
            WpfRecorder.Stop();
            WpfRecorder.SaveToFile("session.json");
        }

        base.OnExit(e);
    }
}
```

---

## Recording UI Events

### Automatic Recording with UIHook

The `UIHook` class provides methods to record various UI events:

```csharp
using WpfEventRecorder.Core.Hooks;

// Get the UIHook from the hub
var uiHook = new UIHook();
RecordingHub.Instance.SetUIHook(uiHook);

// Start capturing
uiHook.Start();
```

### Manual Event Recording

For controls that don't automatically trigger events, record manually:

```csharp
// Record a button click
WpfRecorder.RecordClick("Button", "SaveButton", "Save");

// Record text input
WpfRecorder.RecordTextInput("TextBox", "NameInput", "old value", "new value");

// Record custom events
WpfRecorder.RecordCustomEvent("CustomAction", "{\"action\":\"refresh\"}");
```

### Instrumenting Controls

Add recording to your event handlers:

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        // Record the click
        WpfRecorder.RecordClick("Button", "SubmitButton", "Submit");

        // Your existing logic
        ProcessSubmission();
    }

    private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        WpfRecorder.RecordTextInput(
            "TextBox",
            textBox?.Name,
            null,  // Old value (optional)
            textBox?.Text);
    }

    private void CategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var combo = sender as ComboBox;
        var oldValue = e.RemovedItems.Count > 0 ? e.RemovedItems[0]?.ToString() : null;
        var newValue = combo?.SelectedItem?.ToString();

        WpfRecorder.Hub.AddEntry(new RecordEntry
        {
            EntryType = RecordEntryType.UISelectionChange,
            UIInfo = new UIInfo
            {
                ControlType = "ComboBox",
                ControlName = combo?.Name,
                OldValue = oldValue,
                NewValue = newValue,
                WindowTitle = Title
            }
        });
    }
}
```

### Using Attached Behaviors

Create reusable recording behaviors:

```csharp
public static class RecordingBehavior
{
    public static readonly DependencyProperty RecordClicksProperty =
        DependencyProperty.RegisterAttached(
            "RecordClicks",
            typeof(bool),
            typeof(RecordingBehavior),
            new PropertyMetadata(false, OnRecordClicksChanged));

    public static bool GetRecordClicks(DependencyObject obj) =>
        (bool)obj.GetValue(RecordClicksProperty);

    public static void SetRecordClicks(DependencyObject obj, bool value) =>
        obj.SetValue(RecordClicksProperty, value);

    private static void OnRecordClicksChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is ButtonBase button && (bool)e.NewValue)
        {
            button.Click += (s, args) =>
            {
                var btn = s as ButtonBase;
                WpfRecorder.RecordClick(
                    btn?.GetType().Name ?? "Button",
                    btn?.Name,
                    (btn as Button)?.Content?.ToString());
            };
        }
    }
}
```

Usage in XAML:

```xml
<Button Content="Save"
        local:RecordingBehavior.RecordClicks="True" />
```

---

## Recording HTTP Calls

### Using the Recording HttpClient

The simplest approach is to use the pre-configured HttpClient:

```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        // Creates HttpClient with recording handler
        _httpClient = WpfRecorder.CreateHttpClient();
    }

    public async Task<User> GetUserAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/users/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<User>();
    }
}
```

### Custom Handler Configuration

For more control, create the handler manually:

```csharp
using WpfEventRecorder.Core.Hooks;

// Create with custom settings
var handler = new RecordingHttpHandler(
    innerHandler: new HttpClientHandler
    {
        // Your custom handler settings
        AutomaticDecompression = DecompressionMethods.GZip
    },
    captureRequestBody: true,
    captureResponseBody: true,
    maxBodySize: 512 * 1024  // 512KB limit
);

// Register with the hub
RecordingHub.Instance.SetHttpHandler(handler);

// Create HttpClient
var client = new HttpClient(handler)
{
    BaseAddress = new Uri("https://api.example.com")
};
```

### Subscribing to HTTP Events

React to HTTP events in real-time:

```csharp
var handler = new RecordingHttpHandler();

// Subscribe to requests
handler.Requests.Subscribe(entry =>
{
    Console.WriteLine($"Request: {entry.ApiInfo.Method} {entry.ApiInfo.Url}");
});

// Subscribe to responses
handler.Responses.Subscribe(entry =>
{
    Console.WriteLine($"Response: {entry.ApiInfo.StatusCode} ({entry.DurationMs}ms)");
});

// Subscribe to all events
handler.AllEvents.Subscribe(entry =>
{
    // Handle both requests and responses
});
```

### Correlation with UI Events

Link UI actions to their resulting API calls:

```csharp
private async void RefreshButton_Click(object sender, RoutedEventArgs e)
{
    // Generate a correlation ID for this action
    var correlationId = WpfRecorder.NewCorrelationId();

    // Record the UI event (automatically uses the correlation ID)
    WpfRecorder.RecordClick("Button", "RefreshButton", "Refresh");

    // API calls made now will share the correlation ID
    await _apiService.RefreshDataAsync();
}
```

---

## Working with the RecordingHub

### Singleton Access

```csharp
using WpfEventRecorder.Core.Services;

var hub = RecordingHub.Instance;
```

### Session Management

```csharp
// Start a named session
hub.Start("Integration Test - Login Flow");

// Check recording state
if (hub.IsRecording)
{
    // Recording is active
}

// Get current session info
var session = hub.CurrentSession;
Console.WriteLine($"Session: {session?.Name}");
Console.WriteLine($"Started: {session?.StartTime}");
Console.WriteLine($"Events: {hub.EntryCount}");

// Stop recording
hub.Stop();
Console.WriteLine($"Duration: {session?.Duration}");
```

### Event Subscription

```csharp
// Subscribe to recording state changes
hub.RecordingStateChanged += (sender, isRecording) =>
{
    UpdateStatusIndicator(isRecording);
};

// Subscribe to new entries
hub.EntryRecorded += (sender, entry) =>
{
    AddToEventList(entry);
};

// Use Reactive Extensions
hub.Entries
    .Where(e => e.EntryType == RecordEntryType.ApiResponse)
    .Where(e => e.ApiInfo?.StatusCode >= 400)
    .Subscribe(e => LogError(e));
```

### Accessing Recorded Data

```csharp
// Get all entries
var entries = hub.GetEntries();

// Filter by type
var uiEvents = entries.Where(e => e.UIInfo != null);
var apiEvents = entries.Where(e => e.ApiInfo != null);

// Find slow API calls
var slowCalls = entries
    .Where(e => e.EntryType == RecordEntryType.ApiResponse)
    .Where(e => e.DurationMs > 1000);
```

### Saving and Loading

```csharp
// Save to file
hub.SaveToFile(@"C:\Recordings\session.json");

// Export as string
string json = hub.ExportAsJson();

// Load from file
var loadedSession = hub.LoadFromFile(@"C:\Recordings\session.json");
```

---

## Extending the Recorder

### Custom Event Types

Add new event types by extending the enum and models:

```csharp
// In your application, create wrapper methods
public static class CustomRecorder
{
    public static void RecordDragDrop(string source, string target, object data)
    {
        WpfRecorder.Hub.AddEntry(new RecordEntry
        {
            EntryType = RecordEntryType.UIDragDrop,
            UIInfo = new UIInfo
            {
                ControlType = "DragDrop",
                ControlName = source,
                Text = target,
                Properties = new Dictionary<string, string>
                {
                    ["DataType"] = data?.GetType().Name ?? "null",
                    ["Data"] = data?.ToString() ?? ""
                }
            }
        });
    }

    public static void RecordGesture(string gestureName, Point location)
    {
        WpfRecorder.Hub.AddEntry(new RecordEntry
        {
            EntryType = RecordEntryType.Custom,
            Metadata = JsonSerializer.Serialize(new
            {
                Type = "Gesture",
                Name = gestureName,
                X = location.X,
                Y = location.Y
            })
        });
    }
}
```

### Custom HTTP Handler

Extend the recording handler for special requirements:

```csharp
public class CustomRecordingHandler : RecordingHttpHandler
{
    private readonly ILogger _logger;

    public CustomRecordingHandler(ILogger logger)
        : base(captureRequestBody: true, captureResponseBody: true)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add custom headers
        request.Headers.Add("X-Recording-Session",
            RecordingHub.Instance.CurrentSession?.SessionId.ToString());

        // Log before request
        _logger.LogInformation("HTTP Request: {Method} {Uri}",
            request.Method, request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken);

        // Log after response
        _logger.LogInformation("HTTP Response: {StatusCode}",
            response.StatusCode);

        return response;
    }
}
```

### Creating Recording Middleware

For ASP.NET Core applications receiving requests:

```csharp
public class RecordingMiddleware
{
    private readonly RequestDelegate _next;

    public RecordingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Record incoming request
        var requestEntry = new RecordEntry
        {
            EntryType = RecordEntryType.ApiRequest,
            ApiInfo = new ApiInfo
            {
                Method = context.Request.Method,
                Url = context.Request.GetDisplayUrl(),
                Path = context.Request.Path
            }
        };
        WpfRecorder.Hub.AddEntry(requestEntry);

        await _next(context);

        stopwatch.Stop();

        // Record response
        var responseEntry = new RecordEntry
        {
            EntryType = RecordEntryType.ApiResponse,
            CorrelationId = requestEntry.CorrelationId,
            DurationMs = stopwatch.ElapsedMilliseconds,
            ApiInfo = new ApiInfo
            {
                Method = context.Request.Method,
                Url = context.Request.GetDisplayUrl(),
                StatusCode = context.Response.StatusCode,
                IsSuccess = context.Response.StatusCode < 400
            }
        };
        WpfRecorder.Hub.AddEntry(responseEntry);
    }
}
```

---

## Building and Debugging

### Building the VSIX

```bash
# Debug build
msbuild WpfEventRecorder/WpfEventRecorder.csproj /p:Configuration=Debug

# Release build
msbuild WpfEventRecorder/WpfEventRecorder.csproj /p:Configuration=Release
```

The VSIX file is generated at:
`WpfEventRecorder/bin/Release/WpfEventRecorder.vsix`

### Debugging the Extension

1. Set `WpfEventRecorder` as startup project
2. Press F5
3. Visual Studio Experimental Instance launches
4. Test the extension in the experimental instance

### Debugging the Core Library

1. Set `WpfEventRecorder.SampleApp` as startup project
2. Set breakpoints in Core library code
3. Press F5
4. Interact with the sample app to trigger breakpoints

### Logging and Diagnostics

```csharp
// Enable verbose logging in debug builds
#if DEBUG
hub.EntryRecorded += (s, e) =>
{
    Debug.WriteLine($"[Recorder] {e.EntryType}: {e.Timestamp}");
};
#endif
```

---

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~RecordingHubTests"
```

### Writing Unit Tests

```csharp
public class MyRecordingTests
{
    [Fact]
    public void RecordClick_AddsEntryToHub()
    {
        // Arrange
        var hub = RecordingHub.Instance;
        hub.Clear();
        hub.Start();

        // Act
        WpfRecorder.RecordClick("Button", "TestBtn", "Test");

        // Assert
        Assert.Equal(1, hub.EntryCount);
        var entry = hub.GetEntries().First();
        Assert.Equal(RecordEntryType.UIClick, entry.EntryType);
        Assert.Equal("Button", entry.UIInfo?.ControlType);

        // Cleanup
        hub.Stop();
        hub.Clear();
    }
}
```

### Integration Testing

```csharp
public class HttpRecordingIntegrationTests
{
    [Fact]
    public async Task HttpClient_RecordsRequestAndResponse()
    {
        // Arrange
        var hub = RecordingHub.Instance;
        hub.Clear();
        hub.Start();

        var client = WpfRecorder.CreateHttpClient();

        // Act
        await client.GetAsync("https://httpbin.org/get");

        // Assert
        var entries = hub.GetEntries();
        Assert.Contains(entries, e => e.EntryType == RecordEntryType.ApiRequest);
        Assert.Contains(entries, e => e.EntryType == RecordEntryType.ApiResponse);

        // Cleanup
        hub.Stop();
    }
}
```

---

## Deployment

### Publishing the VSIX

1. Update version in `source.extension.vsixmanifest`
2. Build in Release configuration
3. Test the VSIX file manually
4. Upload to Visual Studio Marketplace

### Publishing the Core Library

```bash
# Pack the NuGet package
dotnet pack WpfEventRecorder.Core -c Release

# Push to NuGet
dotnet nuget push WpfEventRecorder.Core/bin/Release/*.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json
```

---

## Best Practices

### Performance

- **Disable body capture** for large payloads in production
- **Use async/await** throughout to avoid blocking
- **Clear recordings** periodically during long sessions
- **Filter events** if only certain types are needed

### Security

- **Never commit recordings** containing sensitive data
- **Sanitize recordings** before sharing
- **Exclude credentials** from HTTP body capture
- **Use secure storage** for recording files

### Code Quality

- **Use meaningful control names** in XAML
- **Set AutomationId** on important controls
- **Handle recorder disposal** properly
- **Test recording functionality** as part of CI

### Debugging

- **Check IsRecording** before expecting events
- **Verify handler registration** for HTTP recording
- **Use correlation IDs** to trace related events
- **Subscribe to events** for real-time monitoring

---

## Support

- **GitHub Issues**: Report bugs and request features
- **Pull Requests**: Contributions welcome
- **Documentation**: Keep docs updated with changes
