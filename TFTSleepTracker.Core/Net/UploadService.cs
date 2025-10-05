using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace TFTSleepTracker.Core.Net;

/// <summary>
/// Service for uploading sleep summaries to the Discord bot
/// </summary>
public class UploadService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _botHost;
    private readonly string _token;
    private readonly Random _random = new();
    private const int MaxRetryDelaySeconds = 60;
    private const int InitialRetryDelaySeconds = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public UploadService(string botHost, string token)
    {
        _botHost = botHost ?? throw new ArgumentNullException(nameof(botHost));
        _token = token ?? throw new ArgumentNullException(nameof(token));
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Uploads a sleep summary payload to the bot endpoint
    /// </summary>
    /// <param name="payload">The upload payload</param>
    /// <returns>True if upload succeeded (2xx response), false otherwise</returns>
    public async Task<bool> UploadAsync(object payload)
    {
        try
        {
            var url = $"{_botHost}/ingest-sleep?token={Uri.EscapeDataString(_token)}";
            var response = await _httpClient.PostAsJsonAsync(url, payload, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                LogToEventLog($"Successfully uploaded sleep data", EventLogEntryType.Information);
                return true;
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                LogToEventLog($"Upload failed with status {response.StatusCode}: {body}", EventLogEntryType.Warning);
                LastError = $"HTTP {(int)response.StatusCode}: {body}";
                return false;
            }
        }
        catch (Exception ex)
        {
            LogToEventLog($"Upload error: {ex.Message}", EventLogEntryType.Error);
            LastError = ex.Message;
            return false;
        }
    }

    public string? LastError { get; private set; }

    /// <summary>
    /// Calculates retry delay with exponential backoff and jitter
    /// </summary>
    /// <param name="attemptNumber">Zero-based attempt number</param>
    /// <returns>Delay in seconds</returns>
    public int CalculateRetryDelay(int attemptNumber)
    {
        // Exponential backoff: 2^attempt seconds
        var exponentialDelay = Math.Pow(2, attemptNumber) * InitialRetryDelaySeconds;
        
        // Cap at max delay
        var cappedDelay = Math.Min(exponentialDelay, MaxRetryDelaySeconds);
        
        // Add jitter: random value between 0 and 25% of the delay
        var jitter = _random.NextDouble() * cappedDelay * 0.25;
        
        return (int)(cappedDelay + jitter);
    }

    private void LogToEventLog(string message, EventLogEntryType type)
    {
        try
        {
            const string source = "TFTSleepTracker";
            const string logName = "Application";

            // Create event source if it doesn't exist (requires admin privileges)
            if (!EventLog.SourceExists(source))
            {
                try
                {
                    EventLog.CreateEventSource(source, logName);
                }
                catch
                {
                    // Silently fail if we can't create the source (not admin)
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
        _httpClient?.Dispose();
    }
}
