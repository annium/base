using System;
using System.Collections.Generic;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Log sentry that buffers messages until a handler is set, then forwards them
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class LogSentry<TContext> : ILogSentry<TContext>
    where TContext : class
{
    /// <summary>
    /// Buffer for storing log messages before handler is set
    /// </summary>
    private readonly IList<LogMessage<TContext>> _messagesBuffer = new List<LogMessage<TContext>>();

    /// <summary>
    /// The current message handler
    /// </summary>
    private Action<LogMessage<TContext>> _handler;

    /// <summary>
    /// Indicates whether the handler has been set
    /// </summary>
    private bool _isHandlerSet;

    public LogSentry()
    {
        _handler = _messagesBuffer.Add;
    }

    /// <summary>
    /// Registers a log message with the sentry
    /// </summary>
    /// <param name="message">The log message to register</param>
    public void Register(LogMessage<TContext> message) => _handler(message);

    /// <summary>
    /// Sets the message handler and flushes any buffered messages
    /// </summary>
    /// <param name="handler">The handler to set</param>
    /// <exception cref="InvalidOperationException">Thrown if handler is already set</exception>
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
