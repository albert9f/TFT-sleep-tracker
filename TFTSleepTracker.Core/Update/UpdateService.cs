using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Squirrel;
using TFTSleepTracker.Core.Storage;

namespace TFTSleepTracker.Core.Update;

/// <summary>
/// Service for managing application auto-updates via Clowd.Squirrel
/// </summary>
public class UpdateService : IDisposable
{
    private readonly AppSettingsStore _settingsStore;
    private readonly string _githubRepoUrl;
    private UpdateManager? _updateManager;
    private const int UpdateCheckIntervalDays = 2;

    public UpdateService(AppSettingsStore settingsStore, string githubRepoUrl = "https://github.com/albert9f/TFT-sleep-tracker")
    {
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        _githubRepoUrl = githubRepoUrl;
    }

    /// <summary>
    /// Checks if an update check is due (7 days since last check)
    /// </summary>
    public async Task<bool> IsUpdateCheckDueAsync()
    {
        try
        {
            var settings = await _settingsStore.LoadAsync();
            
            if (settings.LastUpdateCheck == null)
            {
                return true; // Never checked
            }

            var daysSinceLastCheck = (DateTimeOffset.Now - settings.LastUpdateCheck.Value).TotalDays;
            return daysSinceLastCheck >= UpdateCheckIntervalDays;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks for updates and downloads them if available (silent, no restart)
    /// </summary>
    /// <param name="forceCheck">If true, bypasses the 7-day interval</param>
    /// <returns>True if updates were found and downloaded, false otherwise</returns>
    public async Task<bool> CheckAndDownloadUpdatesAsync(bool forceCheck = false)
    {
        try
        {
            // Check if update check is due
            if (!forceCheck && !await IsUpdateCheckDueAsync())
            {
                LogToEventLog("Update check skipped - not yet due", EventLogEntryType.Information);
                return false;
            }

            // Update last check timestamp
            var settings = await _settingsStore.LoadAsync();
            settings.LastUpdateCheck = DateTimeOffset.Now;
            await _settingsStore.SaveAsync(settings);

            // Initialize update manager with GitHub source
            using var mgr = new GithubUpdateManager(_githubRepoUrl);
            
            // Check for updates
            var updateInfo = await mgr.CheckForUpdate();

            if (updateInfo == null || !updateInfo.ReleasesToApply.Any())
            {
                LogToEventLog("No updates available", EventLogEntryType.Information);
                return false;
            }

            LogToEventLog($"Found {updateInfo.ReleasesToApply.Count} update(s) to apply", EventLogEntryType.Information);

            // Download and apply updates (but don't restart)
            await mgr.UpdateApp();

            LogToEventLog("Updates downloaded and ready to apply on next restart", EventLogEntryType.Information);
            return true;
        }
        catch (Exception ex)
        {
            LogToEventLog($"Update check failed: {ex.Message}", EventLogEntryType.Warning);
            return false;
        }
    }

    /// <summary>
    /// Applies updates and restarts the application
    /// </summary>
    public void RestartApp()
    {
        try
        {
            LogToEventLog("Restarting application to apply updates", EventLogEntryType.Information);
            UpdateManager.RestartApp();
        }
        catch (Exception ex)
        {
            LogToEventLog($"Failed to restart app: {ex.Message}", EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// Gets the current application version
    /// </summary>
    public string GetCurrentVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
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
        _updateManager?.Dispose();
    }
}