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
}