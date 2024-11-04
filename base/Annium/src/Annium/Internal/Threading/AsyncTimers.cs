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
        : base(logger)
    {
        _state = state;
        _handler = handler;
        Timer = new Timer(Callback, null, dueTime, period);
    }

    protected override ValueTask HandleAsync()
    {
        return _handler(_state);
    }
}

internal class AsyncTimer : AsyncTimerBase
{
    private readonly Func<ValueTask> _handler;

    public AsyncTimer(Func<ValueTask> handler, int dueTime, int period, ILogger logger)
        : base(logger)
    {
        _handler = handler;
        Timer = new Timer(Callback, null, dueTime, period);
    }

    protected override ValueTask HandleAsync()
    {
        return _handler();
    }
}

internal abstract class AsyncTimerBase : ISequentialTimer, ILogSubject
{
    public ILogger Logger { get; }
    protected Timer Timer { get; init; } = default!;
    private volatile int _isHandling;

    protected AsyncTimerBase(ILogger logger)
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

    protected abstract ValueTask HandleAsync();

#pragma warning disable VSTHRD100
    protected async void Callback(object? _)
#pragma warning restore VSTHRD100
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            return;
        }

        try
        {
            await HandleAsync();
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
