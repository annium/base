using System;
using System.Threading.Tasks;
using Annium.Internal;
using Annium.Logging;

namespace Annium;

public static class Disposable
{
    public static DisposableBox Box(ILogger logger) => new(logger);
    public static AsyncDisposableBox AsyncBox(ILogger logger) => new(logger);
    public static IAsyncDisposable Create(Func<Task> handle) => new AsyncDisposer(handle);
    public static IDisposable Create(Action handle) => new Disposer(handle);
    public static readonly IDisposable Empty = new EmptyDisposer();

    public static IDisposableReference<TValue> Reference<TValue>(TValue value)
        where TValue : notnull
    {
        return new DisposableReference<TValue>(value, () => Task.CompletedTask);
    }

    public static IDisposableReference<TValue> Reference<TValue>(TValue value, Func<Task> dispose)
        where TValue : notnull
    {
        return new DisposableReference<TValue>(value, dispose);
    }
}