using System.Threading;

namespace Annium.Execution.Background.Tests;

/// <summary>
/// Helper class providing work simulation methods for testing background executors
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Simulates long-running synchronous work by spinning for a fixed duration
    /// </summary>
    public static void SyncLongWork() => SpinWait.SpinUntil(() => false, 400);
}
