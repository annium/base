using System;

namespace Annium.Internal;

/// <summary>
/// Provides an implementation of <see cref="IDisposable"/> that executes a specified action when disposed.
/// </summary>
internal class Disposer : IDisposable
{
    /// <summary>
    /// The action to execute when this object is disposed.
    /// </summary>
    private readonly Action _handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="Disposer"/> class.
    /// </summary>
    /// <param name="handle">The action to execute when this object is disposed.</param>
    public Disposer(Action handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() => _handle();
}
