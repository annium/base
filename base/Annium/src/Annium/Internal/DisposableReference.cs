using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Internal;

/// <summary>
/// Provides a thread-safe reference to a value that can be disposed asynchronously. The dispose handle
/// runs at most once: subsequent <see cref="DisposeAsync"/> calls are no-ops, satisfying the
/// <see cref="IAsyncDisposable"/> contract that disposal be idempotent.
/// </summary>
/// <typeparam name="TValue">The type of the value to reference.</typeparam>
internal sealed class DisposableReference<TValue> : IDisposableReference<TValue>
    where TValue : notnull
{
    /// <summary>
    /// Gets the referenced value.
    /// </summary>
    public TValue Value { get; private set; }

    /// <summary>
    /// The asynchronous function to execute when this reference is disposed.
    /// </summary>
    private readonly Func<ValueTask> _dispose;

    /// <summary>
    /// 0 if the handle has not yet been run; 1 once a dispose has claimed it.
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableReference{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value to reference.</param>
    /// <param name="dispose">The asynchronous function to execute when this reference is disposed.</param>
    public DisposableReference(TValue value, Func<ValueTask> dispose)
    {
        Value = value;
        _dispose = dispose;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        // Run the dispose callback BEFORE nulling Value so closures over `this.Value` (or racing readers)
        // see the live value during the asynchronous teardown rather than `default`. The idempotency
        // guard above already ensures _dispose() runs exactly once. Value is intentionally nulled
        // post-dispose; the `notnull` constraint only governs live references — callers must not access
        // Value after DisposeAsync returns, and the `default!` suppression here documents that invariant.
        await _dispose().ConfigureAwait(false);
        Value = default!;
    }
}
