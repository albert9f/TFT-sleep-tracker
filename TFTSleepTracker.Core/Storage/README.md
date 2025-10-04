# CSV Logging and Summary Store

This document describes how to use the CSV logging and summary store functionality.

## Overview

The storage system provides:
- **Daily CSV files** with activity data points (timestamp, is_active, inactivity_minutes, sleep_minutes_increment)
- **Summary JSON file** with daily aggregated statistics
- **Atomic writes** to prevent data corruption
- **Integration with LastInputMonitor** for automatic tracking

## Quick Start

```csharp
using TFTSleepTracker.Core.Storage;
using TFTSleepTracker.Core.Logic;

// Define your sleep window (e.g., 11 PM to 8 AM)
var nightlyWindow = new LocalTimeWindow(
    new TimeOnly(23, 0),  // 11 PM
    new TimeOnly(8, 0)    // 8 AM
);

// Define inactivity threshold (e.g., 60 minutes)
var threshold = TimeSpan.FromMinutes(60);

// Create and start the tracker
using var tracker = new ActivityTracker(
    dataDirectory: @"C:\TFTSleepData",
    inactivityThreshold: threshold,
    nightlyWindow: nightlyWindow
);

tracker.Start();

// Let it run...
// Activity is monitored every 30 seconds

// When done
tracker.Stop();
```

## Components

### ActivityDataPoint

Represents a single data point in the CSV:

```csharp
public class ActivityDataPoint
{
    public DateTimeOffset Timestamp { get; set; }
    public bool IsActive { get; set; }
    public double InactivityMinutes { get; set; }
    public int SleepMinutesIncrement { get; set; }
}
```

### DailySummary

Represents aggregated daily data in summary.json:

```csharp
public class DailySummary
{
    public DateOnly Date { get; set; }
    public int TotalSleepMinutes { get; set; }
    public double TotalActiveMinutes { get; set; }
    public double TotalInactiveMinutes { get; set; }
    public int DataPointCount { get; set; }
}
```

### CsvLogger

Handles writing to daily CSV files:

```csharp
var logger = new CsvLogger(@"C:\TFTSleepData");

// Append a data point
await logger.AppendDataPointAsync(dataPoint);

// Read data points for a date
var today = DateOnly.FromDateTime(DateTime.Now);
var dataPoints = await logger.GetDataPointsAsync(today);
```

### SummaryStore

Handles reading/writing summary.json:

```csharp
var store = new SummaryStore(@"C:\TFTSleepData");

// Get a specific summary
var today = DateOnly.FromDateTime(DateTime.Now);
var summary = await store.GetSummaryAsync(today);

// Update a summary
await store.UpdateSummaryAsync(summary);

// Get all summaries
var allSummaries = await store.GetAllSummariesAsync();
```

### ActivityTracker

Coordinates everything:

```csharp
var tracker = new ActivityTracker(
    dataDirectory: @"C:\TFTSleepData",
    inactivityThreshold: TimeSpan.FromMinutes(60),
    nightlyWindow: new LocalTimeWindow(
        new TimeOnly(23, 0),
        new TimeOnly(8, 0)
    )
);

tracker.Start();  // Starts monitoring
// ...
tracker.Stop();   // Stops monitoring
tracker.Dispose(); // Cleanup
```

## File Structure

```
C:\TFTSleepData\
├── 2025-10-01.csv
├── 2025-10-02.csv
├── 2025-10-03.csv
└── summary.json
```

### CSV File Format

Each daily CSV has the following columns:

```csv
timestamp,is_active,inactivity_minutes,sleep_minutes_increment
2025-10-04T19:13:44.9790931+00:00,true,0.50,0
2025-10-04T19:14:14.9790931+00:00,false,5.20,0
2025-10-04T19:14:44.9790931+00:00,false,65.50,5
```

- **timestamp**: ISO 8601 format with timezone
- **is_active**: true if inactivity < threshold, false otherwise
- **inactivity_minutes**: Current inactivity duration in minutes (decimal)
- **sleep_minutes_increment**: Sleep minutes added in this interval (integer)

### Summary JSON Format

```json
{
  "2025-10-04": {
    "date": "2025-10-04",
    "totalSleepMinutes": 420,
    "totalActiveMinutes": 600.5,
    "totalInactiveMinutes": 240.0,
    "dataPointCount": 480
  },
  "2025-10-05": {
    "date": "2025-10-05",
    "totalSleepMinutes": 450,
    "totalActiveMinutes": 580.0,
    "totalInactiveMinutes": 260.5,
    "dataPointCount": 480
  }
}
```

## Atomic Writes

Both `CsvLogger` and `SummaryStore` use atomic writes to prevent data corruption:

1. Write to a temporary file (e.g., `2025-10-04.csv.tmp`)
2. Move the temp file to the final location (atomic operation)
3. Clean up temp file on error

This ensures that files are never left in a partially-written state.

## Data Flow

```
LastInputMonitor (30s interval)
    ↓
InactivityCheck Event
    ↓
ActivityTracker.OnInactivityCheck
    ↓
    ├→ Calculate sleep minutes (using SleepCalculator)
    ├→ Create ActivityDataPoint
    ├→ CsvLogger.AppendDataPointAsync
    └→ SummaryStore.UpdateSummaryAsync
```

## Sleep Calculation

Sleep minutes are calculated using the `SleepCalculator`:

1. Continuous inactivity spans are identified
2. Spans are intersected with the nightly window
3. For each intersection: `sleep = max(0, duration - threshold)`
4. Only increments are logged in the CSV (not cumulative)

## Notes

- Monitoring happens every 30 seconds (LastInputMonitor interval)
- The data directory is created automatically if it doesn't exist
- Temp files are cleaned up on errors
- The system is thread-safe for concurrent reads/writes
- Sleep calculation respects midnight-crossing windows (e.g., 11 PM to 8 AM)
