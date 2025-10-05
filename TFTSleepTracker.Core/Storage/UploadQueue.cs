using System.Text.Json;

namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Represents a queued upload payload
/// </summary>
public class QueuedUpload
{
    public string DeviceId { get; set; } = "";
    public string Date { get; set; } = ""; // ISO 8601 date (yyyy-MM-dd)
    public int SleepMinutes { get; set; }
    public DateTimeOffset ComputedAt { get; set; }
    public string? Message { get; set; } // Optional special message
}

/// <summary>
/// Manages the upload queue directory and operations
/// </summary>
public class UploadQueue
{
    private readonly string _queueDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public UploadQueue(string queueDirectory)
    {
        _queueDirectory = queueDirectory ?? throw new ArgumentNullException(nameof(queueDirectory));
        EnsureDirectoryExists();
    }

    /// <summary>
    /// Enqueues a new upload with atomic write
    /// </summary>
    public async Task EnqueueAsync(QueuedUpload upload)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var fileName = $"{timestamp}_{upload.Date}.json";
        var filePath = Path.Combine(_queueDirectory, fileName);
        var tempFilePath = filePath + ".tmp";

        try
        {
            var json = JsonSerializer.Serialize(upload, JsonOptions);
            await File.WriteAllTextAsync(tempFilePath, json);

            // Atomic move
            File.Move(tempFilePath, filePath, overwrite: false);
        }
        catch
        {
            // Clean up temp file if something went wrong
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            throw;
        }
    }

    /// <summary>
    /// Gets the oldest queued upload file
    /// </summary>
    public string? GetOldestFile()
    {
        try
        {
            var files = Directory.GetFiles(_queueDirectory, "*.json")
                .OrderBy(f => f)
                .FirstOrDefault();

            return files;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Reads the upload from a queue file
    /// </summary>
    public async Task<QueuedUpload?> ReadFileAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<QueuedUpload>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Deletes a successfully uploaded file
    /// </summary>
    public void DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Ignore errors - file might already be deleted
        }
    }

    /// <summary>
    /// Purges queue files older than specified days
    /// </summary>
    public void PurgeOldFiles(int daysToKeep = 7)
    {
        try
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysToKeep);
            var files = Directory.GetFiles(_queueDirectory, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTimeUtc < cutoffDate.UtcDateTime)
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // Ignore errors for individual files
                }
            }
        }
        catch
        {
            // Ignore errors - directory might not exist yet
        }
    }

    /// <summary>
    /// Gets the count of pending uploads
    /// </summary>
    public int GetPendingCount()
    {
        try
        {
            return Directory.GetFiles(_queueDirectory, "*.json").Length;
        }
        catch
        {
            return 0;
        }
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_queueDirectory))
        {
            Directory.CreateDirectory(_queueDirectory);
        }
    }
}
