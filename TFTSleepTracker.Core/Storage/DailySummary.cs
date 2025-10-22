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
    
    /// <summary>
    /// Total minutes playing Teamfight Tactics
    /// </summary>
    public int TftMinutes { get; set; }

    /// <summary>
    /// Total minutes playing Fortnite
    /// </summary>
    public int FortniteMinutes { get; set; }

    /// <summary>
    /// Total minutes playing Roblox
    /// </summary>
    public int RobloxMinutes { get; set; }

    /// <summary>
    /// Sum of all gaming time (TFT + Fortnite + Roblox)
    /// </summary>
    public int TotalGamingMinutes { get; set; }
}
