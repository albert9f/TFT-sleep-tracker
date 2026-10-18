using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Handles reading and writing the summary.json file with atomic operations
/// </summary>
public class SummaryStore
{
    private readonly string _dataDirectory;
    private readonly string _summaryFilePath;
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new DateOnlyJsonConverter() }
    };

    public SummaryStore(string dataDirectory)
    {
        _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
        _summaryFilePath = Path.Combine(_dataDirectory, "summary.json");
        
        EnsureDataDirectoryExists();
        
        var dbPath = Path.Combine(_dataDirectory, "sleeptracker.db");
        _connectionString = $"Data Source={dbPath}";
        
        InitializeDatabase();
    }

    /// <summary>
    /// Gets all daily summaries
    /// </summary>
    public async Task<Dictionary<DateOnly, DailySummary>> GetAllSummariesAsync()
    {
        if (!File.Exists(_summaryFilePath))
            return new Dictionary<DateOnly, DailySummary>();

        try
        {
            var json = await File.ReadAllTextAsync(_summaryFilePath);
            var summaries = JsonSerializer.Deserialize<Dictionary<DateOnly, DailySummary>>(json, JsonOptions);
            return summaries ?? new Dictionary<DateOnly, DailySummary>();
        }
        catch
        {
            // If file is corrupted, return empty dictionary
            return new Dictionary<DateOnly, DailySummary>();
        }
    }

    /// <summary>
    /// Gets summary for a specific date
    /// </summary>
    public async Task<DailySummary?> GetSummaryAsync(DateOnly date)
    {
        var summaries = await GetAllSummariesAsync();
        return summaries.TryGetValue(date, out var summary) ? summary : null;
    }

    /// <summary>
    /// Updates or adds a daily summary
    /// </summary>
    public async Task UpdateSummaryAsync(DailySummary summary)
    {
        if (summary == null)
            throw new ArgumentNullException(nameof(summary));

        await FileRetryHelper.RetryAsync(async () =>
        {
            var summaries = await GetAllSummariesAsync();
            summaries[summary.Date] = summary;

            await SaveSummariesAsync(summaries);
        }, "Summary update");
    }

    /// <summary>
    /// Saves all summaries atomically
    /// </summary>
    private async Task SaveSummariesAsync(Dictionary<DateOnly, DailySummary> summaries)
    {
        var tempFilePath = _summaryFilePath + ".tmp";

        try
        {
            var json = JsonSerializer.Serialize(summaries, JsonOptions);
            await File.WriteAllTextAsync(tempFilePath, json);

            // Atomic move
            File.Move(tempFilePath, _summaryFilePath, overwrite: true);
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

    private void EnsureDataDirectoryExists()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }
    
    /// <summary>
    /// Creates database tables if they don't exist
    /// </summary>
    private void InitializeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            // Create ActivityDataPoints table
            var createActivityTable = @"
                CREATE TABLE IF NOT EXISTS ActivityDataPoints (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    ActivityType TEXT NOT NULL,
                    DurationMinutes INTEGER NOT NULL
                )";
            
            using var cmd = connection.CreateCommand();
            cmd.CommandText = createActivityTable;
            cmd.ExecuteNonQuery();
            
            // Create indexes for performance
            var createTimestampIndex = @"
                CREATE INDEX IF NOT EXISTS idx_activity_timestamp 
                ON ActivityDataPoints(Timestamp)";
            
            cmd.CommandText = createTimestampIndex;
            cmd.ExecuteNonQuery();
            
            var createTypeIndex = @"
                CREATE INDEX IF NOT EXISTS idx_activity_type 
                ON ActivityDataPoints(ActivityType)";
            
            cmd.CommandText = createTypeIndex;
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing database: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Logs a single activity data point to the database
    /// </summary>
    public async Task LogActivityDataPointAsync(ActivityDataPoint dataPoint)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            
            var sql = @"
                INSERT INTO ActivityDataPoints (Timestamp, ActivityType, DurationMinutes)
                VALUES (@Timestamp, @ActivityType, @DurationMinutes)";
            
            await connection.ExecuteAsync(sql, new
            {
                Timestamp = dataPoint.Timestamp.ToString("o"), // ISO 8601 format
                dataPoint.ActivityType,
                dataPoint.DurationMinutes
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error logging activity data point: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Retrieves activity data points within a date range
    /// </summary>
    public async Task<List<ActivityDataPoint>> GetActivityDataPointsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            
            var sql = @"
                SELECT Id, Timestamp, ActivityType, DurationMinutes 
                FROM ActivityDataPoints 
                WHERE Timestamp BETWEEN @StartDate AND @EndDate 
                ORDER BY Timestamp";
            
            var results = await connection.QueryAsync<dynamic>(sql, new
            {
                StartDate = startDate.ToString("o"),
                EndDate = endDate.ToString("o")
            });
            
            return results.Select(r => new ActivityDataPoint
            {
                Id = (int)r.Id,
                Timestamp = DateTimeOffset.ParseExact((string)r.Timestamp, "o", System.Globalization.CultureInfo.InvariantCulture),
                ActivityType = (string)r.ActivityType,
                DurationMinutes = (int)r.DurationMinutes
            }).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving activity data points: {ex.Message}");
            return new List<ActivityDataPoint>();
        }
    }
    
    /// <summary>
    /// Gets aggregated gaming time by game type
    /// </summary>
    public async Task<Dictionary<string, int>> GetGameTimeSummaryAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            
            var sql = @"
                SELECT ActivityType, SUM(DurationMinutes) as TotalMinutes 
                FROM ActivityDataPoints 
                WHERE Timestamp BETWEEN @StartDate AND @EndDate 
                AND ActivityType IN ('TFT', 'Fortnite', 'Roblox') 
                GROUP BY ActivityType";
            
            var results = await connection.QueryAsync<(string ActivityType, int TotalMinutes)>(sql, new
            {
                StartDate = startDate.ToString("o"),
                EndDate = endDate.ToString("o")
            });
            
            return results.ToDictionary(r => r.ActivityType, r => r.TotalMinutes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving game time summary: {ex.Message}");
            return new Dictionary<string, int>();
        }
    }
}

/// <summary>
/// JSON converter for DateOnly
/// </summary>
internal class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.ParseExact(reader.GetString()!, Format);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}
