using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using TFTSleepTracker.Core.Storage;
using TFTSleepTracker.Core.Net;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Linq;
using System.Collections.Generic;
using MessageBox = System.Windows.MessageBox;

namespace TFTSleepTracker.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon? _notifyIcon;
        private DateTime _insightsStartDate;
        private DateTime _insightsEndDate;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();
            InitializeSettings();
            InitializeInsightsTab();
        }

        private void InitializeSettings()
        {
            // Set autostart checkbox based on current state
            AutostartCheckBox.IsChecked = AutostartHelper.IsAutostartEnabled();
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // TODO: Replace with custom icon
                Visible = true,
                Text = "TFT Sleep Tracker"
            };

            // Create context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open", null, (s, e) => ShowMainWindow());
            contextMenu.Items.Add("Send Now", null, (s, e) => SendNow());
            contextMenu.Items.Add("Check for Updates", null, (s, e) => CheckForUpdates());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Quit Background", null, (s, e) => QuitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private async void SendNow()
        {
            try
            {
                var scheduler = ((App)System.Windows.Application.Current).GetSummaryScheduler();
                if (scheduler != null)
                {
                    await scheduler.SendNowAsync();
                    MessageBox.Show("Yesterday's summary has been computed and queued for upload.", "Send Now", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Scheduler is not initialized.", "Send Now", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing summary: {ex.Message}", "Send Now", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CheckForUpdates()
        {
            try
            {
                var updateService = ((App)System.Windows.Application.Current).GetUpdateService();
                if (updateService == null)
                {
                    MessageBox.Show("Update service is not initialized.", "Check for Updates", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show("Checking for updates...", "Check for Updates", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                var updatesFound = await updateService.CheckAndDownloadUpdatesAsync(forceCheck: true);

                if (updatesFound)
                {
                    var result = MessageBox.Show(
                        "Updates have been downloaded and will be applied on next restart.\n\nRestart now?",
                        "Updates Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        updateService.RestartApp();
                    }
                }
                else
                {
                    MessageBox.Show("Your application is up to date.", "Check for Updates", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for updates: {ex.Message}", "Check for Updates", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuitApplication()
        {
            var result = MessageBox.Show(
                "Are you sure you want to quit TFT Sleep Tracker?\nThis will stop background tracking.",
                "Quit Background",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _notifyIcon?.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Hide window instead of closing
            e.Cancel = true;
            Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            _notifyIcon?.Dispose();
            base.OnClosed(e);
        }

        private void AutostartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                AutostartHelper.EnableAutostart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to enable autostart: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AutostartCheckBox.IsChecked = false;
            }
        }

        private void AutostartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                AutostartHelper.DisableAutostart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to disable autostart: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AutostartCheckBox.IsChecked = true;
            }
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestButton.IsEnabled = false;
                TestStatusText.Text = "Sending test data...";

                // Generate random date in September 2001 (1-30)
                var random = new Random();
                var randomDay = random.Next(1, 31);
                var testDate = new DateOnly(2001, 9, randomDay);

                // Get settings
                var settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TFTSleepTracker");
                var settingsStore = new AppSettingsStore(settingsDir);
                var settings = await settingsStore.LoadAsync();
                
                if (string.IsNullOrWhiteSpace(settings.BotHost))
                {
                    TestStatusText.Text = "❌ Error: Discord bot URL not configured. Check settings.json";
                    TestStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    return;
                }

                // Create test payload (2 hours = 120 minutes)
                var testPayload = new
                {
                    deviceId = settings.DeviceId,
                    date = testDate.ToString("yyyy-MM-dd"),
                    sleepMinutes = 120, // 2 hours
                    computedAt = DateTime.UtcNow.ToString("o")
                };

                // Send the test data
                var uploadService = new UploadService(settings.BotHost, settings.Token);
                var success = await uploadService.UploadAsync(testPayload);

                if (success)
                {
                    TestStatusText.Text = $"✅ Success! Sent 2 hours of sleep for September {randomDay}, 2001";
                    TestStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGreen);
                }
                else
                {
                    var errorDetail = uploadService.LastError ?? "Unknown error";
                    TestStatusText.Text = $"❌ Upload failed: {errorDetail}";
                    TestStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                }
            }
            catch (Exception ex)
            {
                TestStatusText.Text = $"❌ Error: {ex.Message}";
                TestStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            finally
            {
                TestButton.IsEnabled = true;
            }
        }
        
        /// <summary>
        /// Initializes the Insights tab with default date range
        /// </summary>
        private void InitializeInsightsTab()
        {
            // Set default date range to last 7 days
            _insightsEndDate = DateTime.Today;
            _insightsStartDate = DateTime.Today.AddDays(-7);
            
            InsightsStartDate.SelectedDate = _insightsStartDate;
            InsightsEndDate.SelectedDate = _insightsEndDate;
            
            // Initial data load
            RefreshInsights();
        }
        
        /// <summary>
        /// Handles the Refresh button click in Insights tab
        /// </summary>
        private void RefreshInsights_Click(object sender, RoutedEventArgs e)
        {
            _insightsStartDate = InsightsStartDate.SelectedDate ?? DateTime.Today.AddDays(-7);
            _insightsEndDate = InsightsEndDate.SelectedDate ?? DateTime.Today;
            RefreshInsights();
        }
        
        /// <summary>
        /// Refreshes all Insights visualizations
        /// </summary>
        private async void RefreshInsights()
        {
            try
            {
                var startDate = _insightsStartDate.Date;
                var endDate = _insightsEndDate.Date.AddDays(1);
                
                // Get data directory
                var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TFTSleepTracker");
                var summaryStore = new SummaryStore(dataDir);
                
                // Get data from database
                var activityData = await summaryStore.GetActivityDataPointsAsync(startDate, endDate);
                var gameSummary = await summaryStore.GetGameTimeSummaryAsync(startDate, endDate);
                
                // Update all visualizations
                UpdateSummaryCards(gameSummary);
                UpdateGamingTimeChart(activityData, startDate, endDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing insights: {ex.Message}", 
                               "Error", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Updates the gaming time summary cards
        /// </summary>
        private void UpdateSummaryCards(Dictionary<string, int> gameSummary)
        {
            var tftMinutes = gameSummary.ContainsKey("TFT") ? gameSummary["TFT"] : 0;
            var fortniteMinutes = gameSummary.ContainsKey("Fortnite") ? gameSummary["Fortnite"] : 0;
            var robloxMinutes = gameSummary.ContainsKey("Roblox") ? gameSummary["Roblox"] : 0;
            var totalMinutes = tftMinutes + fortniteMinutes + robloxMinutes;
            
            TftTimeText.Text = FormatTimeHoursMinutes(tftMinutes);
            FortniteTimeText.Text = FormatTimeHoursMinutes(fortniteMinutes);
            RobloxTimeText.Text = FormatTimeHoursMinutes(robloxMinutes);
            TotalGamingTimeText.Text = FormatTimeHoursMinutes(totalMinutes);
        }
        
        /// <summary>
        /// Formats minutes into "Xh Ym" format
        /// </summary>
        private string FormatTimeHoursMinutes(int totalMinutes)
        {
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            return $"{hours}h {minutes}m";
        }
        
        /// <summary>
        /// Updates the gaming time chart
        /// </summary>
        private void UpdateGamingTimeChart(List<ActivityDataPoint> data, DateTime start, DateTime end)
        {
            // Group by date and game type
            var dailyData = data
                .Where(d => d.ActivityType == "TFT" || d.ActivityType == "Fortnite" || d.ActivityType == "Roblox")
                .GroupBy(d => d.Timestamp.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    TFT = g.Where(x => x.ActivityType == "TFT").Sum(x => x.DurationMinutes) / 60.0,
                    Fortnite = g.Where(x => x.ActivityType == "Fortnite").Sum(x => x.DurationMinutes) / 60.0,
                    Roblox = g.Where(x => x.ActivityType == "Roblox").Sum(x => x.DurationMinutes) / 60.0
                })
                .ToList();
            
            // Fill in missing dates with zeros
            var allDates = Enumerable.Range(0, (int)(end - start).TotalDays + 1)
                .Select(offset => start.AddDays(offset).Date)
                .ToList();
            
            var completeData = allDates.Select(date =>
            {
                var existing = dailyData.FirstOrDefault(d => d.Date == date);
                return new
                {
                    Date = date,
                    TFT = existing?.TFT ?? 0,
                    Fortnite = existing?.Fortnite ?? 0,
                    Roblox = existing?.Roblox ?? 0
                };
            }).ToList();
            
            // For now, just update the placeholder text with summary data
            var totalHours = completeData.Sum(d => d.TFT + d.Fortnite + d.Roblox);
            ChartPlaceholder.Text = $"Gaming chart visualization\nTotal: {totalHours:F1} hours across {completeData.Count} days";
            
            // TODO: Implement proper chart when LiveCharts issue is resolved
            // Create series for LiveCharts
            // var tftSeries = new LineSeries<double>
            // {
            //     Name = "TFT",
            //     Values = completeData.Select(d => d.TFT).ToArray(),
            //     Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
            //     Fill = null,
            //     GeometrySize = 8
            // };
            // 
            // var fortniteSeries = new LineSeries<double>
            // {
            //     Name = "Fortnite",
            //     Values = completeData.Select(d => d.Fortnite).ToArray(),
            //     Stroke = new SolidColorPaint(SKColors.Teal) { StrokeThickness = 3 },
            //     Fill = null,
            //     GeometrySize = 8
            // };
            // 
            // var robloxSeries = new LineSeries<double>
            // {
            //     Name = "Roblox",
            //     Values = completeData.Select(d => d.Roblox).ToArray(),
            //     Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 },
            //     Fill = null,
            //     GeometrySize = 8
            // };
            // 
            // // Update chart
            // GamingTimeChart.Series = new ISeries[] { tftSeries, fortniteSeries, robloxSeries };
        }
    }
}
