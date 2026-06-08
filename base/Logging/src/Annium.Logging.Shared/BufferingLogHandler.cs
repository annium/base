using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Logging.Shared;

/// <summary>
/// Abstract base class for buffering log handlers that queue messages when sending fails.
/// Implements the canonical <see cref="ILogHandler{TContext}"/> contract; concrete subclasses
/// override <see cref="SendEventsAsync"/> to deliver to their target.
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public abstract class BufferingLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// The log route configuration
    /// </summary>
    private readonly LogRouteConfiguration _cfg;

    /// <summary>
    /// Buffer for queuing failed log messages
    /// </summary>
    private readonly Queue<LogMessage<TContext>> _eventsBuffer = new();

    /// <summary>
    /// Initializes a new instance of the BufferingLogHandler class
    /// </summary>
    /// <param name="cfg">The log route configuration</param>
    protected BufferingLogHandler(LogRouteConfiguration cfg)
    {
        _cfg = cfg;
    }

    /// <summary>
    /// Handles log messages with buffering support for failed sends.
    /// </summary>
    /// <param name="messages">The log messages to handle</param>
    /// <param name="ct">Cancellation token signalled when the scheduler is shutting down</param>
    /// <returns>A task representing the handling operation</returns>
    public async ValueTask HandleAsync(IReadOnlyList<LogMessage<TContext>> messages, CancellationToken ct)
    {
        var events = new List<LogMessage<TContext>>(messages);

        // if failed to send events - add them to buffer
        if (!await SendEventsAsync(events))
        {
            BufferEvents(events);
            return;
        }

        // Drain the retry buffer regardless of ct: the loop is bounded (it breaks on an empty buffer or
        // a repeat send failure), and gating it on ct would drop already-buffered events on shutdown —
        // BackgroundLogScheduler cancels its token before the final drain, so a ct gate here loses the
        // failure-buffer exactly when we most want it flushed.
        while (true)
        {
            // pick slice to send
            lock (_eventsBuffer)
            {
                if (_eventsBuffer.Count == 0)
                    break;

                events.Clear();

                var count = Math.Min(_eventsBuffer.Count, _cfg.BufferCount);
                for (var i = 0; i < count; i++)
                    events.Add(_eventsBuffer.Dequeue());
            }

            // if sent successfully - go to next slice
            if (await SendEventsAsync(events))
                continue;

            // if failed to send - move events back to buffer and break sending
            BufferEvents(events);
            break;
        }
    }

    /// <summary>
    /// Sends log events to the target destination
    /// </summary>
    /// <param name="events">The events to send</param>
    /// <returns>True if sending was successful, false otherwise</returns>
    protected abstract ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<TContext>> events);

    /// <summary>
    /// Buffers events for retry when sending fails
    /// </summary>
    /// <param name="events">The events to buffer</param>
    private void BufferEvents(IReadOnlyCollection<LogMessage<TContext>> events)
    {
        lock (_eventsBuffer)
            foreach (var e in events)
                _eventsBuffer.Enqueue(e);
    }
}
