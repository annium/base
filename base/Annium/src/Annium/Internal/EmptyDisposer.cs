using System;

namespace Annium.Internal;

/// <summary>
/// Provides an implementation of <see cref="IDisposable"/> that does nothing when disposed.
/// </summary>
internal sealed class EmptyDisposer : IDisposable
{
    /// <summary>
    /// Performs no operation when called.
    /// </summary>
    public void Dispose() { }
}
