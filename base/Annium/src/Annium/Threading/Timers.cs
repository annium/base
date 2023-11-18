using System;
using System.Threading.Tasks;
using Annium.Internal.Threading;
using Annium.Logging;

namespace Annium.Threading;

public static class Timers
{
    public static IAsyncTimer Async(Func<ValueTask> handler, int dueTime, int period) =>
        new AsyncTimer(handler, dueTime, period);

    public static IAsyncTimer Async<T>(T state, Func<T, ValueTask> handler, int dueTime, int period)
        where T : class => new AsyncTimer<T>(state, handler, dueTime, period);

    public static IDebounceTimer Debounce(Func<ValueTask> handler, int period, ILogger logger) =>
        new DebounceTimer(handler, period, logger);

    public static IDebounceTimer Debounce<T>(T state, Func<T, ValueTask> handler, int period, ILogger logger)
        where T : class => new DebounceTimer<T>(state, handler, period, logger);
}
