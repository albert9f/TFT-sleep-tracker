using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TFTSleepTracker.Core.Storage;
using TFTSleepTracker.Core.Logic;

namespace TFTSleepTracker.App
{
    /// <summary>
    /// Scheduler for hourly summary uploads (sends complete data every hour on the hour)
    /// </summary>
    public class DailySummaryScheduler : IDisposable
    {
        private readonly SummaryStore _summaryStore;
        private readonly CsvLogger _csvLogger;
        private readonly LocalTimeWindow _nightlyWindow;
        private readonly TimeSpan _inactivityThreshold;
        private Timer? _hourlyTimer;
        private CancellationTokenSource? _cancellationTokenSource;

        public event EventHandler<SummaryReadyEventArgs>? SummaryReady;

        public DailySummaryScheduler(
            string dataDirectory,
            TimeSpan inactivityThreshold,
            LocalTimeWindow nightlyWindow)
        {
            _summaryStore = new SummaryStore(dataDirectory);
            _csvLogger = new CsvLogger(dataDirectory);
            _inactivityThreshold = inactivityThreshold;
            _nightlyWindow = nightlyWindow;
        }

        /// <summary>
        /// Starts the hourly scheduler
        /// </summary>
        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            ScheduleNextUpload();
        }

        /// <summary>
        /// Stops the hourly scheduler
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _hourlyTimer?.Dispose();
            _hourlyTimer = null;
        }

        /// <summary>
        /// Manually triggers summary computation and upload for yesterday
        /// </summary>
        public async Task SendNowAsync()
        {
            await ProcessCompletedSummariesAsync();
        }

        private void ScheduleNextUpload()
        {
            var now = DateTime.Now;
            
            // Schedule for the start of the next hour
            var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
            var delay = nextHour - now;
            
            _hourlyTimer = new Timer(
                async _ => await OnScheduledTimeAsync(),
                null,
                delay,
                TimeSpan.FromHours(1)); // Repeat every hour
        }

        private async Task OnScheduledTimeAsync()
        {
            await ProcessCompletedSummariesAsync();
        }

        private async Task ProcessCompletedSummariesAsync()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                
                // Process all complete days (exclude today since it's incomplete)
                // Look back up to 7 days to catch any missed summaries
                for (int daysAgo = 1; daysAgo <= 7; daysAgo++)
                {
                    var date = today.AddDays(-daysAgo);
                    
                    // Get all data points for this date
                    var dataPoints = await _csvLogger.GetDataPointsAsync(date);
                    
                    if (dataPoints.Count == 0)
                    {
                        continue; // No data for this day
                    }

                    // Compute sleep minutes from data points
                    var intervals = new List<(DateTimeOffset start, DateTimeOffset end, bool wasActive)>();
                    
                    for (int i = 0; i < dataPoints.Count - 1; i++)
                    {
                        var current = dataPoints[i];
                        var next = dataPoints[i + 1];
                        intervals.Add((current.Timestamp, next.Timestamp, current.IsActive));
                    }

                    // Handle gaps in data (PC powered off) - treat as inactive during sleep hours
                    // Look at previous day's last data point and today's first data point
                    var previousDate = date.AddDays(-1);
                    var previousDayDataPoints = await _csvLogger.GetDataPointsAsync(previousDate);
                    
                    if (previousDayDataPoints.Count > 0 && dataPoints.Count > 0)
                    {
                        var lastPreviousPoint = previousDayDataPoints[previousDayDataPoints.Count - 1];
                        var firstTodayPoint = dataPoints[0];
                        
                        // If there's a gap between last point of previous day and first point of today
                        var gapDuration = firstTodayPoint.Timestamp - lastPreviousPoint.Timestamp;
                        
                        // If the gap is longer than 30 seconds (missed data point), treat as inactive
                        if (gapDuration.TotalSeconds > 30)
                        {
                            // Add this gap as an inactive interval
                            intervals.Insert(0, (lastPreviousPoint.Timestamp, firstTodayPoint.Timestamp, false));
                        }
                    }
                    
                    // Also check for gap from last data point to end of day
                    var nextDate = date.AddDays(1);
                    var nextDayDataPoints = await _csvLogger.GetDataPointsAsync(nextDate);
                    
                    if (dataPoints.Count > 0 && nextDayDataPoints.Count > 0)
                    {
                        var lastTodayPoint = dataPoints[dataPoints.Count - 1];
                        var firstNextPoint = nextDayDataPoints[0];
                        
                        var gapDuration = firstNextPoint.Timestamp - lastTodayPoint.Timestamp;
                        
                        // If the gap is longer than 30 seconds, treat as inactive
                        if (gapDuration.TotalSeconds > 30)
                        {
                            intervals.Add((lastTodayPoint.Timestamp, firstNextPoint.Timestamp, false));
                        }
                    }

                    var sleepMinutes = SleepCalculator.ComputeSleepMinutes(intervals, _inactivityThreshold, _nightlyWindow);

                    // Get or create summary
                    var summary = await _summaryStore.GetSummaryAsync(date);
                    if (summary == null)
                    {
                        summary = new DailySummary
                        {
                            Date = date,
                            TotalSleepMinutes = sleepMinutes,
                            TotalActiveMinutes = 0,
                            TotalInactiveMinutes = 0,
                            DataPointCount = dataPoints.Count
                        };
                    }
                    else
                    {
                        // Update the sleep minutes if they've changed
                        if (summary.TotalSleepMinutes != sleepMinutes)
                        {
                            summary.TotalSleepMinutes = sleepMinutes;
                        }
                    }

                    // Update summary store
                    await _summaryStore.UpdateSummaryAsync(summary);

                    // Raise event for upload
                    SummaryReady?.Invoke(this, new SummaryReadyEventArgs
                    {
                        Summary = summary,
                        Date = date,
                        HasData = true
                    });
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error processing completed summaries: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }

    public class SummaryReadyEventArgs : EventArgs
    {
        public DailySummary? Summary { get; set; }
        public DateOnly Date { get; set; }
        public bool HasData { get; set; } // Indicates if there was data to process
    }
}
