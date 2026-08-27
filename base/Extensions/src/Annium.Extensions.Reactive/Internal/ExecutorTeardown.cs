using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;

namespace Annium.Extensions.Reactive.Internal;

/// <summary>
/// The single way the parallel/sequential reactive operators reach their observer: every notification
/// goes through here, and ending the subscription disposes the executor on a background task before
/// sending the one terminal notification the observer is allowed to receive.
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
    /// Gets a value indicating whether the sequence has already failed, so scheduled work that has not
    /// started yet can skip its handler instead of doing work whose result nobody wants.
    /// </summary>
    /// <remarks>
    /// This is not an ordering guarantee. On the sequential executor it is one - items run strictly one
    /// at a time, so nothing after the failing item starts. On the parallel executor the later items are
    /// already in flight and will finish, so values may still be emitted after the failure was recorded.
    /// They are emitted before the terminal notification either way, so the sequence stays well-formed.
    /// </remarks>
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
    /// Held for the duration of every notification. Rx observers are written against a grammar in which
    /// notifications never overlap, so on the parallel executor - where several items are in flight at
    /// once - the work runs in parallel but its delivery does not.
    /// </summary>
    private readonly Lock _gate = new();

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
    /// Passes a value on to the observer, one at a time.
    /// </summary>
    /// <param name="value">The value to deliver.</param>
    public void Next(T value)
    {
        lock (_gate)
            _observer.OnNext(value);
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
    /// Abandons the subscription without notifying anyone: the subscriber disposed it, and Rx does not
    /// ask for a terminal notification after that. The executor is still disposed - otherwise its
    /// background loop outlives the subscription that created it.
    /// </summary>
    public void Cancel()
    {
        if (Interlocked.Exchange(ref _teardownStarted, 1) == 1)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await _executor.DisposeAsync();
            }
            catch (Exception)
            {
                // nobody is left to tell
            }
        });
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
            lock (_gate)
            {
                if (error is null)
                    _observer.OnCompleted();
                else
                    _observer.OnError(error);
            }
        });
    }
}
