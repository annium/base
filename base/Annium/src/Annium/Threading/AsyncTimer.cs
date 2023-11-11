using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Threading;

public sealed class AsyncTimer<T> : IDisposable
{
    private readonly Func<T, ValueTask> _handler;
    private readonly Timer _timer;
    private int _isHandling;

    internal AsyncTimer(T state, Func<T, ValueTask> handler, int dueTime, int period)
    {
        _handler = handler;
        _timer = new Timer(Handle, state, dueTime, period);
    }

    internal AsyncTimer(T state, Func<T, ValueTask> handler, TimeSpan dueTime, TimeSpan period)
    {
        _handler = handler;
        _timer = new Timer(Handle, state, dueTime, period);
    }

    public void Change(int dueTime, int period)
    {
        _timer.Change(dueTime, period);
    }

    public void Change(TimeSpan dueTime, TimeSpan period)
    {
        _timer.Change(dueTime, period);
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private async void Handle(object? state)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            return;
        }

        await _handler((T)state!);
        _isHandling = 0;
    }
}

public sealed class AsyncTimer : IDisposable
{
    public static AsyncTimer Create(Func<ValueTask> handler, int dueTime, int period) => new(handler, dueTime, period);

    public static AsyncTimer Create(Func<ValueTask> handler, TimeSpan dueTime, TimeSpan period) =>
        new(handler, dueTime, period);

    public static AsyncTimer<T> Create<T>(T state, Func<T, ValueTask> handler, int dueTime, int period) =>
        new(state, handler, dueTime, period);

    public static AsyncTimer<T> Create<T>(T state, Func<T, ValueTask> handler, TimeSpan dueTime, TimeSpan period) =>
        new(state, handler, dueTime, period);

    private readonly Func<ValueTask> _handler;
    private readonly Timer _timer;
    private int _isHandling;

    private AsyncTimer(Func<ValueTask> handler, int dueTime, int period)
    {
        _handler = handler;
        _timer = new Timer(Handle, null, dueTime, period);
    }

    private AsyncTimer(Func<ValueTask> handler, TimeSpan dueTime, TimeSpan period)
    {
        _handler = handler;
        _timer = new Timer(Handle, null, dueTime, period);
    }

    public void Change(int dueTime, int period)
    {
        _timer.Change(dueTime, period);
    }

    public void Change(TimeSpan dueTime, TimeSpan period)
    {
        _timer.Change(dueTime, period);
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private async void Handle(object? state)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            return;
        }

        await _handler();
        _isHandling = 0;
    }
}
