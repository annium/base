using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Threading;

namespace Annium.Internal.Threading;

internal class DebounceTimer<T> : DebounceTimerBase
{
    private readonly Func<T, ValueTask> _handler;

    public DebounceTimer(T state, Func<T, ValueTask> handler, int period)
        : base(state, period)
    {
        _handler = handler;
    }

    protected override ValueTask Handle(object? state)
    {
        return _handler((T)state.NotNull());
    }
}

internal class DebounceTimer : DebounceTimerBase
{
    private readonly Func<ValueTask> _handler;

    public DebounceTimer(Func<ValueTask> handler, int period)
        : base(null, period)
    {
        _handler = handler;
    }

    protected override ValueTask Handle(object? state)
    {
        return _handler();
    }
}

internal abstract class DebounceTimerBase : IDebounceTimer
{
    private readonly Timer _timer;
    private int _period;
    private volatile int _isRequested;
    private volatile int _isHandling;

    protected DebounceTimerBase(object? state, int period)
    {
        _timer = new Timer(Callback, state, Timeout.Infinite, Timeout.Infinite);
        _period = period;
    }

    public void Change(int period)
    {
        _period = period;
    }

    public void Request()
    {
        _timer.Change(_period, Timeout.Infinite);
        _isRequested = 1;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    protected abstract ValueTask Handle(object? state);

    private async void Callback(object? state)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
            return;

        _isRequested = 0;

        await Handle(state);

        _isHandling = 0;

        if (_isRequested == 1)
            Request();
    }
}
