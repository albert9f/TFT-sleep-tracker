using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace TFTSleepTracker.Core.Logic;

/// <summary>
/// Handles system events like power mode changes, time changes, and session switches
/// </summary>
public class SystemEventsHandler : IDisposable
{
    private bool _isDisposed;

    public event EventHandler? SystemResumed;
    public event EventHandler? SystemSuspended;
    public event EventHandler? TimeChanged;
    public event EventHandler? SessionSwitch;

    public SystemEventsHandler()
    {
        try
        {
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.TimeChanged += OnTimeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;
        }
        catch (Exception ex)
        {
            LogToEventLog($"Failed to register system events: {ex.Message}", EventLogEntryType.Warning);
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        try
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    LogToEventLog("System resumed from suspend/hibernate", EventLogEntryType.Information);
                    SystemResumed?.Invoke(this, EventArgs.Empty);
                    break;

                case PowerModes.Suspend:
                    LogToEventLog("System entering suspend/hibernate", EventLogEntryType.Information);
                    SystemSuspended?.Invoke(this, EventArgs.Empty);
                    break;

                case PowerModes.StatusChange:
                    // Battery/AC power status changed - we can ignore this
                    break;
            }
        }
        catch (Exception ex)
        {
            LogToEventLog($"Error handling power mode change: {ex.Message}", EventLogEntryType.Error);
        }
    }

    private void OnTimeChanged(object sender, EventArgs e)
    {
        try
        {
            LogToEventLog("System time changed (DST or manual adjustment)", EventLogEntryType.Information);
            TimeChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            LogToEventLog($"Error handling time change: {ex.Message}", EventLogEntryType.Error);
        }
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        try
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.RemoteConnect:
                    LogToEventLog($"Session switch: {e.Reason}", EventLogEntryType.Information);
                    SessionSwitch?.Invoke(this, EventArgs.Empty);
                    break;

                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.RemoteDisconnect:
                    LogToEventLog($"Session switch: {e.Reason}", EventLogEntryType.Information);
                    break;
            }
        }
        catch (Exception ex)
        {
            LogToEventLog($"Error handling session switch: {ex.Message}", EventLogEntryType.Error);
        }
    }

    private void LogToEventLog(string message, EventLogEntryType type)
    {
        try
        {
            const string source = "TFTSleepTracker";
            const string logName = "Application";

            if (!EventLog.SourceExists(source))
            {
                try
                {
                    EventLog.CreateEventSource(source, logName);
                }
                catch
                {
                    return;
                }
            }

            EventLog.WriteEntry(source, message, type);
        }
        catch
        {
            // Silently fail - logging should not crash the app
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        try
        {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            SystemEvents.TimeChanged -= OnTimeChanged;
            SystemEvents.SessionSwitch -= OnSessionSwitch;
        }
        catch
        {
            // Ignore errors during cleanup
        }

        _isDisposed = true;
    }
}
