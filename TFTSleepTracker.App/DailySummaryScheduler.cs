using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TFTSleepTracker.Core.Storage;
using TFTSleepTracker.Core.Logic;

namespace TFTSleepTracker.App
{
    /// <summary>
    /// Scheduler for daily summary uploads at 08:05 AM
    /// </summary>
    public class DailySummaryScheduler : IDisposable
    {
        private readonly SummaryStore _summaryStore;
        private readonly CsvLogger _csvLogger;
        private readonly LocalTimeWindow _nightlyWindow;
        private readonly TimeSpan _inactivityThreshold;
        private Timer? _dailyTimer;
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
        /// Starts the daily scheduler
        /// </summary>
        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            ScheduleNextUpload();
        }

        /// <summary>
        /// Stops the daily scheduler
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _dailyTimer?.Dispose();
            _dailyTimer = null;
        }

        /// <summary>
        /// Manually triggers summary computation and upload for yesterday
        /// </summary>
        public async Task SendNowAsync()
        {
            await ProcessYesterdaySummaryAsync();
        }

        private void ScheduleNextUpload()
        {
            var now = DateTime.Now;
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 8, 5, 0);

            // If we've already passed 08:05 today, schedule for tomorrow
            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var delay = scheduledTime - now;
            _dailyTimer = new Timer(
                async _ => await OnScheduledTimeAsync(),
                null,
                delay,
                TimeSpan.FromDays(1));
        }

        private async Task OnScheduledTimeAsync()
        {
            await ProcessYesterdaySummaryAsync();
        }

        private async Task ProcessYesterdaySummaryAsync()
        {
            try
            {
                var yesterday = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
                
                // Get all data points for yesterday
                var dataPoints = await _csvLogger.GetDataPointsAsync(yesterday);
                
                if (dataPoints.Count == 0)
                {
                    return; // No data to process
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
                var summary = await _summaryStore.GetSummaryAsync(yesterday);
                if (summary == null)
                {
                    summary = new DailySummary
                    {
                        Date = yesterday,
                        TotalSleepMinutes = sleepMinutes,
                        TotalActiveMinutes = 0,
                        TotalInactiveMinutes = 0,
                        DataPointCount = dataPoints.Count
                    };
                }
                else
                {
                    summary.TotalSleepMinutes = sleepMinutes;
                }

                // Update summary store
                await _summaryStore.UpdateSummaryAsync(summary);

                // Raise event for upload
                SummaryReady?.Invoke(this, new SummaryReadyEventArgs
                {
                    Summary = summary,
                    Date = yesterday,
                    HasData = true
                });
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error processing yesterday's summary: {ex.Message}");
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
