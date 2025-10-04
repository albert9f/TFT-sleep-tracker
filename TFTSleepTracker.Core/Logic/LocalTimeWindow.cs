namespace TFTSleepTracker.Core.Logic;

/// <summary>
/// Represents a time window that recurs daily (e.g., 23:00 to 08:00)
/// </summary>
public readonly struct LocalTimeWindow
{
    /// <summary>
    /// Start time of the window (inclusive)
    /// </summary>
    public TimeOnly Start { get; }

    /// <summary>
    /// End time of the window (exclusive)
    /// </summary>
    public TimeOnly End { get; }

    /// <summary>
    /// Indicates whether the window crosses midnight
    /// </summary>
    public bool CrossesMidnight => End <= Start;

    public LocalTimeWindow(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Checks if a given time falls within this window
    /// </summary>
    public bool Contains(TimeOnly time)
    {
        if (CrossesMidnight)
        {
            // Window like 23:00 to 08:00 (crosses midnight)
            return time >= Start || time < End;
        }
        else
        {
            // Window like 09:00 to 17:00 (same day)
            return time >= Start && time < End;
        }
    }
}
