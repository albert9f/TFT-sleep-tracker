using System;
using System.Threading;
using System.Threading.Tasks;

namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Helper for retrying file operations with exponential backoff
/// </summary>
public static class FileRetryHelper
{
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 100;

    /// <summary>
    /// Executes an action with retry logic for transient file IO errors
    /// </summary>
    public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, string operationName = "File operation")
    {
        var random = new Random();
        
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (IOException ex) when (attempt < MaxRetries - 1)
            {
                // Exponential backoff with jitter
                var delay = InitialDelayMs * Math.Pow(2, attempt);
                var jitter = random.Next(0, (int)(delay * 0.3));
                var totalDelay = (int)delay + jitter;

                System.Diagnostics.Debug.WriteLine(
                    $"{operationName} failed (attempt {attempt + 1}/{MaxRetries}): {ex.Message}. " +
                    $"Retrying in {totalDelay}ms...");

                await Task.Delay(totalDelay);
            }
        }

        // Last attempt without catching
        return await operation();
    }

    /// <summary>
    /// Executes an action with retry logic for transient file IO errors
    /// </summary>
    public static async Task RetryAsync(Func<Task> operation, string operationName = "File operation")
    {
        var random = new Random();
        
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                await operation();
                return;
            }
            catch (IOException ex) when (attempt < MaxRetries - 1)
            {
                // Exponential backoff with jitter
                var delay = InitialDelayMs * Math.Pow(2, attempt);
                var jitter = random.Next(0, (int)(delay * 0.3));
                var totalDelay = (int)delay + jitter;

                System.Diagnostics.Debug.WriteLine(
                    $"{operationName} failed (attempt {attempt + 1}/{MaxRetries}): {ex.Message}. " +
                    $"Retrying in {totalDelay}ms...");

                await Task.Delay(totalDelay);
            }
        }

        // Last attempt without catching
        await operation();
    }
}
