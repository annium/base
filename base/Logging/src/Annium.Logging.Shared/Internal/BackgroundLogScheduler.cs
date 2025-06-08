using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Background log scheduler that processes log messages asynchronously in a separate task
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class BackgroundLogScheduler<TContext> : ILogScheduler<TContext>, ILogSubject, IAsyncDisposable
    where TContext : class
{
    /// <summary>
    /// Gets the logger instance for this scheduler
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the filter function for determining which messages to process
    /// </summary>
    public Func<LogMessage<TContext>, bool> Filter { get; }

    /// <summary>
    /// Gets the count of messages in the queue
    /// </summary>
    private int Count => _messageReader.CanCount ? _messageReader.Count : -1;

    /// <summary>
    /// Indicates whether this scheduler has been disposed
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Channel reader for consuming log messages
    /// </summary>
    private readonly ChannelReader<LogMessage<TContext>> _messageReader;

    /// <summary>
    /// Channel writer for producing log messages
    /// </summary>
    private readonly ChannelWriter<LogMessage<TContext>> _messageWriter;

    /// <summary>
    /// Cancellation token source for the observable stream
    /// </summary>
    private readonly CancellationTokenSource _observableCts = new();

    /// <summary>
    /// Observable stream for processing log messages
    /// </summary>
    private readonly IObservable<LogMessage<TContext>> _observable;

    /// <summary>
    /// Subscription to the observable stream
    /// </summary>
    private readonly IDisposable _subscription;

    public BackgroundLogScheduler(
        Func<LogMessage<TContext>, bool> filter,
        IAsyncLogHandler<TContext> handler,
        LogRouteConfiguration configuration
    )
    {
        if (configuration.BufferTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(configuration.BufferTime),
                "Buffer time is expected to be non-zero"
            );

        if (configuration.BufferCount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(configuration.BufferCount),
                "Buffer count is expected to be positive"
            );

        Logger = VoidLogger.Instance;
        Filter = filter;

        var channel = Channel.CreateUnbounded<LogMessage<TContext>>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleWriter = false,
                SingleReader = true,
            }
        );
        _messageWriter = channel.Writer;
        _messageReader = channel.Reader;
        _observable = ObservableExt
            .StaticSyncInstance<LogMessage<TContext>>(RunAsync, _observableCts.Token, VoidLogger.Instance)
            .TrackCompletion(VoidLogger.Instance);
        _subscription = _observable
            .Buffer(configuration.BufferTime, configuration.BufferCount)
            .Where(x => x.Count > 0)
            .DoSequentialAsync(async x => await handler.HandleAsync(x.AsReadOnly()))
            .Subscribe();
    }

    /// <summary>
    /// Handles a log message by queuing it for background processing
    /// </summary>
    /// <param name="message">The log message to handle</param>
    public void Handle(LogMessage<TContext> message)
    {
        EnsureNotDisposed();

        lock (_messageWriter)
            if (!_messageWriter.TryWrite(message))
                throw new InvalidOperationException("Message must have been written to channel");
    }

    /// <summary>
    /// Runs the background task for processing log messages
    /// </summary>
    /// <param name="ctx">The observer context for the stream</param>
    /// <returns>A task that represents the background processing operation</returns>
    private async Task<Func<Task>> RunAsync(ObserverContext<LogMessage<TContext>> ctx)
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

    /// <summary>
    /// Disposes the scheduler and completes any remaining log processing
    /// </summary>
    /// <returns>A task that represents the disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");
        EnsureNotDisposed();
        Volatile.Write(ref _isDisposed, true);
        lock (_messageWriter)
            _messageWriter.Complete();
        this.Trace("wait for reader completion");
#pragma warning disable VSTHRD003
        await _messageReader.Completion;
#pragma warning restore VSTHRD003
        this.Trace("cancel observable cts");
        await _observableCts.CancelAsync();
        this.Trace("await observable");
        await _observable.WhenCompletedAsync(Logger);
        this.Trace("dispose subscription");
        _subscription.Dispose();
        this.Trace("done");
    }

    /// <summary>
    /// Ensures the scheduler has not been disposed
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the scheduler is already disposed</exception>
    private void EnsureNotDisposed()
    {
        if (Volatile.Read(ref _isDisposed))
            throw new InvalidOperationException("Log scheduler is already disposed");
    }
}
