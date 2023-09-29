using System;
using System.Threading.Tasks;

namespace Annium.Internal;

internal class AsyncDisposer : IAsyncDisposable
{
    private readonly Func<Task> _handle;

    public AsyncDisposer(Func<Task> handle)
    {
        _handle = handle;
    }

    public async ValueTask DisposeAsync() => await _handle();
}