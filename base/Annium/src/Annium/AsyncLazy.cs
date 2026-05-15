using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium;

/// <summary>
/// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
/// </summary>
/// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
public sealed class AsyncLazy<T>
{
    /// <summary>
    /// Gets a value indicating whether the value has been created.
    /// </summary>
    public bool IsValueCreated => _isValueCreated;

    /// <summary>
    /// Asynchronously gets the value, kicking off the underlying factory the first time it is
    /// accessed. The cancellation token cancels the wait, not the underlying factory (which
    /// <see cref="Lazy{T}"/> guarantees runs at most once).
    /// </summary>
    /// <param name="ct">Cancellation token for the wait.</param>
    /// <returns>A value task that completes with the lazy's value.</returns>
    public ValueTask<T> GetValueAsync(CancellationToken ct = default)
    {
        var task = _instance.Value;
        // IsCompletedSuccessfully (not IsCompleted) so faulted/cancelled tasks take the slow
        // path via WaitAsync — that propagates the original exception unwrapped, while the
        // fast path's task.Result would throw AggregateException for faulted/cancelled tasks.
        // task.Result on a successfully-completed task is non-blocking — analyzer is being
        // conservative here.
#pragma warning disable VSTHRD103
        return task.IsCompletedSuccessfully
            ? new ValueTask<T>(task.Result)
            : new ValueTask<T>(task.WaitAsync(ct));
#pragma warning restore VSTHRD103
    }

    /// <summary>
    /// The underlying lazy task.
    /// </summary>
    private readonly Lazy<Task<T>> _instance;

    /// <summary>
    /// Indicates whether the value has been created.
    /// </summary>
    private volatile bool _isValueCreated;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class with a synchronous factory.
    /// </summary>
    /// <param name="factory">The delegate that is invoked on a background thread to produce the value when it is needed.</param>
    public AsyncLazy(Func<T> factory)
    {
#pragma warning disable VSTHRD011
        _instance = new Lazy<Task<T>>(
#pragma warning restore VSTHRD011
            async () =>
            {
                var value = await Task.Run(factory).ConfigureAwait(false);
                _isValueCreated = true;
                return value;
            },
            isThreadSafe: true
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class with an asynchronous factory.
    /// </summary>
    /// <param name="factory">The asynchronous delegate that is invoked on a background thread to produce the value when it is needed.</param>
    public AsyncLazy(Func<Task<T>> factory)
    {
#pragma warning disable VSTHRD011
        _instance = new Lazy<Task<T>>(
#pragma warning restore VSTHRD011
            async () =>
            {
                var value = await Task.Run(factory).ConfigureAwait(false);
                _isValueCreated = true;
                return value;
            },
            isThreadSafe: true
        );
    }

    /// <summary>
    /// Gets the awaiter for the asynchronous operation.
    /// </summary>
    /// <returns>A <see cref="TaskAwaiter{T}"/> for the asynchronous operation.</returns>
    public TaskAwaiter<T> GetAwaiter()
    {
        return _instance.Value.GetAwaiter();
    }
}
