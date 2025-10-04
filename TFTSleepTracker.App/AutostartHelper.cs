using Microsoft.Win32;
using System;

namespace TFTSleepTracker.App
{
    /// <summary>
    /// Helper class for managing Windows autostart via registry
    /// </summary>
    public static class AutostartHelper
    {
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "TFTSleepTracker";

        /// <summary>
        /// Checks if autostart is enabled
        /// </summary>
        public static bool IsAutostartEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
                var value = key?.GetValue(AppName);
                return value != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enables autostart at login
        /// </summary>
        public static void EnableAutostart()
        {
            try
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath))
                    return;

                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                key?.SetValue(AppName, $"\"{exePath}\"");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to enable autostart", ex);
            }
        }

        /// <summary>
        /// Disables autostart at login
        /// </summary>
        public static void DisableAutostart()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                key?.DeleteValue(AppName, false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to disable autostart", ex);
            }
        }

        /// <summary>
        /// Toggles autostart on/off
        /// </summary>
        public static void ToggleAutostart()
        {
            if (IsAutostartEnabled())
            {
                DisableAutostart();
            }
            else
            {
                EnableAutostart();
            }
        }
    }
}
