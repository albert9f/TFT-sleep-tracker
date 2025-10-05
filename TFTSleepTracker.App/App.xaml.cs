using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TFTSleepTracker.Core.Storage;
using TFTSleepTracker.Core.Logic;
using TFTSleepTracker.Core.Net;
using TFTSleepTracker.Core.Update;

namespace TFTSleepTracker.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _instanceMutex;
        private ActivityTracker? _activityTracker;
        private DailySummaryScheduler? _summaryScheduler;
        private UploadQueueProcessor? _uploadProcessor;
        private AppSettingsStore? _settingsStore;
        private UploadQueue? _uploadQueue;
        private UpdateService? _updateService;

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

            // Enable autostart on first run if not already enabled
            try
            {
                if (!AutostartHelper.IsAutostartEnabled())
                {
                    AutostartHelper.EnableAutostart();
                }
            }
            catch
            {
                // Silently fail if autostart cannot be enabled
            }

            // Create main window but keep it hidden
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            // Don't show the window - it will be shown when user clicks tray icon

            // Initialize activity tracker
            var dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TFTSleepTracker");
            var inactivityThreshold = TimeSpan.FromMinutes(5);
            var nightlyWindow = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));

            _activityTracker = new ActivityTracker(dataDirectory, inactivityThreshold, nightlyWindow);
            _activityTracker.Start();

            // Initialize daily summary scheduler
            _summaryScheduler = new DailySummaryScheduler(dataDirectory, inactivityThreshold, nightlyWindow);
            _summaryScheduler.SummaryReady += OnSummaryReady;
            _summaryScheduler.Start();

            // Initialize upload queue and processor
            InitializeUploadSystemAsync().ConfigureAwait(false);

            // Check for updates in background (if due)
            CheckForUpdatesInBackgroundAsync().ConfigureAwait(false);
        }

        private async Task CheckForUpdatesInBackgroundAsync()
        {
            try
            {
                // Use ProgramData for shared settings
                var programDataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "TFTSleepTracker");
                
                var settingsStore = new AppSettingsStore(programDataDirectory);
                _updateService = new UpdateService(settingsStore);

                // Check for updates if due (non-blocking)
                await _updateService.CheckAndDownloadUpdatesAsync(forceCheck: false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking for updates: {ex.Message}");
            }
        }

        private async Task InitializeUploadSystemAsync()
        {
            try
            {
                // Use ProgramData for shared settings and queue
                var programDataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "TFTSleepTracker");
                
                var queueDirectory = Path.Combine(programDataDirectory, "queue");

                // Load settings
                _settingsStore = new AppSettingsStore(programDataDirectory);
                var settings = await _settingsStore.LoadAsync();

                // Initialize upload queue
                _uploadQueue = new UploadQueue(queueDirectory);

                // Initialize upload service and processor
                if (!string.IsNullOrEmpty(settings.Token))
                {
                    var uploadService = new UploadService(settings.BotHost, settings.Token);
                    _uploadProcessor = new UploadQueueProcessor(_uploadQueue, uploadService);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing upload system: {ex.Message}");
            }
        }

        private async void OnSummaryReady(object? sender, SummaryReadyEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Summary ready for {e.Date}: {e.Summary?.TotalSleepMinutes} minutes");

                // Enqueue upload if system is initialized
                if (_uploadQueue != null && _settingsStore != null)
                {
                    var settings = await _settingsStore.LoadAsync();
                    
                    // Check if we should send the "no sleep" message
                    string? message = null;
                    if (e.HasData && e.Summary?.TotalSleepMinutes == 0)
                    {
                        // Zero sleep detected with data present
                        // Check if we should send the message (once per week)
                        bool shouldSendMessage = false;
                        
                        if (string.IsNullOrEmpty(settings.LastNoSleepMessageDate))
                        {
                            // Never sent before
                            shouldSendMessage = true;
                        }
                        else
                        {
                            // Check if at least 7 days have passed
                            if (DateOnly.TryParse(settings.LastNoSleepMessageDate, out var lastMessageDate))
                            {
                                var daysSinceLastMessage = e.Date.DayNumber - lastMessageDate.DayNumber;
                                if (daysSinceLastMessage >= 7)
                                {
                                    shouldSendMessage = true;
                                }
                            }
                            else
                            {
                                // Invalid date, reset
                                shouldSendMessage = true;
                            }
                        }
                        
                        if (shouldSendMessage)
                        {
                            message = "looks like ethan didnt sleep tonight";
                            // Update the last message date
                            settings.LastNoSleepMessageDate = e.Date.ToString("yyyy-MM-dd");
                            await _settingsStore.SaveAsync(settings);
                        }
                    }
                    
                    var upload = new QueuedUpload
                    {
                        DeviceId = settings.DeviceId,
                        Date = e.Date.ToString("yyyy-MM-dd"),
                        SleepMinutes = e.Summary?.TotalSleepMinutes ?? 0,
                        ComputedAt = DateTimeOffset.Now,
                        Message = message
                    };

                    await _uploadQueue.EnqueueAsync(upload);
                    System.Diagnostics.Debug.WriteLine($"Enqueued upload for {e.Date}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enqueuing upload: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _activityTracker?.Stop();
            _activityTracker?.Dispose();
            _summaryScheduler?.Stop();
            _summaryScheduler?.Dispose();
            _uploadProcessor?.Dispose();
            _updateService?.Dispose();
            _instanceMutex?.ReleaseMutex();
            _instanceMutex?.Dispose();
            base.OnExit(e);
        }

        public DailySummaryScheduler? GetSummaryScheduler()
        {
            return _summaryScheduler;
        }

        public UpdateService? GetUpdateService()
        {
            return _updateService;
        }
    }
}
