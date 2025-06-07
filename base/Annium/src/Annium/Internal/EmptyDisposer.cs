using System;

namespace Annium.Internal;

/// <summary>
/// Provides an implementation of <see cref="IDisposable"/> that does nothing when disposed.
/// </summary>
internal class EmptyDisposer : IDisposable
{
    /// <summary>
    /// Performs no operation when called.
    /// </summary>
    public void Dispose() { }
}
