using System;
using System.Threading;

namespace Annium.Internal;

/// <summary>
/// Provides an implementation of <see cref="IDisposable"/> that executes a specified action when disposed.
/// The handle runs at most once: subsequent <see cref="Dispose"/> calls are no-ops, satisfying the
/// <see cref="IDisposable"/> contract that disposal be idempotent.
/// </summary>
internal sealed class Disposer : IDisposable
{
    /// <summary>
    /// The action to execute when this object is disposed.
    /// </summary>
    private readonly Action _handle;

    /// <summary>
    /// 0 if the handle has not yet been run; 1 once a dispose has claimed it.
    /// </summary>
    private int _disposed;

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
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        _handle();
    }
}
