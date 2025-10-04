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
                // Calculate duration in local time (not real time) to handle DST correctly
                // This ensures we count 9 hours for [23:00, 08:00) regardless of DST transitions
                var startLocal = DateTime.SpecifyKind(intersection.start.DateTime, DateTimeKind.Unspecified);
                var endLocal = DateTime.SpecifyKind(intersection.end.DateTime, DateTimeKind.Unspecified);
                var durationMinutes = (int)(endLocal - startLocal).TotalMinutes;
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
    /// Intersects a time span with the nightly window, handling midnight crossing and DST
    /// </summary>
    private static List<(DateTimeOffset start, DateTimeOffset end)> IntersectWithNightlyWindow(
        DateTimeOffset spanStart,
        DateTimeOffset spanEnd,
        LocalTimeWindow nightly)
    {
        var intersections = new List<(DateTimeOffset start, DateTimeOffset end)>();
        
        // Work with local DateTime for window intersection to handle DST correctly
        // Use DateTime with Unspecified kind to avoid offset conflicts
        var spanStartLocal = DateTime.SpecifyKind(spanStart.DateTime, DateTimeKind.Unspecified);
        var spanEndLocal = DateTime.SpecifyKind(spanEnd.DateTime, DateTimeKind.Unspecified);
        
        // Iterate through each day in the span
        var currentDate = spanStartLocal.Date.AddDays(-1);
        var endDate = spanEndLocal.Date.AddDays(1);
        
        while (currentDate <= endDate)
        {
            // Determine the nightly window boundaries for this date in local time
            DateTime windowStartLocal, windowEndLocal;
            
            if (nightly.CrossesMidnight)
            {
                // Window like 23:00 to 08:00
                windowStartLocal = currentDate.Add(nightly.Start.ToTimeSpan());
                windowEndLocal = currentDate.AddDays(1).Add(nightly.End.ToTimeSpan());
            }
            else
            {
                // Window like 09:00 to 17:00 (same day)
                windowStartLocal = currentDate.Add(nightly.Start.ToTimeSpan());
                windowEndLocal = currentDate.Add(nightly.End.ToTimeSpan());
            }
            
            // Calculate intersection in local time
            var intersectionStartLocal = Max(spanStartLocal, windowStartLocal);
            var intersectionEndLocal = Min(spanEndLocal, windowEndLocal);
            
            if (intersectionStartLocal < intersectionEndLocal)
            {
                // Convert back to DateTimeOffset, preserving the appropriate offsets
                var intersectionStart = ConvertToDateTimeOffset(intersectionStartLocal, spanStart, spanEnd);
                var intersectionEnd = ConvertToDateTimeOffset(intersectionEndLocal, spanStart, spanEnd);
                
                intersections.Add((intersectionStart, intersectionEnd));
            }
            
            currentDate = currentDate.AddDays(1);
        }
        
        return intersections;
    }

    /// <summary>
    /// Converts a local DateTime to DateTimeOffset, determining the appropriate offset
    /// based on where it falls relative to the span
    /// </summary>
    private static DateTimeOffset ConvertToDateTimeOffset(
        DateTime localDateTime,
        DateTimeOffset spanStart,
        DateTimeOffset spanEnd)
    {
        var spanStartLocal = DateTime.SpecifyKind(spanStart.DateTime, DateTimeKind.Unspecified);
        var spanEndLocal = DateTime.SpecifyKind(spanEnd.DateTime, DateTimeKind.Unspecified);
        
        // If the local time matches the span start's local time, use its offset
        if (localDateTime == spanStartLocal)
        {
            return spanStart;
        }
        
        // If the local time matches the span end's local time, use its offset
        if (localDateTime == spanEndLocal)
        {
            return spanEnd;
        }
        
        // If before span start, use span start's offset
        if (localDateTime < spanStartLocal)
        {
            return new DateTimeOffset(localDateTime, spanStart.Offset);
        }
        
        // If after span end, use span end's offset
        if (localDateTime > spanEndLocal)
        {
            return new DateTimeOffset(localDateTime, spanEnd.Offset);
        }
        
        // Within the span - if offsets differ (DST transition), determine which to use
        if (spanStart.Offset != spanEnd.Offset)
        {
            // During a DST transition, use the offset that's appropriate for the time
            // Try both offsets and see which one makes sense
            var withStartOffset = new DateTimeOffset(localDateTime, spanStart.Offset);
            var withEndOffset = new DateTimeOffset(localDateTime, spanEnd.Offset);
            
            // Use the offset that keeps us within the span's UTC range
            if (withStartOffset.UtcDateTime >= spanStart.UtcDateTime && 
                withStartOffset.UtcDateTime <= spanEnd.UtcDateTime)
            {
                return withStartOffset;
            }
            else if (withEndOffset.UtcDateTime >= spanStart.UtcDateTime && 
                     withEndOffset.UtcDateTime <= spanEnd.UtcDateTime)
            {
                return withEndOffset;
            }
        }
        
        // Default: use span start's offset
        return new DateTimeOffset(localDateTime, spanStart.Offset);
    }

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;
    private static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
