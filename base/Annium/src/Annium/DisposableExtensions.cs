using System;
using System.Threading.Tasks;

namespace Annium;

/// <summary>
/// Provides extension methods for disposable resources.
/// </summary>
public static class DisposableExtensions
{
    /// <summary>
    /// Asynchronously disposes a resource.
    /// </summary>
    /// <param name="disposable">The disposable resource to dispose.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public static ValueTask DisposeAsync(this IDisposable disposable)
    {
        if (disposable is IAsyncDisposable asyncDisposable)
            return asyncDisposable.DisposeAsync();

        disposable.Dispose();

        return new ValueTask(Task.CompletedTask);
    }
}
