namespace TFTSleepTracker.Core.Logic;

/// <summary>
/// Computes sleep minutes from activity intervals
/// </summary>
public static class SleepCalculator
{
    /// <summary>
    /// Computes sleep minutes by building continuous inactivity spans,
    /// intersecting with nightly window, and summing max(0, spanMinutes - threshold)
    /// </summary>
    /// <param name="intervals">Activity intervals with start, end, and activity status</param>
    /// <param name="threshold">Minimum inactivity duration before counting as sleep (typically 60 minutes)</param>
    /// <param name="nightly">Time window for sleep (e.g., [23:00, 08:00))</param>
    /// <returns>Total sleep minutes</returns>
    public static int ComputeSleepMinutes(
        IEnumerable<(DateTimeOffset start, DateTimeOffset end, bool wasActive)> intervals,
        TimeSpan threshold,
        LocalTimeWindow nightly)
    {
        if (intervals == null)
            throw new ArgumentNullException(nameof(intervals));

        // Step 1: Build continuous inactivity spans
        var inactivitySpans = BuildInactivitySpans(intervals);

        // Step 2: Intersect with nightly window and calculate sleep
        int totalSleepMinutes = 0;
        foreach (var span in inactivitySpans)
        {
            var nightlyIntersections = IntersectWithNightlyWindow(span.start, span.end, nightly);
            
            foreach (var intersection in nightlyIntersections)
            {
                var durationMinutes = (int)(intersection.end - intersection.start).TotalMinutes;
                // Add max(0, spanMinutes - threshold)
                int sleepMinutes = Math.Max(0, durationMinutes - (int)threshold.TotalMinutes);
                totalSleepMinutes += sleepMinutes;
            }
        }

        return totalSleepMinutes;
    }

    /// <summary>
    /// Builds continuous inactivity spans from activity intervals
    /// </summary>
    private static List<(DateTimeOffset start, DateTimeOffset end)> BuildInactivitySpans(
        IEnumerable<(DateTimeOffset start, DateTimeOffset end, bool wasActive)> intervals)
    {
        var inactivitySpans = new List<(DateTimeOffset start, DateTimeOffset end)>();
        
        // Sort intervals by start time
        var sortedIntervals = intervals.OrderBy(i => i.start).ToList();
        
        if (sortedIntervals.Count == 0)
            return inactivitySpans;
        
        DateTimeOffset? currentInactivityStart = null;
        DateTimeOffset? currentInactivityEnd = null;
        
        foreach (var interval in sortedIntervals)
        {
            if (interval.wasActive)
            {
                // Active interval - end any ongoing inactivity span
                if (currentInactivityStart.HasValue && currentInactivityEnd.HasValue)
                {
                    inactivitySpans.Add((currentInactivityStart.Value, currentInactivityEnd.Value));
                    currentInactivityStart = null;
                    currentInactivityEnd = null;
                }
            }
            else
            {
                // Inactive interval
                if (currentInactivityStart == null)
                {
                    // Start new inactivity span
                    currentInactivityStart = interval.start;
                    currentInactivityEnd = interval.end;
                }
                else
                {
                    // Continue existing inactivity span
                    currentInactivityEnd = interval.end;
                }
            }
        }
        
        // Handle the last inactivity span if still open
        if (currentInactivityStart.HasValue && currentInactivityEnd.HasValue)
        {
            inactivitySpans.Add((currentInactivityStart.Value, currentInactivityEnd.Value));
        }
        
        return inactivitySpans;
    }

    /// <summary>
    /// Intersects a time span with the nightly window, handling midnight crossing
    /// </summary>
    private static List<(DateTimeOffset start, DateTimeOffset end)> IntersectWithNightlyWindow(
        DateTimeOffset spanStart,
        DateTimeOffset spanEnd,
        LocalTimeWindow nightly)
    {
        var intersections = new List<(DateTimeOffset start, DateTimeOffset end)>();
        
        // Iterate through each day in the span
        var currentDate = spanStart.Date.AddDays(-1); // Start one day before to catch night windows that started the previous day
        var endDate = spanEnd.Date.AddDays(1); // Go one day after to be safe
        
        while (currentDate <= endDate)
        {
            // Determine the nightly window boundaries for this date
            DateTimeOffset windowStart, windowEnd;
            
            if (nightly.CrossesMidnight)
            {
                // Window like 23:00 to 08:00
                // The window starts on currentDate at Start time and ends on next day at End time
                windowStart = new DateTimeOffset(
                    currentDate.Add(nightly.Start.ToTimeSpan()), 
                    spanStart.Offset);
                windowEnd = new DateTimeOffset(
                    currentDate.AddDays(1).Add(nightly.End.ToTimeSpan()), 
                    spanStart.Offset);
            }
            else
            {
                // Window like 09:00 to 17:00 (same day)
                windowStart = new DateTimeOffset(
                    currentDate.Add(nightly.Start.ToTimeSpan()), 
                    spanStart.Offset);
                windowEnd = new DateTimeOffset(
                    currentDate.Add(nightly.End.ToTimeSpan()), 
                    spanStart.Offset);
            }
            
            // Calculate intersection
            var intersectionStart = Max(spanStart, windowStart);
            var intersectionEnd = Min(spanEnd, windowEnd);
            
            if (intersectionStart < intersectionEnd)
            {
                intersections.Add((intersectionStart, intersectionEnd));
            }
            
            currentDate = currentDate.AddDays(1);
        }
        
        return intersections;
    }

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;
    private static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
