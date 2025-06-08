using System.Threading;
using System.Threading.Tasks;

namespace Annium.Execution.Background.Tests;

/// <summary>
/// Helper class providing work simulation methods for testing background executors
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Simulates long-running asynchronous work
    /// </summary>
    /// <returns>A task representing the asynchronous work</returns>
    public static Task AsyncLongWorkAsync() => AsyncWorkAsync(200);

    /// <summary>
    /// Simulates fast-running asynchronous work
    /// </summary>
    /// <returns>A task representing the asynchronous work</returns>
    public static Task AsyncFastWorkAsync() => AsyncWorkAsync(20);

    /// <summary>
    /// Simulates long-running synchronous work
    /// </summary>
    public static void SyncLongWork() => SyncWork(400);

    /// <summary>
    /// Simulates fast-running synchronous work
    /// </summary>
    public static void SyncFastWork() => SyncWork(40);

    /// <summary>
    /// Performs asynchronous work by delaying for a specified number of iterations
    /// </summary>
    /// <param name="iterations">The number of delay iterations to perform</param>
    /// <returns>A task representing the asynchronous work</returns>
    private static async Task AsyncWorkAsync(int iterations)
    {
        var i = 0;
        while (i < iterations)
        {
            await Task.Delay(2);
            i++;
        }
    }

    /// <summary>
    /// Performs synchronous work by spinning for a specified duration
    /// </summary>
    /// <param name="delay">The delay duration in milliseconds</param>
    private static void SyncWork(int delay)
    {
        SpinWait.SpinUntil(() => false, delay);
    }
}
