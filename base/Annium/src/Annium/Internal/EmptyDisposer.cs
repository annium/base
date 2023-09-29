using System;

namespace Annium.Internal;

internal class EmptyDisposer : IDisposable
{
    public void Dispose()
    {
    }
}