using System.Threading;

namespace Annium.Execution.Background.Tests;

/// <summary>
/// Helper class providing work simulation methods for testing background executors
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Simulates long-running synchronous work
    /// </summary>
    public static void SyncLongWork() => SyncWork(400);

    /// <summary>
    /// Performs synchronous work by spinning for a specified duration
    /// </summary>
    /// <param name="delay">The delay duration in milliseconds</param>
    private static void SyncWork(int delay)
    {
        SpinWait.SpinUntil(() => false, delay);
    }
}
