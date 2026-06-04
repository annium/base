using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal implementation of action scheduler for real-time scheduling
/// </summary>
internal class ActionScheduler : IActionScheduler, ILogSubject
{
    /// <summary>
    /// Gets the logger instance used for surfacing scheduled-action failures.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionScheduler"/> class.
    /// </summary>
    /// <param name="logger">Logger used to report unhandled exceptions from scheduled actions.</param>
    public ActionScheduler(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Schedules an action to be executed after a specified timeout in milliseconds
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="timeout">The timeout in milliseconds</param>
    /// <returns>A cancellation action</returns>
    public Action Delay(Action handle, int timeout) => Delay(handle, Duration.FromMilliseconds(timeout));

    /// <summary>
    /// Schedules an action to be executed after a specified timeout duration.
    /// Runs the callback on a background task; exceptions are surfaced via <see cref="Logger"/>.
    /// The execute/cancel flag uses <see cref="Interlocked"/> + <see cref="Volatile"/> reads
    /// so the cancellation Action's write is observed by the background task on weakly-ordered
    /// memory models.
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="timeout">The timeout duration</param>
    /// <returns>A cancellation action</returns>
    public Action Delay(Action handle, Duration timeout)
    {
        var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeout.ToTimeSpan(), cts.Token);
                handle();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                this.Error("scheduled action failed: {exception}", ex);
            }
        });

        // CTS is intentionally not disposed: the returned cancel action must stay safe to invoke
        // after the delay has already elapsed (when the background task is gone). Cancelling promptly
        // unblocks the pending Task.Delay; the ObjectDisposedException guard is defensive only.
        return () =>
        {
            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException) { }
        };
    }

    /// <summary>
    /// Schedules an action to be executed repeatedly at specified intervals in milliseconds
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="interval">The interval in milliseconds</param>
    /// <returns>A cancellation action</returns>
    public Action Interval(Action handle, int interval) => Interval(handle, Duration.FromMilliseconds(interval));

    /// <summary>
    /// Schedules an action to be executed repeatedly at specified interval duration
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="interval">The interval duration</param>
    /// <returns>A cancellation action</returns>
    public Action Interval(Action handle, Duration interval)
    {
        var span = interval.ToTimeSpan();
        var timer = new Timer(_ => handle(), null, span, span);

        return () => timer.Dispose();
    }
}
