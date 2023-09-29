using System;
using System.Threading.Tasks;

namespace Annium;

public static class DisposableExtensions
{
    public static ValueTask DisposeAsync(this IDisposable disposable)
    {
        if (disposable is IAsyncDisposable asyncDisposable)
            return asyncDisposable.DisposeAsync();

        disposable.Dispose();

        return new ValueTask(Task.CompletedTask);
    }
}