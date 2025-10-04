using System;
using System.Windows;
using System.Threading;
using TFTSleepTracker.Core.Storage;
using TFTSleepTracker.Core.Logic;

namespace TFTSleepTracker.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _instanceMutex;
        private ActivityTracker? _activityTracker;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Single instance check
            bool createdNew;
            _instanceMutex = new Mutex(true, "TFTSleepTracker_SingleInstance", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("TFT Sleep Tracker is already running.", "Already Running", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            // Initialize activity tracker
            var dataDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TFTSleepTracker");
            var inactivityThreshold = TimeSpan.FromMinutes(5);
            var nightlyWindow = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));

            _activityTracker = new ActivityTracker(dataDirectory, inactivityThreshold, nightlyWindow);
            _activityTracker.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _activityTracker?.Stop();
            _activityTracker?.Dispose();
            _instanceMutex?.ReleaseMutex();
            _instanceMutex?.Dispose();
            base.OnExit(e);
        }
    }
}
