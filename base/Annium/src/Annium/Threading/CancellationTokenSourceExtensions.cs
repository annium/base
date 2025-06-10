using System.Threading;
using NodaTime;

namespace Annium.Threading;

/// <summary>
/// Provides extension methods for working with cancellation token sources.
/// </summary>
public static class CancellationTokenSourceExtensions
{
    /// <summary>
    /// Schedules cancellation of the token source after the specified duration.
    /// </summary>
    /// <param name="cts">The cancellation token source to cancel.</param>
    /// <param name="scheduler">The scheduler to use for scheduling the cancellation.</param>
    /// <param name="duration">The duration after which to cancel the token source.</param>
    public static void CancelAfter(this CancellationTokenSource cts, IActionScheduler scheduler, Duration duration)
    {
        scheduler.Delay(cts.Cancel, duration);
    }
}
