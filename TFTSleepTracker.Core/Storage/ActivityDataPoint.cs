namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Represents a single activity data point for CSV logging
/// </summary>
public class ActivityDataPoint
{
    /// <summary>
    /// Timestamp when the data point was recorded
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Whether the user was active at this time
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of minutes of inactivity
    /// </summary>
    public double InactivityMinutes { get; set; }

    /// <summary>
    /// Sleep minutes increment for this data point
    /// </summary>
    public int SleepMinutesIncrement { get; set; }
}
