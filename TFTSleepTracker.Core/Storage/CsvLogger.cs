using System.Globalization;
using System.Text;

namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Handles writing activity data to daily CSV files with atomic operations
/// </summary>
public class CsvLogger
{
    private readonly string _dataDirectory;

    public CsvLogger(string dataDirectory)
    {
        _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
        EnsureDataDirectoryExists();
    }

    /// <summary>
    /// Appends an activity data point to the daily CSV file
    /// </summary>
    public async Task AppendDataPointAsync(ActivityDataPoint dataPoint)
    {
        if (dataPoint == null)
            throw new ArgumentNullException(nameof(dataPoint));

        await FileRetryHelper.RetryAsync(async () =>
        {
            var date = DateOnly.FromDateTime(dataPoint.Timestamp.LocalDateTime.Date);
            var csvFilePath = GetCsvFilePath(date);
            
            // Check if file needs header
            bool needsHeader = !File.Exists(csvFilePath);

            // Build CSV line
            var sb = new StringBuilder();
            if (needsHeader)
            {
                sb.AppendLine("timestamp,is_active,inactivity_minutes,sleep_minutes_increment");
            }

            // Format: ISO 8601 timestamp, boolean, double, integer
            sb.Append(dataPoint.Timestamp.ToString("o")); // ISO 8601 format
            sb.Append(',');
            sb.Append(dataPoint.IsActive ? "true" : "false");
            sb.Append(',');
            sb.Append(dataPoint.InactivityMinutes.ToString("F2", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(dataPoint.SleepMinutesIncrement);
            sb.AppendLine();

            // Atomic write using temp file
            await WriteAtomicallyAsync(csvFilePath, sb.ToString(), append: true);
        }, "CSV append");
    }

    /// <summary>
    /// Gets all data points for a specific date
    /// </summary>
    public async Task<List<ActivityDataPoint>> GetDataPointsAsync(DateOnly date)
    {
        var csvFilePath = GetCsvFilePath(date);
        
        if (!File.Exists(csvFilePath))
            return new List<ActivityDataPoint>();

        var dataPoints = new List<ActivityDataPoint>();
        var lines = await File.ReadAllLinesAsync(csvFilePath);

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            var parts = line.Split(',');
            if (parts.Length != 4)
                continue;

            try
            {
                dataPoints.Add(new ActivityDataPoint
                {
                    Timestamp = DateTimeOffset.Parse(parts[0], CultureInfo.InvariantCulture),
                    IsActive = bool.Parse(parts[1]),
                    InactivityMinutes = double.Parse(parts[2], CultureInfo.InvariantCulture),
                    SleepMinutesIncrement = int.Parse(parts[3], CultureInfo.InvariantCulture)
                });
            }
            catch
            {
                // Skip malformed lines
            }
        }

        return dataPoints;
    }

    private string GetCsvFilePath(DateOnly date)
    {
        var fileName = $"{date:yyyy-MM-dd}.csv";
        return Path.Combine(_dataDirectory, fileName);
    }

    private void EnsureDataDirectoryExists()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    private async Task WriteAtomicallyAsync(string filePath, string content, bool append)
    {
        var tempFilePath = filePath + ".tmp";

        try
        {
            if (append && File.Exists(filePath))
            {
                // Read existing content and append
                var existingContent = await File.ReadAllTextAsync(filePath);
                await File.WriteAllTextAsync(tempFilePath, existingContent + content);
            }
            else
            {
                // Write new file
                await File.WriteAllTextAsync(tempFilePath, content);
            }

            // Atomic move (overwrites if exists)
            File.Move(tempFilePath, filePath, overwrite: true);
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
}
