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
    /// Atomic dispose guard: <c>0</c> until the first <see cref="DisposeAsync"/> wins the
    /// <see cref="Interlocked.Exchange(ref int, int)"/>, then <c>1</c>. Makes disposal idempotent even
    /// under a concurrent double-dispose (a scheduler can be reached by more than one OnDisposed
    /// subscriber when AddLogging is called multiple times on the same container).
    /// </summary>
    private int _disposeGuard;

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

    /// <summary>
    /// The handler this scheduler dispatches batches to. Retained so it can be disposed when the
    /// scheduler is disposed (the handler may own resources, e.g. <c>FileLogHandler</c>'s gate).
    /// </summary>
    private readonly ILogHandler<TContext> _handler;

    /// <summary>
    /// Completion source signalled when the sink pipeline (Buffer → DoSequentialAsync → handler)
    /// has fully drained. <see cref="DisposeAsync"/> awaits this before disposing the subscription,
    /// so a slow sink never has queued batches dropped on dispose.
    /// </summary>
    private readonly TaskCompletionSource _pipelineDrained = new();

    public BackgroundLogScheduler(
        Func<LogMessage<TContext>, bool> filter,
        ILogHandler<TContext> handler,
        LogRouteConfiguration configuration
    )
    {
        if (configuration.BufferTime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(configuration.BufferTime),
                "Buffer time is expected to be positive"
            );

        if (configuration.BufferCount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(configuration.BufferCount),
                "Buffer count is expected to be positive"
            );

        Logger = VoidLogger.Instance;
        Filter = filter;
        _handler = handler;

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
            .DoSequentialAsync(async x => await handler.HandleAsync(x.AsReadOnly(), _observableCts.Token))
            // onError unblocks the drain (rather than the default OnErrorNotImplementedException): a
            // pipeline fault during teardown must still complete _pipelineDrained so DisposeAsync never
            // hangs. Disposal robustness is prioritized over surfacing a terminal teardown error.
            .Subscribe(_ => { }, _ => _pipelineDrained.TrySetResult(), () => _pipelineDrained.TrySetResult());
    }

    /// <summary>
    /// Handles a log message by queuing it for background processing
    /// </summary>
    /// <param name="message">The log message to handle</param>
    public void Handle(LogMessage<TContext> message)
    {
        EnsureNotDisposed();

        lock (_messageWriter)
        {
            // re-check under the lock: DisposeAsync sets _isDisposed and then completes the writer under
            // this same lock, so a Handle racing disposal could otherwise pass the outer check, find the
            // writer already completed, and throw the misleading "must have been written" error. Surface
            // the disposed state with the consistent exception instead.
            EnsureNotDisposed();
            if (!_messageWriter.TryWrite(message))
                throw new InvalidOperationException("Message must have been written to channel");
        }
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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ChannelClosedException)
            {
                break;
            }
        }

        // shutdown mode - handle only left tasks
        this.Trace("handle {count} messages left", Count);
        while (_messageReader.TryRead(out var message))
            ctx.OnNext(message);

        this.Trace("done");

        return () => Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the scheduler and completes any remaining log processing — canonical order
    /// per §8.2.17: stop accepting writes → drain channel → stop observable → await pipeline
    /// drain (so slow sinks finish their queued batches) → dispose subscription.
    /// </summary>
    /// <returns>A task that represents the disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");
        // idempotent (atomic): a scheduler may be reached by more than one OnDisposed subscriber when
        // AddLogging is called multiple times on the same container (the schedulers live in a shared
        // singleton list), so a second dispose — even a concurrent one — must be a no-op rather than
        // throw or double-dispose.
        if (Interlocked.Exchange(ref _disposeGuard, 1) != 0)
        {
            this.Trace("already disposed, skip");
            return;
        }

        Volatile.Write(ref _isDisposed, true);
        lock (_messageWriter)
            _messageWriter.Complete();
        this.Trace("wait for reader completion");
        // awaiting our own channel's completion, not a foreign task — VSTHRD003 false positive
#pragma warning disable VSTHRD003
        await _messageReader.Completion;
#pragma warning restore VSTHRD003
        this.Trace("cancel observable cts");
        await _observableCts.CancelAsync();
        this.Trace("await observable");
        await _observable.WhenCompletedAsync(Logger);
        this.Trace("await pipeline drain");
        // awaiting our own pipeline-drain completion source, not a foreign task — VSTHRD003 false positive
#pragma warning disable VSTHRD003
        await _pipelineDrained.Task;
#pragma warning restore VSTHRD003
        this.Trace("dispose subscription");
        _subscription.Dispose();
        _observableCts.Dispose();

        // dispose the owned handler last — after the pipeline has drained, so no in-flight batch
        // touches a handler resource (e.g. FileLogHandler's gate) after it is released.
        switch (_handler)
        {
            case IAsyncDisposable ad:
                await ad.DisposeAsync();
                break;
            case IDisposable d:
                // VSTHRD103: fallback for a handler that is only IDisposable (e.g. FileLogHandler's
                // gate); async disposal is handled by the IAsyncDisposable arm above, so Dispose() is correct.
#pragma warning disable VSTHRD103
                d.Dispose();
#pragma warning restore VSTHRD103
                break;
        }

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
