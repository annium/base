using System;
using System.Threading;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Internal.Threading;

internal class SyncTimer<T> : SyncTimerBase
    where T : class
{
    private readonly T _state;
    private readonly Action<T> _handler;

    public SyncTimer(T state, Action<T> handler, int dueTime, int period, ILogger logger)
        : base(logger)
    {
        _state = state;
        _handler = handler;
        Timer = new Timer(Callback, null, dueTime, period);
    }

    protected override void Handle()
    {
        _handler(_state);
    }
}

internal class SyncTimer : SyncTimerBase
{
    private readonly Action _handler;

    public SyncTimer(Action handler, int dueTime, int period, ILogger logger)
        : base(logger)
    {
        _handler = handler;
        Timer = new Timer(Callback, null, dueTime, period);
    }

    protected override void Handle()
    {
        _handler();
    }
}

internal abstract class SyncTimerBase : ISequentialTimer, ILogSubject
{
    public ILogger Logger { get; }
    protected Timer Timer { get; init; } = default!;
    private volatile int _isHandling;

    protected SyncTimerBase(ILogger logger)
    {
        Logger = logger;
    }

    public void Dispose()
    {
        Timer.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Change(int dueTime, int period)
    {
        Timer.Change(dueTime, period);
    }

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        return Timer.Change(dueTime, period);
    }

    protected abstract void Handle();

    protected void Callback(object? _)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            return;
        }

        try
        {
            Handle();
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
