using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Annium.Logging.Shared.Internal;

internal class BackgroundLogScheduler<TContext> : ILogScheduler<TContext>, ILogSubject, IAsyncDisposable
    where TContext :
    class, ILogContext
{
    public ILogger Logger { get; }
    public Func<LogMessage<TContext>, bool> Filter { get; }
    private int Count => _messageReader.CanCount ? _messageReader.Count : -1;
    private bool _isDisposed;
    private readonly ChannelReader<LogMessage<TContext>> _messageReader;
    private readonly ChannelWriter<LogMessage<TContext>> _messageWriter;
    private readonly CancellationTokenSource _observableCts = new();
    private readonly IObservable<LogMessage<TContext>> _observable;
    private readonly IDisposable _subscription;

    public BackgroundLogScheduler(
        Func<LogMessage<TContext>, bool> filter,
        IAsyncLogHandler<TContext> handler,
        LogRouteConfiguration configuration
    )
    {
        if (configuration.BufferTime == TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(configuration.BufferTime), "Buffer time is expected to be non-zero");

        if (configuration.BufferCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(configuration.BufferCount), "Buffer count is expected to be positive");

        Logger = VoidLogger.Instance;
        Filter = filter;

        var channel = Channel.CreateUnbounded<LogMessage<TContext>>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = true,
            SingleWriter = false,
            SingleReader = true
        });
        _messageWriter = channel.Writer;
        _messageReader = channel.Reader;
        _observable = ObservableExt.StaticSyncInstance<LogMessage<TContext>>(Run, _observableCts.Token, VoidLogger.Instance).TrackCompletion(VoidLogger.Instance);
        _subscription = _observable
            .Buffer(configuration.BufferTime, configuration.BufferCount)
            .Where(x => x.Count > 0)
            .DoParallelAsync(async x => await handler.Handle(x.ToArray()))
            .Subscribe();
    }

    public void Handle(LogMessage<TContext> message)
    {
        EnsureNotDisposed();

        lock (_messageWriter)
            if (!_messageWriter.TryWrite(message))
                throw new InvalidOperationException("Message must have been written to channel");
    }

    private async Task<Func<Task>> Run(ObserverContext<LogMessage<TContext>> ctx)
    {
        this.Trace("start");

        // normal mode - runs task immediately or waits for one
        while (!Volatile.Read(ref _isDisposed))
        {
            try
            {
                var message = await _messageReader.ReadAsync(ctx.Ct);
                ctx.OnNext(message);
            }
            catch (ChannelClosedException)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        // shutdown mode - handle only left tasks
        this.Trace("handle {count} messages left", Count);
        while (true)
        {
            if (_messageReader.TryRead(out var message))
                ctx.OnNext(message);
            else
                break;
        }

        this.Trace("done");

        return () => Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");
        EnsureNotDisposed();
        Volatile.Write(ref _isDisposed, true);
        lock (_messageWriter)
            _messageWriter.Complete();
        this.Trace("wait for reader completion");
        await _messageReader.Completion;
        this.Trace("cancel observable cts");
        _observableCts.Cancel();
        this.Trace("await observable");
        await _observable.WhenCompleted(Logger);
        this.Trace("dispose subscription");
        _subscription.Dispose();
        this.Trace("done");
    }

    private void EnsureNotDisposed()
    {
        if (Volatile.Read(ref _isDisposed))
            throw new InvalidOperationException("Log scheduler is already disposed");
    }
}