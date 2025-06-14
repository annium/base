using System.Threading.Tasks;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Provides extension methods for waiting on observable completion
/// </summary>
public static class WhenCompletedExtensions
{
    /// <summary>
    /// Asynchronously waits for an observable to complete
    /// </summary>
    /// <typeparam name="TSource">The type of items emitted by the observable</typeparam>
    /// <param name="source">The observable to wait for completion</param>
    /// <param name="logger">Logger for tracking the wait operation</param>
    /// <returns>A task that completes when the observable completes</returns>
    public static async Task WhenCompletedAsync<TSource>(this IObservable<TSource> source, ILogger logger)
    {
        var ctx = new CompletionContext(logger);
        var tcs = new TaskCompletionSource<object?>();
        ctx.Trace("subscribe");
        using var _ = source.Subscribe(
            delegate { },
            () =>
            {
                ctx.Trace("set - start");
                tcs.SetResult(null);
                ctx.Trace("set - done");
            }
        );
        ctx.Trace("wait");
        await tcs.Task;
        ctx.Trace("done");
    }
}

/// <summary>
/// Context for tracking completion operations with logging support
/// </summary>
/// <param name="Logger">The logger instance for tracking completion events</param>
file record CompletionContext(ILogger Logger) : ILogSubject;
