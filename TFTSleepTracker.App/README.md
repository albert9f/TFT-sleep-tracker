# TFT Sleep Tracker - Windows Application

A WPF-based Windows application for tracking sleep patterns while away from keyboard, inspired by TeamFight Tactics aesthetics.

## Features

### 🎯 Issue 06: Tray Application with NotifyIcon
- **System Tray Integration**: Application runs in the system tray for minimal disruption
- **Background Tracking**: Continues monitoring activity even when window is hidden
- **Single Instance**: Uses named mutex to ensure only one instance runs at a time
- **Hide on Close**: Pressing "X" hides the window instead of closing the app
- **Tray Menu Options**:
  - **Open**: Show the main window
  - **Send Now**: Manually trigger yesterday's summary computation and upload
  - **Check for Updates**: Check for application updates (placeholder)
  - **Quit Background**: Exit the application with confirmation dialog

### 🎨 Issue 07: TFT-Inspired Theme
- **Color Palette**:
  - Dark Blue Background: `#0A1428`, `#162C4C`
  - Accent Blue: `#1E5F8C`
  - Gold Highlights: `#C89B3C`, `#F0E6D2`
  - Text White: `#E8E8E8`
- **Gradient Backgrounds**: Smooth vertical gradients from dark to mid blue
- **Card-Based Layout**: Content organized in styled cards with gold borders
- **TFT-Style Typography**: Bold gold headers with light body text

### 🚀 Issue 08: Autostart at Login
- **First-Run Setup**: Automatically enables autostart on first launch
- **Registry Integration**: Uses `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- **Settings Toggle**: Checkbox in UI to enable/disable autostart
- **Helper Class**: `AutostartHelper.cs` provides static methods for managing autostart

### ⏰ Issue 09: Daily Summary Scheduler
- **Scheduled Upload**: Automatically computes yesterday's sleep summary at 08:05 AM daily
- **Sleep Calculation**: Uses `SleepCalculator` to compute sleep time from activity data
- **Yesterday's Data**: Processes previous day's CSV data and updates summary.json
- **Send Now Feature**: Manual trigger via tray menu for immediate processing
- **Event-Driven**: `SummaryReady` event fires when summary is computed and ready for upload

## Architecture

### Key Components

#### `App.xaml.cs`
- Application entry point and lifecycle management
- Single instance enforcement via Mutex
- Initializes `ActivityTracker` and `DailySummaryScheduler`
- Handles autostart setup on first run

#### `MainWindow.xaml` / `MainWindow.xaml.cs`
- Main application window with TFT-themed UI
- NotifyIcon integration for system tray
- Override `OnClosing` to hide instead of close
- Autostart settings toggle
- Statistics display (status, sleep time, active/inactive time)

#### `AutostartHelper.cs`
Static helper class for Windows registry autostart management:
```csharp
bool IsAutostartEnabled()    // Check if autostart is enabled
void EnableAutostart()       // Add registry entry
void DisableAutostart()      // Remove registry entry
void ToggleAutostart()       // Toggle on/off
```

#### `DailySummaryScheduler.cs`
Manages daily summary computation and scheduling:
- Schedules tasks for 08:05 AM daily using `Timer`
- Computes sleep minutes from CSV data using `SleepCalculator`
- Updates `summary.json` with yesterday's totals
- Raises `SummaryReady` event for upload handling
- `SendNowAsync()` method for manual triggering

## Data Flow

1. **Tracking**: `ActivityTracker` monitors user input every 30 seconds
2. **Storage**: Data points written to daily CSV files (`YYYY-MM-DD.csv`)
3. **Scheduling**: At 08:05 AM, `DailySummaryScheduler` triggers
4. **Computation**: Reads yesterday's CSV, computes sleep using `SleepCalculator`
5. **Summary Update**: Updates `summary.json` with computed totals
6. **Upload Event**: Fires `SummaryReady` event (upload logic can be attached)

## Configuration

### Default Settings
- **Data Directory**: `%APPDATA%\TFTSleepTracker`
- **Inactivity Threshold**: 5 minutes
- **Nightly Window**: 23:00 (11 PM) to 08:00 (8 AM)
- **Scheduler Time**: 08:05 AM local time

### Registry Key
- **Path**: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- **Key Name**: `TFTSleepTracker`
- **Value**: Full path to executable with quotes

## UI Components

### Status Card
- Shows current tracking status (Active/Inactive)
- Displays last check timestamp

### Today's Stats Card
- Sleep Time (hours)
- Active Time (hours)
- Inactive Time (hours)

### Settings Card
- Autostart toggle checkbox
- Information about data storage and upload schedule

## Building and Running

```bash
# Build the application
dotnet build

# Run the application
dotnet run --project TFTSleepTracker.App

# Publish for deployment
dotnet publish -c Release -r win-x64 --self-contained
```

## Testing

All existing tests pass:
```bash
cd /path/to/TFTSleepTracker
dotnet test
```

## Future Enhancements

- [ ] Custom icon for NotifyIcon
- [ ] Real-time statistics updates in UI
- [ ] Upload functionality implementation
- [ ] Update checking implementation
- [ ] Toast notifications for daily summaries
- [ ] Settings persistence for custom thresholds

## Notes

- Application requires Windows OS (uses Win32 APIs and Windows Forms)
- First run automatically enables autostart (can be disabled in settings)
- Window stays hidden in tray by default after initial setup
- All data stored locally until upload implementation is added
