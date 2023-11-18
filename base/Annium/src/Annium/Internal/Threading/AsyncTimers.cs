using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Threading;

namespace Annium.Internal.Threading;

internal class AsyncTimer<T> : AsyncTimerBase
    where T : class
{
    private readonly Func<T, ValueTask> _handler;

    public AsyncTimer(T state, Func<T, ValueTask> handler, int dueTime, int period)
        : base(state, dueTime, period)
    {
        _handler = handler;
    }

    protected override ValueTask Handle(object? state)
    {
        return _handler((T)state.NotNull());
    }
}

internal class AsyncTimer : AsyncTimerBase
{
    private readonly Func<ValueTask> _handler;

    public AsyncTimer(Func<ValueTask> handler, int dueTime, int period)
        : base(null, dueTime, period)
    {
        _handler = handler;
    }

    protected override ValueTask Handle(object? state)
    {
        return _handler();
    }
}

internal abstract class AsyncTimerBase : IAsyncTimer
{
    private readonly Timer _timer;
    private volatile int _isHandling;

    protected AsyncTimerBase(object? state, int dueTime, int period)
    {
        _timer = new Timer(Callback, state, dueTime, period);
    }

    public void Change(int dueTime, int period)
    {
        _timer.Change(dueTime, period);
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    protected abstract ValueTask Handle(object? state);

    private async void Callback(object? state)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            return;
        }

        await Handle(state);

        _isHandling = 0;
    }
}
