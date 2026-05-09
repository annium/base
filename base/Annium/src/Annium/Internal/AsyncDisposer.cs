using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Internal;

/// <summary>
/// Provides an implementation of <see cref="IAsyncDisposable"/> that executes a specified asynchronous
/// function when disposed. The handle runs at most once: subsequent <see cref="DisposeAsync"/> calls are
/// no-ops, satisfying the <see cref="IAsyncDisposable"/> contract that disposal be idempotent.
/// </summary>
internal sealed class AsyncDisposer : IAsyncDisposable
{
    /// <summary>
    /// The asynchronous function to execute when this object is disposed.
    /// </summary>
    private readonly Func<Task> _handle;

    /// <summary>
    /// 0 if the handle has not yet been run; 1 once an in-flight or completed dispose has claimed it.
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDisposer"/> class.
    /// </summary>
    /// <param name="handle">The asynchronous function to execute when this object is disposed.</param>
    public AsyncDisposer(Func<Task> handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        await _handle().ConfigureAwait(false);
    }
}
