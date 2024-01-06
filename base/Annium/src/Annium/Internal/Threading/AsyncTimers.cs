using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Internal.Threading;

internal class AsyncTimer<T> : AsyncTimerBase
    where T : class
{
    private readonly T _state;
    private readonly Func<T, ValueTask> _handler;

    public AsyncTimer(T state, Func<T, ValueTask> handler, int dueTime, int period, ILogger logger)
        : base(dueTime, period, logger)
    {
        _state = state;
        _handler = handler;
    }

    protected override ValueTask Handle()
    {
        return _handler(_state);
    }
}

internal class AsyncTimer : AsyncTimerBase
{
    private readonly Func<ValueTask> _handler;

    public AsyncTimer(Func<ValueTask> handler, int dueTime, int period, ILogger logger)
        : base(dueTime, period, logger)
    {
        _handler = handler;
    }

    protected override ValueTask Handle()
    {
        return _handler();
    }
}

internal abstract class AsyncTimerBase : IAsyncTimer, ILogSubject
{
    public ILogger Logger { get; }
    private readonly Timer _timer;
    private volatile int _isHandling;

    protected AsyncTimerBase(int dueTime, int period, ILogger logger)
    {
        Logger = logger;
        _timer = new Timer(Callback, null, dueTime, period);
    }

    public void Change(int dueTime, int period)
    {
        _timer.Change(dueTime, period);
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    protected abstract ValueTask Handle();

    private async void Callback(object? _)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            return;
        }

        try
        {
            await Handle();
        }
        catch (Exception e)
        {
            this.Error(e);
        }
        finally
        {
            _isHandling = 0;
        }
    }
}
