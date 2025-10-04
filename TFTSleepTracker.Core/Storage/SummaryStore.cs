using System.Text.Json;
using System.Text.Json.Serialization;

namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Handles reading and writing the summary.json file with atomic operations
/// </summary>
public class SummaryStore
{
    private readonly string _dataDirectory;
    private readonly string _summaryFilePath;
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
