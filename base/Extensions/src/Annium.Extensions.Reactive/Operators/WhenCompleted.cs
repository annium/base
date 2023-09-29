using System.Threading.Tasks;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

public static class WhenCompletedExtensions
{
    public static async Task WhenCompleted<TSource>(
        this IObservable<TSource> source,
        ILogger logger
    )
    {
        var ctx = new CompletionContext(logger);
        var tcs = new TaskCompletionSource<object?>();
        ctx.Trace("subscribe");
        using var _ = source.Subscribe(delegate { }, () =>
        {
            ctx.Trace("set - start");
            tcs.SetResult(null);
            ctx.Trace("set - done");
        });
        ctx.Trace("wait");
        await tcs.Task;
        ctx.Trace("done");
    }
}

file record CompletionContext(ILogger Logger) : ILogSubject;