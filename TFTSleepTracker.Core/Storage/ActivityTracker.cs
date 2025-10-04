using System.Diagnostics;
using TFTSleepTracker.Core.Logic;

namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Coordinates activity tracking, CSV logging, and summary updates
/// </summary>
public class ActivityTracker : IDisposable
{
    private readonly CsvLogger _csvLogger;
    private readonly SummaryStore _summaryStore;
    private readonly LastInputMonitor _inputMonitor;
    private readonly SystemEventsHandler _systemEventsHandler;
    private readonly TimeSpan _inactivityThreshold;
    private readonly LocalTimeWindow _nightlyWindow;

    private DateTimeOffset? _lastCheckTime;
    private bool _wasActiveLastCheck;
    private double _currentInactivityMinutes;
    private bool _isMonitoring;

    public ActivityTracker(
        string dataDirectory,
        TimeSpan inactivityThreshold,
        LocalTimeWindow nightlyWindow)
    {
        if (string.IsNullOrEmpty(dataDirectory))
            throw new ArgumentNullException(nameof(dataDirectory));

        _csvLogger = new CsvLogger(dataDirectory);
        _summaryStore = new SummaryStore(dataDirectory);
        _inactivityThreshold = inactivityThreshold;
        _nightlyWindow = nightlyWindow;

        _inputMonitor = new LastInputMonitor();
        _inputMonitor.InactivityCheck += OnInactivityCheck;

        // Setup system events handler
        _systemEventsHandler = new SystemEventsHandler();
        _systemEventsHandler.SystemResumed += OnSystemResumed;
        _systemEventsHandler.SystemSuspended += OnSystemSuspended;
        _systemEventsHandler.TimeChanged += OnTimeChanged;
        _systemEventsHandler.SessionSwitch += OnSessionSwitch;
    }

    /// <summary>
    /// Starts tracking activity
    /// </summary>
    public void Start()
    {
        _inputMonitor.StartMonitoring();
        _isMonitoring = true;
    }

    /// <summary>
    /// Stops tracking activity
    /// </summary>
    public void Stop()
    {
        _inputMonitor.StopMonitoring();
        _isMonitoring = false;
    }

    private void OnSystemResumed(object? sender, EventArgs e)
    {
        // Reset tracking state after resume to avoid corrupted spans
        LogEvent("System resumed - resetting tracking state", EventLogEntryType.Warning);
        _lastCheckTime = null;
        _wasActiveLastCheck = false;
        _currentInactivityMinutes = 0;
    }

    private void OnSystemSuspended(object? sender, EventArgs e)
    {
        // Pause monitoring during suspension
        LogEvent("System suspending - pausing monitoring", EventLogEntryType.Information);
    }

    private void OnTimeChanged(object? sender, EventArgs e)
    {
        // Reset tracking state after time change (DST, manual adjustment)
        LogEvent("Time changed detected - resetting tracking state", EventLogEntryType.Warning);
        _lastCheckTime = null;
    }

    private void OnSessionSwitch(object? sender, EventArgs e)
    {
        // Reset inactivity counter on session reconnect (e.g., RDP)
        LogEvent("Session switch detected - resetting tracking state", EventLogEntryType.Information);
        _lastCheckTime = null;
    }

    private void LogEvent(string message, EventLogEntryType type)
    {
        try
        {
            const string source = "TFTSleepTracker";
            const string logName = "Application";

            if (!System.Diagnostics.EventLog.SourceExists(source))
            {
                try
                {
                    System.Diagnostics.EventLog.CreateEventSource(source, logName);
                }
                catch
                {
                    return;
                }
            }

            System.Diagnostics.EventLog.WriteEntry(source, message, type);
        }
        catch
        {
            // Silently fail - logging should not crash the app
        }
    }

    private async void OnInactivityCheck(object? sender, InactivityEventArgs e)
    {
        try
        {
            var timestamp = new DateTimeOffset(e.NowLocal);
            var inactivityMinutes = e.Inactivity.TotalMinutes;

            // Determine if currently active (inactive for less than threshold)
            bool isActive = inactivityMinutes < _inactivityThreshold.TotalMinutes;

            // Calculate sleep minutes increment
            int sleepMinutesIncrement = 0;
            
            if (_lastCheckTime.HasValue && !isActive && !_wasActiveLastCheck)
            {
                // Continuous inactivity - calculate sleep increment
                var intervalStart = _lastCheckTime.Value;
                var intervalEnd = timestamp;

                // Check if this interval intersects with nightly window
                var intervals = new[] { (intervalStart, intervalEnd, false) };
                sleepMinutesIncrement = SleepCalculator.ComputeSleepMinutes(
                    intervals,
                    _inactivityThreshold,
                    _nightlyWindow);
            }

            // Create data point
            var dataPoint = new ActivityDataPoint
            {
                Timestamp = timestamp,
                IsActive = isActive,
                InactivityMinutes = inactivityMinutes,
                SleepMinutesIncrement = sleepMinutesIncrement
            };

            // Write to CSV
            await _csvLogger.AppendDataPointAsync(dataPoint);

            // Update summary
            await UpdateSummaryAsync(dataPoint);

            // Update state for next check
            _lastCheckTime = timestamp;
            _wasActiveLastCheck = isActive;
            _currentInactivityMinutes = inactivityMinutes;
        }
        catch (Exception ex)
        {
            // Log error but don't crash the monitoring
            Console.Error.WriteLine($"Error processing inactivity check: {ex.Message}");
        }
    }

    private async Task UpdateSummaryAsync(ActivityDataPoint dataPoint)
    {
        var date = DateOnly.FromDateTime(dataPoint.Timestamp.LocalDateTime.Date);
        
        // Get existing summary or create new one
        var summary = await _summaryStore.GetSummaryAsync(date);
        if (summary == null)
        {
            summary = new DailySummary
            {
                Date = date,
                TotalSleepMinutes = 0,
                TotalActiveMinutes = 0,
                TotalInactiveMinutes = 0,
                DataPointCount = 0
            };
        }

        // Update summary
        summary.TotalSleepMinutes += dataPoint.SleepMinutesIncrement;
        summary.DataPointCount++;

        // Approximate active/inactive time based on 30-second intervals
        const double intervalMinutes = 0.5; // 30 seconds
        if (dataPoint.IsActive)
        {
            summary.TotalActiveMinutes += intervalMinutes;
        }
        else
        {
            summary.TotalInactiveMinutes += intervalMinutes;
        }

        await _summaryStore.UpdateSummaryAsync(summary);
    }

    public void Dispose()
    {
        _inputMonitor.InactivityCheck -= OnInactivityCheck;
        _inputMonitor.Dispose();
        _systemEventsHandler.Dispose();
    }
}
