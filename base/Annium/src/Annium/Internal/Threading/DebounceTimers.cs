using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Internal.Threading;

internal class DebounceTimer<T> : DebounceTimerBase
{
    private readonly T _state;
    private readonly Func<T, ValueTask> _handler;

    public DebounceTimer(T state, Func<T, ValueTask> handler, int period, ILogger logger)
        : base(period, logger)
    {
        _state = state;
        _handler = handler;
    }

    protected override ValueTask HandleAsync()
    {
        return _handler(_state);
    }
}

internal class DebounceTimer : DebounceTimerBase
{
    private readonly Func<ValueTask> _handler;

    public DebounceTimer(Func<ValueTask> handler, int period, ILogger logger)
        : base(period, logger)
    {
        _handler = handler;
    }

    protected override ValueTask HandleAsync()
    {
        return _handler();
    }
}

internal abstract class DebounceTimerBase : IDebounceTimer, ILogSubject
{
    public ILogger Logger { get; }
    private readonly Timer _timer;
    private int _period;
    private volatile int _isRequested;
    private volatile int _isHandling;

    protected DebounceTimerBase(int period, ILogger logger)
    {
        Logger = logger;
        _timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
        _period = period;
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
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

    protected abstract ValueTask HandleAsync();

#pragma warning disable VSTHRD100
    private async void Callback(object? _)
#pragma warning restore VSTHRD100
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
            return;

        _isRequested = 0;

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

            if (_isRequested == 1)
                Request();
        }
    }
}
