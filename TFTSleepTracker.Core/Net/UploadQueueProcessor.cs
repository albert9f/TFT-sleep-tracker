using System.Diagnostics;
using TFTSleepTracker.Core.Storage;

namespace TFTSleepTracker.Core.Net;

/// <summary>
/// Background service that processes the upload queue
/// </summary>
public class UploadQueueProcessor : IDisposable
{
    private readonly UploadQueue _queue;
    private readonly UploadService _uploadService;
    private readonly Timer _processTimer;
    private readonly Timer _purgeTimer;
    private bool _isProcessing = false;
    private int _currentAttempt = 0;
    private string? _currentFile = null;

    public UploadQueueProcessor(UploadQueue queue, UploadService uploadService)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));

        // Start processing immediately and then every 5 seconds
        _processTimer = new Timer(
            async _ => await ProcessNextUploadAsync(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(5));

        // Purge old files once per day
        _purgeTimer = new Timer(
            _ => _queue.PurgeOldFiles(7),
            null,
            TimeSpan.Zero,
            TimeSpan.FromHours(24));
    }

    private async Task ProcessNextUploadAsync()
    {
        // Avoid concurrent processing
        if (_isProcessing)
        {
            return;
        }

        _isProcessing = true;

        try
        {
            // Get the oldest file
            var file = _queue.GetOldestFile();
            if (file == null)
            {
                // No files to process, reset attempt counter
                _currentAttempt = 0;
                _currentFile = null;
                return;
            }

            // Check if this is a new file or a retry
            if (_currentFile != file)
            {
                _currentAttempt = 0;
                _currentFile = file;
            }

            // Read the upload payload
            var upload = await _queue.ReadFileAsync(file);
            if (upload == null)
            {
                // File is corrupted, delete it
                LogToEventLog($"Deleting corrupted queue file: {Path.GetFileName(file)}", EventLogEntryType.Warning);
                _queue.DeleteFile(file);
                _currentAttempt = 0;
                _currentFile = null;
                return;
            }

            // Attempt upload
            var success = await _uploadService.UploadAsync(upload);

            if (success)
            {
                // Upload succeeded, delete the file
                _queue.DeleteFile(file);
                _currentAttempt = 0;
                _currentFile = null;
            }
            else
            {
                // Upload failed, calculate backoff delay
                _currentAttempt++;
                var delaySeconds = _uploadService.CalculateRetryDelay(_currentAttempt - 1);
                
                LogToEventLog(
                    $"Upload failed for {Path.GetFileName(file)}, attempt {_currentAttempt}. " +
                    $"Will retry in {delaySeconds} seconds.",
                    EventLogEntryType.Warning);

                // Adjust timer for next retry
                _processTimer.Change(
                    TimeSpan.FromSeconds(delaySeconds),
                    TimeSpan.FromSeconds(5));
            }
        }
        catch (Exception ex)
        {
            LogToEventLog($"Error processing upload queue: {ex.Message}", EventLogEntryType.Error);
        }
        finally
        {
            _isProcessing = false;
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
        _processTimer?.Dispose();
        _purgeTimer?.Dispose();
    }
}
