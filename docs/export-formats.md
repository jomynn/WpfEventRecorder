# Export Formats Guide

WPF Event Recorder supports multiple export formats to help you integrate recorded events into your testing workflow.

## Available Formats

| Format | Extension | Use Case |
|--------|-----------|----------|
| JSON | `.json` | Data exchange, playback, analysis |
| CSV | `.csv` | Spreadsheet analysis, data processing |
| Excel | `.xml` | Excel-compatible XML spreadsheet |
| MSTest | `.cs` | Visual Studio MSTest unit tests |
| NUnit | `.cs` | NUnit framework tests |
| xUnit | `.cs` | xUnit framework tests |
| Playwright | `.cs` | Playwright-style selectors (conceptual) |

## JSON Export

The JSON format provides a complete recording session with all metadata.

### Structure

```json
{
  "sessionId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "sessionName": "Customer Form Test",
  "startTime": "2024-01-15T10:30:00.000Z",
  "endTime": "2024-01-15T10:35:00.000Z",
  "targetWindow": {
    "title": "Customer Management",
    "processName": "CustomerApp",
    "handle": 12345
  },
  "entries": [
    {
      "id": "guid",
      "timestamp": "2024-01-15T10:30:01.123Z",
      "entryType": "UITextInput",
      "correlationId": "guid",
      "uiInfo": {
        "controlType": "TextBox",
        "controlName": "txtCustomerName",
        "automationId": "CustomerNameInput",
        "text": null,
        "oldValue": "",
        "newValue": "John Doe",
        "windowTitle": "Customer Management",
        "visualTreePath": "Text:Name",
        "screenPosition": { "x": 450, "y": 230 }
      }
    },
    {
      "id": "guid",
      "timestamp": "2024-01-15T10:30:02.456Z",
      "entryType": "UIClick",
      "correlationId": "guid",
      "uiInfo": {
        "controlType": "Button",
        "controlName": "btnSave",
        "automationId": "SaveButton",
        "contentText": "Save",
        "windowTitle": "Customer Management",
        "screenPosition": { "x": 500, "y": 400 }
      }
    },
    {
      "id": "guid",
      "timestamp": "2024-01-15T10:30:02.500Z",
      "entryType": "ApiRequest",
      "correlationId": "guid",
      "apiInfo": {
        "method": "POST",
        "url": "https://api.example.com/customers",
        "requestHeaders": {
          "Content-Type": "application/json"
        },
        "requestBody": "{\"name\":\"John Doe\",\"email\":\"john@example.com\"}"
      }
    },
    {
      "id": "guid",
      "timestamp": "2024-01-15T10:30:02.750Z",
      "entryType": "ApiResponse",
      "correlationId": "guid",
      "durationMs": 250,
      "apiInfo": {
        "method": "POST",
        "url": "https://api.example.com/customers",
        "statusCode": 201,
        "responseHeaders": {
          "Content-Type": "application/json"
        },
        "responseBody": "{\"id\":42,\"name\":\"John Doe\",\"email\":\"john@example.com\"}",
        "isSuccess": true
      }
    }
  ]
}
```

### Usage

```csharp
using WpfEventRecorder.Services;

// Export to JSON
ExportService.ExportToJson(entries, "recording.json", session);

// Or use WpfRecorder directly
WpfRecorder.SaveToFile("recording.json");
```

## CSV Export

CSV format is ideal for data analysis in spreadsheets or data processing tools.

### Columns

- Timestamp
- Type
- ControlType
- ControlName
- AutomationId
- Text
- ContentText
- OldValue
- NewValue
- WindowTitle
- VisualTreePath
- ScreenX
- ScreenY
- KeyCombination
- Properties
- Method
- URL
- StatusCode
- Duration
- CorrelationId

### Example Output

```csv
Timestamp,Type,ControlType,ControlName,AutomationId,Text,ContentText,OldValue,NewValue,WindowTitle,...
2024-01-15 10:30:01.123,UITextInput,TextBox,txtCustomerName,CustomerNameInput,,,,"John Doe",Customer Management,...
2024-01-15 10:30:02.456,UIClick,Button,btnSave,SaveButton,,Save,,,Customer Management,...
```

### Usage

```csharp
ExportService.ExportToCsv(entries, "recording.csv");
```

## Excel Export

Exports as XML Spreadsheet format that can be opened directly in Microsoft Excel.

### Features

- Formatted headers with bold styling
- Column widths optimized for content
- Error rows highlighted in red
- Date/time formatting

### Usage

```csharp
ExportService.ExportToExcel(entries, "recording.xml");
```

## MSTest Export

Generates a C# test class compatible with Microsoft's MSTest framework.

