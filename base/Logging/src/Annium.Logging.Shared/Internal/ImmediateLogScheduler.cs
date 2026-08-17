using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Immediate log scheduler that processes log messages synchronously without queueing.
/// Calls the handler with one-message batches via the canonical <see cref="ILogHandler{TContext}"/>
/// contract; sync sinks return <see cref="System.Threading.Tasks.ValueTask.CompletedTask"/> so this is
/// effectively non-blocking. Forcing a buffering handler into immediate mode (rare, via the
/// <c>WithImmediateScheduler()</c> override) blocks the caller until the buffering handler completes.
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class ImmediateLogScheduler<TContext> : ILogScheduler<TContext>, IAsyncDisposable
    where TContext : class
{
    /// <summary>
    /// Gets the filter function for determining which messages to process
    /// </summary>
    public Func<LogMessage<TContext>, bool> Filter { get; }

    /// <summary>
    /// The log handler for processing messages
    /// </summary>
    private readonly ILogHandler<TContext> _handler;

    /// <summary>
    /// Dispose guard: <c>0</c> until the first <see cref="DisposeAsync"/>, then <c>1</c>. Makes
    /// disposal idempotent (a scheduler can be reached by more than one OnDisposed subscriber when
    /// AddLogging is called multiple times on the same container).
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmediateLogScheduler{TContext}"/> class.
    /// </summary>
    /// <param name="filter">Predicate deciding whether a message is accepted by this route.</param>
    /// <param name="handler">Handler each message is dispatched to synchronously.</param>
    public ImmediateLogScheduler(Func<LogMessage<TContext>, bool> filter, ILogHandler<TContext> handler)
    {
        Filter = filter;
        _handler = handler;
    }

    /// <summary>
    /// Handles a log message immediately by dispatching a one-element batch to the handler.
    /// </summary>
    /// <param name="message">The log message to handle</param>
    public void Handle(LogMessage<TContext> message)
    {
        // Sync sinks return ValueTask.CompletedTask, so this is non-blocking. A buffering handler
        // forced into immediate mode (via .WithImmediateScheduler()) would block — that's the
        // documented contract of the override.
#pragma warning disable VSTHRD002
        _handler.HandleAsync(new[] { message }, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }

    /// <summary>
    /// Disposes the owned handler if it holds resources. Invoked by the container's <c>OnDisposed</c>
    /// callback when logging is torn down.
    /// </summary>
    /// <returns>A task that completes once the handler is disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

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
    }
}
