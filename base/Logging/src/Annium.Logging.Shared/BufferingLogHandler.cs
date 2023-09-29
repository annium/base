using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Logging.Shared;

public abstract class BufferingLogHandler<TContext> : IAsyncLogHandler<TContext>
    where TContext : class, ILogContext
{
    private readonly LogRouteConfiguration _cfg;
    private readonly Queue<LogMessage<TContext>> _eventsBuffer = new();

    protected BufferingLogHandler(
        LogRouteConfiguration cfg
    )
    {
        _cfg = cfg;
    }

    public async ValueTask Handle(IReadOnlyCollection<LogMessage<TContext>> messages)
    {
        var events = new List<LogMessage<TContext>>(messages);

        // if failed to send events - add them to buffer
        if (!await SendEventsAsync(events))
        {
            BufferEvents(events);
            return;
        }

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

    protected abstract ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<TContext>> events);

    private void BufferEvents(IReadOnlyCollection<LogMessage<TContext>> events)
    {
        lock (_eventsBuffer)
            foreach (var e in events)
                _eventsBuffer.Enqueue(e);
    }
}