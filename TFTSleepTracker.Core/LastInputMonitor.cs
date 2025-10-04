using System.Runtime.InteropServices;

namespace TFTSleepTracker.Core;

/// <summary>
/// Event arguments containing inactivity information
/// </summary>
public class InactivityEventArgs : EventArgs
{
    public DateTime NowLocal { get; set; }
    public TimeSpan Inactivity { get; set; }
}

/// <summary>
/// Monitors user input inactivity using GetLastInputInfo Win32 API
/// </summary>
public class LastInputMonitor : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    private Task? _monitoringTask;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Event fired every 30 seconds with current time and inactivity duration
    /// </summary>
    public event EventHandler<InactivityEventArgs>? InactivityCheck;

    /// <summary>
    /// Gets the time elapsed since the last user input
    /// </summary>
    /// <returns>TimeSpan representing the duration of inactivity</returns>
    public static TimeSpan GetInactivity()
    {
        var lastInputInfo = new LASTINPUTINFO
        {
            cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
        };

        if (!GetLastInputInfo(ref lastInputInfo))
        {
            return TimeSpan.Zero;
        }

        var tickCount = (uint)Environment.TickCount;
        var idleTicks = tickCount - lastInputInfo.dwTime;
        return TimeSpan.FromMilliseconds(idleTicks);
    }

    /// <summary>
    /// Starts monitoring user inactivity. Fires InactivityCheck event every 30 seconds.
    /// </summary>
    public void StartMonitoring()
    {
        if (_monitoringTask != null)
        {
            return; // Already monitoring
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _monitoringTask = MonitorInactivityAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stops monitoring user inactivity
    /// </summary>
    public void StopMonitoring()
    {
        _cancellationTokenSource?.Cancel();
        _monitoringTask?.Wait(TimeSpan.FromSeconds(5));
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _monitoringTask = null;
    }

    private async Task MonitorInactivityAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var inactivity = GetInactivity();
                var eventArgs = new InactivityEventArgs
                {
                    NowLocal = DateTime.Now,
                    Inactivity = inactivity
                };

                InactivityCheck?.Invoke(this, eventArgs);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping monitoring
        }
    }

    public void Dispose()
    {
        StopMonitoring();
        GC.SuppressFinalize(this);
    }
}
