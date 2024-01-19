using System;
using System.Threading.Tasks;
using Annium.Internal.Threading;
using Annium.Logging;

namespace Annium.Threading;

public static class Timers
{
    public static ISequentialTimer Sync(Action handler, int dueTime, int period, ILogger logger)
    {
        return new SyncTimer(handler, dueTime, period, logger);
    }

    public static ISequentialTimer Sync<T>(T state, Action<T> handler, int dueTime, int period, ILogger logger)
        where T : class
    {
        return new SyncTimer<T>(state, handler, dueTime, period, logger);
    }

    public static ISequentialTimer Async(Func<ValueTask> handler, int dueTime, int period, ILogger logger)
    {
        return new AsyncTimer(handler, dueTime, period, logger);
    }

    public static ISequentialTimer Async<T>(T state, Func<T, ValueTask> handler, int dueTime, int period, ILogger logger)
        where T : class
    {
        return new AsyncTimer<T>(state, handler, dueTime, period, logger);
    }

    public static IDebounceTimer Debounce(Func<ValueTask> handler, int period, ILogger logger)
    {
        return new DebounceTimer(handler, period, logger);
    }

    public static IDebounceTimer Debounce<T>(T state, Func<T, ValueTask> handler, int period, ILogger logger)
        where T : class
    {
        return new DebounceTimer<T>(state, handler, period, logger);
    }
}