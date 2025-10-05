# Issue Fix Summary - Version 1.0.1

## Overview
This document summarizes the fixes applied to address multiple issues in the TFT Sleep Tracker application.

## Issues Addressed

### 1. AppSettings Persistence Bug ✅
**Problem:**
- `AppSettingsStore.LoadAsync()` was overwriting saved settings with hardcoded values on every load
- DeviceId was being regenerated on each load instead of persisting
- Tests were failing: `SaveAsync_PersistsSettings` test failed because BotHost and Token were being overridden

**Solution:**
- Removed hardcoded override logic for BotHost and Token in `LoadAsync()`
- DeviceId is now only generated if it doesn't exist in storage
- Settings are properly serialized and deserialized without modification

**Files Changed:**
- `TFTSleepTracker.Core/Storage/AppSettings.cs`

**Code Changes:**
```csharp
// Before: Always overwrote settings
if (settings != null)
{
    settings.BotHost = "http://35.212.220.200:8080";
    settings.Token = "weatheryETHAN";
    await SaveAsync(settings);
}

// After: Only generate DeviceId if missing
if (settings != null && string.IsNullOrEmpty(settings.DeviceId))
{
    settings.DeviceId = GenerateDeviceId();
    await SaveAsync(settings);
}
```

**Test Results:**
- ✅ `LoadAsync_CreatesDefaultSettingsWithDeviceId` - PASS
- ✅ `SaveAsync_PersistsSettings` - PASS (was failing before)
- ✅ `LoadAsync_PreservesDeviceIdAcrossLoads` - PASS
- ✅ `SaveAsync_UpdatesLastUpdateCheck` - PASS

### 2. Sleep Calculation for PC Power-Off ✅
**Problem:**
- When PC is powered off between 11pm and 8am (sleep hours), no data points are recorded
- This resulted in 0 sleep hours being reported, even though the user was actually sleeping
- Client complained that overnight shutdowns weren't being counted as sleep

**Solution:**
- Modified `DailySummaryScheduler.ProcessCompletedSummariesAsync()` to detect gaps in data
- Gaps between the last data point of one day and first data point of next day are now treated as inactive intervals
- These gap intervals are passed to `SleepCalculator` which counts them as sleep if they fall within the nightly window (11pm-8am)

**Files Changed:**
- `TFTSleepTracker.App/DailySummaryScheduler.cs`

**Implementation Details:**
```csharp
// Check for gap from previous day's last point to today's first point
if (previousDayDataPoints.Count > 0 && dataPoints.Count > 0)
{
    var lastPreviousPoint = previousDayDataPoints[previousDayDataPoints.Count - 1];
    var firstTodayPoint = dataPoints[0];
    var gapDuration = firstTodayPoint.Timestamp - lastPreviousPoint.Timestamp;
    
    // If gap > 30 seconds, treat as inactive interval
    if (gapDuration.TotalSeconds > 30)
    {
        intervals.Insert(0, (lastPreviousPoint.Timestamp, firstTodayPoint.Timestamp, false));
    }
}
```

**Impact:**
- PC powered off at 11pm and turned on at 8am will now correctly count ~9 hours of sleep (minus threshold)
- Gaps in data during sleep hours are now properly accounted for

### 3. App Starts in Background ✅
**Problem:**
- App window was visible on startup, getting in the way of user's workflow
- Client complained about intrusive startup behavior
- Window should only appear when user explicitly opens it from system tray

**Solution:**
- Removed `StartupUri="MainWindow.xaml"` from `App.xaml`
- Created MainWindow instance in `App.OnStartup()` but don't call `Show()`
- Window remains hidden until user clicks "Open" from system tray icon

**Files Changed:**
- `TFTSleepTracker.App/App.xaml`
- `TFTSleepTracker.App/App.xaml.cs`

**Code Changes:**
```csharp
// Before: App.xaml had StartupUri that automatically showed window
<Application StartupUri="MainWindow.xaml">

// After: No StartupUri, window created but hidden
<Application>

// In App.xaml.cs OnStartup:
var mainWindow = new MainWindow();
MainWindow = mainWindow;
// Don't call Show() - window stays hidden in tray
```

**Behavior:**
- ✅ App starts silently in system tray
- ✅ Tray icon is visible with context menu
- ✅ User can click "Open" to show window when needed
- ✅ Background tracking continues regardless of window state

### 4. HasData Flag Added ✅
**Problem:**
- `SummaryReadyEventArgs` didn't indicate whether actual data existed for the day
- This is needed for "no sleep" message logic

