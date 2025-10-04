# LastInputMonitor Usage Example

The `LastInputMonitor` class provides functionality to monitor user input inactivity on Windows systems using the GetLastInputInfo Win32 API.

## Features

- **GetInactivity()**: Static method that returns a `TimeSpan` representing the time elapsed since the last user input
- **Background Monitoring**: Automatic monitoring with events fired every 30 seconds
- **Event-driven**: Subscribe to `InactivityCheck` event to receive regular updates

## Usage Examples

### Getting Current Inactivity

```csharp
using TFTSleepTracker.Core;

// Get the current inactivity duration
TimeSpan inactivity = LastInputMonitor.GetInactivity();
Console.WriteLine($"User has been inactive for {inactivity.TotalSeconds} seconds");
```

### Background Monitoring with Events

```csharp
using TFTSleepTracker.Core;

// Create and start the monitor
using var monitor = new LastInputMonitor();

// Subscribe to the event
monitor.InactivityCheck += (sender, args) =>
{
    Console.WriteLine($"Time: {args.NowLocal:HH:mm:ss}");
    Console.WriteLine($"Inactivity: {args.Inactivity.TotalMinutes:F2} minutes");
};

// Start monitoring (fires event every 30 seconds)
monitor.StartMonitoring();

// ... your application logic ...

// Stop monitoring when done
monitor.StopMonitoring();

// Or simply dispose (which stops monitoring automatically)
```

### Using with IDisposable Pattern

```csharp
using TFTSleepTracker.Core;

using (var monitor = new LastInputMonitor())
{
    monitor.InactivityCheck += HandleInactivity;
    monitor.StartMonitoring();
    
    // Application runs...
    
    // Dispose automatically calls StopMonitoring()
}

void HandleInactivity(object? sender, InactivityEventArgs e)
{
    if (e.Inactivity > TimeSpan.FromMinutes(5))
    {
        Console.WriteLine("User has been inactive for more than 5 minutes!");
    }
}
```

## InactivityEventArgs Properties

- `NowLocal`: `DateTime` - The current local time when the event was fired
- `Inactivity`: `TimeSpan` - The duration of user inactivity

## Notes

- This functionality requires Windows and uses the `user32.dll` library
- The background monitoring loop fires events every 30 seconds
- Events are fired on a background thread
- The monitor implements `IDisposable` and should be properly disposed
