using System;
using System.Threading.Tasks;

namespace Annium.Internal;

/// <summary>
/// Provides a thread-safe reference to a value that can be disposed asynchronously.
/// </summary>
/// <typeparam name="TValue">The type of the value to reference.</typeparam>
internal class DisposableReference<TValue> : IDisposableReference<TValue>
    where TValue : notnull
{
    /// <summary>
    /// Gets the referenced value.
    /// </summary>
    public TValue Value { get; private set; }

    /// <summary>
    /// The asynchronous function to execute when this reference is disposed.
    /// </summary>
    private readonly Func<Task> _dispose;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableReference{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value to reference.</param>
    /// <param name="dispose">The asynchronous function to execute when this reference is disposed.</param>
    public DisposableReference(TValue value, Func<Task> dispose)
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
        Value = default!;
        await _dispose();
    }
}