### Generated Code Structure

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RecordedTests
{
    /// <summary>
    /// Tests generated from WPF Event Recorder session: Customer Form Test
    /// Recorded on: 2024-01-15 10:30:00
    /// </summary>
    [TestClass]
    public class Customer_Form_TestTests
    {
        private HttpClient _httpClient;

        [TestInitialize]
        public void Setup()
        {
            _httpClient = new HttpClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient?.Dispose();
        }

        [TestMethod]
        public async Task Test_Customer_Form_Test_20240115_103000()
        {
            // Arrange
            // TODO: Initialize your ViewModels and services here

            // Act - Recorded UI Interactions
            // Text input: TextBox#txtCustomerName
            // Binding: Text:Name
            // vm.Name = "John Doe";

            // Click: Button#btnSave
            // vm.SaveCommand.Execute(null);

            // API POST https://api.example.com/customers
            // Request: {"name":"John Doe","email":"john@example.com"}
            // API Response: 201 (250ms)

            // Assert
            // TODO: Add your assertions here
        }
    }
}
```

### Usage

```csharp
ExportService.ExportToMSTest(entries, "RecordedTests.cs", session);
```

## NUnit Export

Generates a C# test class compatible with the NUnit framework.

### Generated Code Structure

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RecordedTests
{
    /// <summary>
    /// Tests generated from WPF Event Recorder session: Customer Form Test
    /// Recorded on: 2024-01-15 10:30:00
    /// </summary>
    [TestFixture]
    public class Customer_Form_TestTests
    {
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _httpClient = new HttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public async Task Test_Customer_Form_Test_20240115_103000()
        {
            // Recorded test steps...
        }
    }
}
```

### Usage

```csharp
ExportService.ExportToNUnit(entries, "RecordedTests.cs", session);
```

## xUnit Export

Generates a C# test class compatible with the xUnit framework.

### Generated Code Structure

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RecordedTests
{
    /// <summary>
    /// Tests generated from WPF Event Recorder session: Customer Form Test
    /// Recorded on: 2024-01-15 10:30:00
    /// </summary>
    public class Customer_Form_TestTests : IDisposable
    {
        private readonly HttpClient _httpClient;

        public Customer_Form_TestTests()
        {
            _httpClient = new HttpClient();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        [Fact]
        public async Task Test_Customer_Form_Test_20240115_103000()
        {
            // Recorded test steps...
        }
    }
}
```

### Usage

```csharp
ExportService.ExportToXUnit(entries, "RecordedTests.cs", session);
```

## Playwright Export

Generates Playwright-style assertions as comments. These are conceptual and meant for reference when creating web-based tests or when using WPF automation tools like FlaUI.

### Generated Code Structure

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace RecordedTests
{
    /// <summary>
    /// Playwright-style tests generated from WPF Event Recorder session
    /// Note: These are conceptual Playwright-style assertions for WPF automation
    /// </summary>
    public class Customer_Form_TestTests
    {
        [Fact]
        public async Task Test_Customer_Form_Test()
        {
            // Note: This is a Playwright-style representation of the recorded WPF interactions
            // For actual WPF automation, consider using FlaUI or similar libraries

            // await page.Locator("[data-automation-id='CustomerNameInput']").FillAsync("John Doe");
            // await page.Locator("[data-automation-id='SaveButton']").ClickAsync();
            // await Expect(page).ToHaveRequestAsync(request =>
            //     request.Method == "POST" &&
            //     request.Url.Contains("https://api.example.com/customers"));
            // await Expect(response).ToHaveStatusCodeAsync(201);
        }
    }
}
```

### Selector Mapping

The export uses these mappings for Playwright-style selectors:

| WPF Property | Playwright Selector |
|--------------|---------------------|
| AutomationId | `[data-automation-id='value']` |
| Name (x:Name) | `#value` |
| Content | `text=value` |
| ControlType | `controltype` (lowercase) |

### Usage

```csharp
ExportService.ExportToPlaywright(entries, "RecordedTests.cs", session);
```

## Programmatic Export

Use the `ExportService` to export programmatically:

```csharp
using WpfEventRecorder.Services;

var entries = RecordingHub.Instance.GetEntries();
var session = RecordingHub.Instance.CurrentSession;

// Use the unified Export method
ExportService.Export(entries, "output.json", ExportFormat.Json, session);
ExportService.Export(entries, "output.cs", ExportFormat.MSTest, session);
ExportService.Export(entries, "output.cs", ExportFormat.NUnit, session);
ExportService.Export(entries, "output.cs", ExportFormat.XUnit, session);
ExportService.Export(entries, "output.cs", ExportFormat.Playwright, session);
ExportService.Export(entries, "output.csv", ExportFormat.Csv, session);
ExportService.Export(entries, "output.xml", ExportFormat.Excel, session);
```

## Best Practices

### For Test Generation

1. **Use AutomationId**: Set `AutomationProperties.AutomationId` in XAML for reliable selectors
2. **Meaningful Names**: Use descriptive control names for readable test code
3. **Group Related Actions**: Use correlation IDs to track related UI and API actions
4. **Review Generated Code**: Always review and enhance generated test code

### For Data Analysis

1. **Use CSV for Filtering**: Import CSV into Excel or data tools for analysis
2. **Use JSON for Playback**: JSON preserves all metadata for potential playback
3. **Archive Sessions**: Keep JSON exports for regression testing reference

### For Team Collaboration

1. **Version Control**: Store exports in source control for team review
2. **Naming Convention**: Use descriptive session names like "Login_HappyPath"
3. **Documentation**: Add comments to explain complex interaction sequences
