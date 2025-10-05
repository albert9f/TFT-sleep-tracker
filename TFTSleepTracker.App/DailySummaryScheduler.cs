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
                        Date = date
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
