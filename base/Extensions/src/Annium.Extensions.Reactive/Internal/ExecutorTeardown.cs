using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;

namespace Annium.Extensions.Reactive.Internal;

/// <summary>
/// The single way the parallel/sequential reactive operators end a subscription: it disposes the
/// executor on a background task and then sends the one terminal notification the observer is allowed
/// to receive.
/// </summary>
/// <typeparam name="T">The observed sequence element type</typeparam>
/// <remarks>
/// Both the source and the caller's own handler can end the sequence, and they can do so at the same
/// time - a source that emits and completes in one go has its completion scheduled while the handler for
/// an earlier value has not run yet. Letting each path dispose and notify on its own meant whichever
/// background task finished first decided the outcome, so a real handler failure could be replaced by the
/// completion that happened to win. Teardown therefore happens once: the first caller starts it, later
/// ones only record what they know, and the failure - whenever it was recorded - beats the completion.
/// </remarks>
internal sealed class ExecutorTeardown<T>
{
    /// <summary>
    /// Gets a value indicating whether the sequence has already failed, so scheduled work still queued
    /// behind the failure can stop rather than emit values after it.
    /// </summary>
    public bool HasFailed => Volatile.Read(ref _error) is not null;

    /// <summary>
    /// The executor running the scheduled work.
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// The observer to notify.
    /// </summary>
    private readonly IObserver<T> _observer;

    /// <summary>
    /// The failure that ended the sequence, if any. The first one recorded wins.
    /// </summary>
    private Exception? _error;

    /// <summary>
    /// Set to 1 by the caller that starts the teardown.
    /// </summary>
    private int _teardownStarted;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutorTeardown{T}"/> class.
    /// </summary>
    /// <param name="executor">The executor running the scheduled work.</param>
    /// <param name="observer">The observer to notify once.</param>
    public ExecutorTeardown(IExecutor executor, IObserver<T> observer)
    {
        _executor = executor;
        _observer = observer;
    }

    /// <summary>
    /// Ends the sequence with the given failure.
    /// </summary>
    /// <param name="error">The failure raised by the source or by the caller's handler.</param>
    public void Fail(Exception error)
    {
        Interlocked.CompareExchange(ref _error, error, null);

        Terminate();
    }

    /// <summary>
    /// Ends the sequence normally, unless a failure was recorded - including one raised by work that is
    /// still queued and only drains during the disposal below.
    /// </summary>
    public void Complete() => Terminate();

    /// <summary>
    /// Starts the teardown, once. Later callers return immediately: the one that got there first
    /// notifies, and reads the recorded failure after the executor has drained, so a failure raised while
    /// draining is still the one reported.
    /// </summary>
    private void Terminate()
    {
        if (Interlocked.Exchange(ref _teardownStarted, 1) == 1)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await _executor.DisposeAsync();
            }
            catch (OperationCanceledException)
            {
                // the executor was cancelled rather than failing - that is not something the observer
                // needs to hear about on top of whatever ended the sequence
            }
            catch (Exception e)
            {
                Interlocked.CompareExchange(ref _error, e, null);
            }

            var error = Volatile.Read(ref _error);
            if (error is null)
                _observer.OnCompleted();
            else
                _observer.OnError(error);
        });
    }
}
