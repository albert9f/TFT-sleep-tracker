namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Represents daily summary data
/// </summary>
public class DailySummary
{
    /// <summary>
    /// Date for this summary
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Total sleep minutes for the day
    /// </summary>
    public int TotalSleepMinutes { get; set; }

    /// <summary>
    /// Total active time in minutes
    /// </summary>
    public double TotalActiveMinutes { get; set; }

    /// <summary>
    /// Total inactive time in minutes
    /// </summary>
    public double TotalInactiveMinutes { get; set; }

    /// <summary>
    /// Number of activity data points recorded
    /// </summary>
    public int DataPointCount { get; set; }
}
