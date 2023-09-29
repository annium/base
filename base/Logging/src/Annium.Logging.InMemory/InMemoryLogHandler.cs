using System.Collections.Generic;
using Annium.Logging.Shared;

namespace Annium.Logging.InMemory;

public class InMemoryLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class, ILogContext
{
    public IReadOnlyList<LogMessage<TContext>> Logs => _logs;

    private readonly List<LogMessage<TContext>> _logs = new();

    public void Handle(LogMessage<TContext> message)
    {
        _logs.Add(message);
    }
}