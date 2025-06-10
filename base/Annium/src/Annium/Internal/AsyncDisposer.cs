using System;
using System.Threading.Tasks;

namespace Annium.Internal;

/// <summary>
/// Provides an implementation of <see cref="IAsyncDisposable"/> that executes a specified asynchronous function when disposed.
/// </summary>
internal class AsyncDisposer : IAsyncDisposable
{
    /// <summary>
    /// The asynchronous function to execute when this object is disposed.
    /// </summary>
    private readonly Func<Task> _handle;

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
    public async ValueTask DisposeAsync() => await _handle();
}
