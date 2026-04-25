using System;
using System.Threading;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Immediate log scheduler that processes log messages synchronously without queueing.
/// Calls the handler with one-message batches via the canonical <see cref="ILogHandler{TContext}"/>
/// contract; sync sinks return <see cref="System.Threading.Tasks.ValueTask.CompletedTask"/> so this is
/// effectively non-blocking. Forcing a buffering handler into immediate mode (rare, via the
/// <c>WithImmediateScheduler()</c> override) blocks the caller until the buffering handler completes.
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class ImmediateLogScheduler<TContext> : ILogScheduler<TContext>
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
}
