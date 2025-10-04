using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using MessageBox = System.Windows.MessageBox;

namespace TFTSleepTracker.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon? _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();
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

        private void SendNow()
        {
            // TODO: Implement send now functionality
            MessageBox.Show("Send Now functionality will be implemented.", "Send Now", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CheckForUpdates()
        {
            // TODO: Implement update checking
            MessageBox.Show("Check for Updates functionality will be implemented.", "Check for Updates", 
                MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
