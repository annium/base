using System;

namespace Annium.Extensions.Jobs;

public static class Timer
{
    public static IDisposable Start(Action handle, int dueTime, int period) =>
        new System.Threading.Timer(_ => handle(), null, dueTime, period);

    public static IDisposable Start(Action handle, uint dueTime, uint period) =>
        new System.Threading.Timer(_ => handle(), null, dueTime, period);

    public static IDisposable Start(Action handle, long dueTime, long period) =>
        new System.Threading.Timer(_ => handle(), null, dueTime, period);

    public static IDisposable Start(Action handle, TimeSpan dueTime, TimeSpan period) =>
        new System.Threading.Timer(_ => handle(), null, dueTime, period);

    public static IDisposable Start<T>(Action<T> handle, T state, int dueTime, int period) =>
        new System.Threading.Timer(x => handle((T)x!), state, dueTime, period);

    public static IDisposable Start<T>(Action<T> handle, T state, uint dueTime, uint period) =>
        new System.Threading.Timer(x => handle((T)x!), state, dueTime, period);

    public static IDisposable Start<T>(Action<T> handle, T state, long dueTime, long period) =>
        new System.Threading.Timer(x => handle((T)x!), state, dueTime, period);

    public static IDisposable Start<T>(Action<T> handle, T state, TimeSpan dueTime, TimeSpan period) =>
        new System.Threading.Timer(x => handle((T)x!), state, dueTime, period);
}