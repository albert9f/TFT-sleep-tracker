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
    private readonly TimeSpan _inactivityThreshold;
    private readonly LocalTimeWindow _nightlyWindow;

    private DateTimeOffset? _lastCheckTime;
    private bool _wasActiveLastCheck;
    private double _currentInactivityMinutes;

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
    }

    /// <summary>
    /// Starts tracking activity
    /// </summary>
    public void Start()
    {
        _inputMonitor.StartMonitoring();
    }

    /// <summary>
    /// Stops tracking activity
    /// </summary>
    public void Stop()
    {
        _inputMonitor.StopMonitoring();
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
    }
}
