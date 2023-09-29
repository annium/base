using System;
using System.Collections.Generic;

namespace Annium.Logging.Shared.Internal;

internal class LogSentry<TContext> : ILogSentry<TContext>
    where TContext : class, ILogContext
{
    private readonly IList<LogMessage<TContext>> _messagesBuffer = new List<LogMessage<TContext>>();
    private Action<LogMessage<TContext>> _handler;
    private bool _isHandlerSet;

    public LogSentry(
    )
    {
        _handler = _messagesBuffer.Add;
    }

    public void Register(LogMessage<TContext> message) => _handler(message);

    public void SetHandler(Action<LogMessage<TContext>> handler)
    {
        if (_isHandlerSet)
            throw new InvalidOperationException("Handler is intended to be set once");

        _isHandlerSet = true;
        _handler = handler;

        foreach (var message in _messagesBuffer)
            _handler(message);
    }
}