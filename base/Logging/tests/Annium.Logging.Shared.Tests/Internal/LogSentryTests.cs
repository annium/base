using System;
using System.Collections.Generic;
using Annium.Logging.Shared.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Tests for <see cref="LogSentry{TContext}"/>:
/// messages registered before SetHandler are flushed on first SetHandler call,
/// and a second SetHandler throws <see cref="InvalidOperationException"/>.
/// </summary>
public class LogSentryTests
{
    /// <summary>
    /// Messages registered before SetHandler is called are buffered internally; the buffer
    /// is empty from the caller's perspective until SetHandler fires.
    /// </summary>
    [Fact]
    public void Register_BeforeSetHandler_BuffersMessages()
    {
        var sentry = new LogSentry<DefaultLogContext>();
        var delivered = new List<LogMessage<DefaultLogContext>>();

        var msg = LoggingTestHelpers.BuildMessage(1);
        sentry.Register(msg);

        // nothing delivered yet — handler not set
        delivered.IsEmpty();
    }

    /// <summary>
    /// When SetHandler is called, all previously buffered messages are flushed to the handler
    /// in their original registration order.
    /// </summary>
    [Fact]
    public void SetHandler_FlushesBufferedMessages_InOrder()
    {
        var sentry = new LogSentry<DefaultLogContext>();
        var delivered = new List<LogMessage<DefaultLogContext>>();

        var msgA = LoggingTestHelpers.BuildMessage(1);
        var msgB = LoggingTestHelpers.BuildMessage(2);
        sentry.Register(msgA);
        sentry.Register(msgB);

        sentry.SetHandler(delivered.Add);

        delivered.Has(2);
        delivered.At(0).Is(msgA);
        delivered.At(1).Is(msgB);
    }

    /// <summary>
    /// After SetHandler is called, subsequent Register calls are forwarded to the handler
    /// directly (no buffering).
    /// </summary>
    [Fact]
    public void Register_AfterSetHandler_ForwardsDirectlyToHandler()
    {
        var sentry = new LogSentry<DefaultLogContext>();
        var delivered = new List<LogMessage<DefaultLogContext>>();
        sentry.SetHandler(delivered.Add);

        var msg = LoggingTestHelpers.BuildMessage(1);
        sentry.Register(msg);

        delivered.Has(1);
        delivered.At(0).Is(msg);
    }

    /// <summary>
    /// A second SetHandler call on the same sentry must throw
    /// <see cref="InvalidOperationException"/> with the canonical message.
    /// </summary>
    [Fact]
    public void SetHandler_CalledTwice_ThrowsInvalidOperationException()
    {
        var sentry = new LogSentry<DefaultLogContext>();
        sentry.SetHandler(_ => { });

        Wrap.It(() => sentry.SetHandler(_ => { })).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Buffer is cleared after SetHandler flushes it — subsequent Register calls after
    /// SetHandler must not re-deliver the originally buffered messages.
    /// </summary>
    [Fact]
    public void SetHandler_ClearsBufferAfterFlush()
    {
        var sentry = new LogSentry<DefaultLogContext>();
        var delivered = new List<LogMessage<DefaultLogContext>>();

        sentry.Register(LoggingTestHelpers.BuildMessage(1));
        sentry.SetHandler(delivered.Add);

        // delivered the buffered message
        delivered.Has(1);

        // new message registered after SetHandler must be delivered once only
        sentry.Register(LoggingTestHelpers.BuildMessage(2));

        delivered.Has(2);
    }
}