**Solution:**
- Added `HasData` property to `SummaryReadyEventArgs`
- Set to `true` when data points exist and summary is computed

**Files Changed:**
- `TFTSleepTracker.App/DailySummaryScheduler.cs`

**Code Changes:**
```csharp
SummaryReady?.Invoke(this, new SummaryReadyEventArgs
{
    Summary = summary,
    Date = date,
    HasData = true  // ← Added this flag
});
```

### 5. Version Bump for Auto-Update ✅
**Problem:**
- Auto-update system needs version increment to detect and deploy new version

**Solution:**
- Bumped version from 1.0.0 to 1.0.1 in project file

**Files Changed:**
- `TFTSleepTracker.App/TFTSleepTracker.App.csproj`

**Code Changes:**
```xml
<Version>1.0.1</Version>  <!-- Was 1.0.0 -->
```

## Testing

### Unit Tests
All 37 unit tests pass:
```
Test summary: total: 37, failed: 0, succeeded: 37, skipped: 0
```

### Key Tests
- ✅ AppSettings persistence tests
- ✅ Sleep calculator tests (including DST handling)
- ✅ Upload queue tests
- ✅ No-sleep message tests

## Deployment

### Building the Update
To create an update package for auto-update:
```powershell
cd scripts
.\pack-squirrel.ps1
```

This will:
1. Build the application with version 1.0.1
2. Create a Squirrel release package
3. Upload to the update server

### Auto-Update Process
1. Running instances of TFT Sleep Tracker will check for updates hourly
2. When version 1.0.1 is detected, the update will be downloaded
3. On next restart, the application will auto-update to 1.0.1
4. All fixes will be applied automatically

## Files Modified

| File | Changes | Purpose |
|------|---------|---------|
| `TFTSleepTracker.Core/Storage/AppSettings.cs` | Removed hardcoded override logic | Fix settings persistence |
| `TFTSleepTracker.App/DailySummaryScheduler.cs` | Added gap detection logic | Count PC power-off as sleep |
| `TFTSleepTracker.App/App.xaml` | Removed StartupUri | Start hidden in background |
| `TFTSleepTracker.App/App.xaml.cs` | Create window without showing | Start hidden in background |
| `TFTSleepTracker.App/TFTSleepTracker.App.csproj` | Version bump | Enable auto-update |

## Verification

### Manual Testing Checklist
- [ ] Install version 1.0.1
- [ ] Verify app starts hidden (no window visible)
- [ ] Verify tray icon appears
- [ ] Click "Open" in tray menu - window should appear
- [ ] Close window (X button) - should hide, not exit
- [ ] Save custom BotHost/Token in settings.json
- [ ] Restart app - verify settings are preserved (not overwritten)
- [ ] Power off PC at 11pm, turn on at 8am
- [ ] Check next day's summary - should show ~9 hours sleep
- [ ] Verify DeviceId remains constant across restarts

### Auto-Update Verification
- [ ] Deploy version 1.0.1 to update server
- [ ] Wait for running instance to detect update
- [ ] Verify update downloads and applies on restart
- [ ] Check Event Viewer for "TFTSleepTracker" events
- [ ] Confirm all fixes are working in updated version

## Notes

### DeviceId Handling
- DeviceId is generated once on first run
- Persists across application restarts
- Uses format: `device-{guid}` (e.g., `device-abc123def456...`)
- Never regenerated unless explicitly deleted from settings.json

### Sleep Calculation Logic
- Inactivity threshold: 5 minutes
- Nightly window: 11pm to 8am (23:00 to 08:00)
- Gaps > 30 seconds are treated as inactivity
- Sleep calculation uses local time to handle DST correctly
- Formula: max(0, inactivityMinutes - thresholdMinutes)

### Background Operation
- App runs silently in system tray
- Activity tracking continues even when window is hidden
- System tray menu provides:
  - Open (show window)
  - Send Now (manual sync)
  - Check for Updates
  - Quit Background (exit app)

## Conclusion

All issues identified in the original problem statement have been addressed:
1. ✅ AppSettings persistence fixed - DeviceId and other settings now persist correctly
2. ✅ Sleep calculation fixed - PC power-off during sleep hours is now counted
3. ✅ App starts hidden - no longer intrusive on startup
4. ✅ Version bumped - ready for auto-update deployment

The changes are minimal, focused, and all tests pass. The application is ready for deployment via auto-update.
