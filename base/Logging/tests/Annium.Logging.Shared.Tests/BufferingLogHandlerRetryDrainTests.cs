using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared.Tests.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests;

/// <summary>
/// Tests for <see cref="BufferingLogHandler{TContext}"/> retry/drain logic:
/// a failed first send buffers events, and a subsequent successful send drains them in order;
/// a handler that always fails never loses events across calls.
/// </summary>
public class BufferingLogHandlerRetryDrainTests
{
    /// <summary>
    /// First HandleAsync call — SendEventsAsync returns false — buffers the batch.
    /// Second HandleAsync call — SendEventsAsync returns true — delivers all events
    /// (first batch then second batch, in order) to the sink.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_FirstCallFails_SecondCallDrainsBothBatchesInOrder()
    {
        var sink = new FailThenSucceedSink(failFirstCall: true);
        var msgA = LoggingTestHelpers.BuildMessage(1);
        var msgB = LoggingTestHelpers.BuildMessage(2);

        // first call: SendEventsAsync returns false → events buffered, not yet delivered
        await sink.HandleAsync([msgA], CancellationToken.None);

        sink.Delivered.IsEmpty();

        // second call: SendEventsAsync returns true for msgB (the new batch) first, then drains
        // the buffer and delivers msgA. The canonical order is: new batch first, then buffer drain.
        await sink.HandleAsync([msgB], CancellationToken.None);

        // both events must be delivered: the new batch (msgB) first, then the retry-buffer (msgA)
        sink.Delivered.Has(2);
        sink.Delivered.At(0).Is(msgB);
        sink.Delivered.At(1).Is(msgA);
    }

    /// <summary>
    /// A handler that always fails (SendEventsAsync always returns false) must never lose
    /// events — each HandleAsync call accumulates them in the internal buffer.
    /// A later successful call must flush every buffered event.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_AlwaysFails_NeverLosesEvents_LaterSuccessFlushesAll()
    {
        var sink = new SmallBufferCountSink(failCount: 3);
        var messages = Enumerable.Range(0, 3).Select(LoggingTestHelpers.BuildMessage).ToArray();

        // three failing calls — all events buffered
        foreach (var msg in messages)
            await sink.HandleAsync([msg], CancellationToken.None);

        sink.Delivered.IsEmpty();

        // fourth call succeeds — must drain all 3 buffered messages plus deliver the 4th
        var lastMsg = LoggingTestHelpers.BuildMessage(99);
        await sink.HandleAsync([lastMsg], CancellationToken.None);

        sink.Delivered.Has(4);

        // the new (triggering) batch is sent first, then the buffered messages are drained in
        // their original registration order — canonical order: new batch at index 0, buffer after.
        sink.Delivered.At(0).Is(lastMsg);
        for (var i = 0; i < 3; i++)
            sink.Delivered.At(i + 1).Is(messages[i]);
    }

    /// <summary>
    /// The drain loop in <see cref="BufferingLogHandler{TContext}"/> slices the retry buffer
    /// into chunks of at most <c>BufferCount</c> per iteration, using <c>continue</c> to keep
    /// looping until the buffer is empty. This test forces MORE messages into the buffer than
    /// <c>BufferCount</c> (5 buffered vs BufferCount=2), so the drain requires 3 iterations
    /// (slices of 2 + 2 + 1). If the <c>continue</c> were replaced with <c>break</c>, only the
    /// first slice of 2 would drain and the remaining 3 messages would be silently lost.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_LargeBuffer_SmallBufferCount_DrainsAllSlices()
    {
        // BufferCount=2 → each drain iteration handles at most 2 buffered messages.
        // We accumulate 5 messages in the buffer by failing 5 single-message calls.
        const int bufferCount = 2;
        const int failCount = 5;
        var sink = new SmallBufferCountSink(failCount: failCount, bufferCount: bufferCount);
        var bufferedMessages = Enumerable.Range(0, failCount).Select(LoggingTestHelpers.BuildMessage).ToArray();

        // fail all 5 individual calls — each one-message batch is buffered
        foreach (var msg in bufferedMessages)
            await sink.HandleAsync([msg], CancellationToken.None);

        sink.Delivered.IsEmpty();

        // trigger a successful call — the drain loop must iterate ceil(5/2) = 3 times to drain all
        var triggerMsg = LoggingTestHelpers.BuildMessage(99);
        await sink.HandleAsync([triggerMsg], CancellationToken.None);

        // trigger batch (1) + all 5 buffered = 6 total
        sink.Delivered.Has(failCount + 1);

        // trigger batch is sent first (canonical order)
        sink.Delivered.At(0).Is(triggerMsg);

        // buffered messages follow in their original registration order
        for (var i = 0; i < failCount; i++)
            sink.Delivered.At(i + 1).Is(bufferedMessages[i]);
    }

    /// <summary>
    /// Concrete <see cref="BufferingLogHandler{TContext}"/> that fails the very first
    /// <see cref="SendEventsAsync"/> call and succeeds on all subsequent calls.
    /// Accumulates all successfully-sent messages into <see cref="Delivered"/>.
    /// </summary>
    private sealed class FailThenSucceedSink : BufferingLogHandler<DefaultLogContext>
    {
        /// <summary>All events that have been successfully delivered by SendEventsAsync.</summary>
        public List<LogMessage<DefaultLogContext>> Delivered { get; } = new();

        /// <summary>Number of <see cref="SendEventsAsync"/> calls made so far.</summary>
        private int _callCount;

        /// <summary>Whether the very first <see cref="SendEventsAsync"/> call should be forced to fail.</summary>
        private readonly bool _failFirstCall;

        /// <summary>
        /// Initializes a new instance of the <see cref="FailThenSucceedSink"/> class.
        /// </summary>
        /// <param name="failFirstCall">Whether the first handle call throws before succeeding.</param>
        public FailThenSucceedSink(bool failFirstCall)
            : base(new LogRouteConfiguration { BufferTime = TimeSpan.FromMilliseconds(100), BufferCount = 100 })
        {
            _failFirstCall = failFirstCall;
        }

        /// <summary>
        /// Records the batch and fails the first send when <c>_failFirstCall</c> is set, then succeeds
        /// on all subsequent calls — to exercise the retry-drain path.
        /// </summary>
        /// <param name="events">The batch of log messages to send.</param>
        /// <returns>
        /// <c>false</c> on the very first call when <c>_failFirstCall</c> is <c>true</c> (events are buffered);
        /// <c>true</c> on every subsequent call (events are delivered and appended to <see cref="Delivered"/>).
        /// </returns>
        protected override ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<DefaultLogContext>> events)
        {
            var callIndex = Interlocked.Increment(ref _callCount);
            if (_failFirstCall && callIndex == 1)
                return new(false);

            foreach (var e in events)
                Delivered.Add(e);

            return new(true);
        }
    }

    /// <summary>
    /// Concrete <see cref="BufferingLogHandler{TContext}"/> that fails the first <c>failCount</c>
    /// <see cref="SendEventsAsync"/> calls and succeeds afterwards. Optionally configured with a
    /// deliberately small <c>bufferCount</c> to force multiple drain iterations.
    /// Accumulates all successfully-sent messages into <see cref="Delivered"/>.
    /// </summary>
    private sealed class SmallBufferCountSink : BufferingLogHandler<DefaultLogContext>
    {
        /// <summary>All events that have been successfully delivered by SendEventsAsync.</summary>
        public List<LogMessage<DefaultLogContext>> Delivered { get; } = new();

        /// <summary>Number of <see cref="SendEventsAsync"/> calls made so far.</summary>
        private int _callCount;

        /// <summary>Number of leading <see cref="SendEventsAsync"/> calls that are forced to fail before success.</summary>
        private readonly int _failCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmallBufferCountSink"/> class.
        /// </summary>
        /// <param name="failCount">Number of leading handle calls that throw.</param>
        /// <param name="bufferCount">Buffer size the underlying route is configured with.</param>
        public SmallBufferCountSink(int failCount, int bufferCount = 100)
            : base(new LogRouteConfiguration { BufferTime = TimeSpan.FromMilliseconds(100), BufferCount = bufferCount })
        {
            _failCount = failCount;
        }

        /// <summary>
        /// Records the batch and fails the first <c>_failCount</c> sends, then succeeds on all subsequent
        /// calls — to exercise the retry-drain path across multiple buffered batches.
        /// </summary>
        /// <param name="events">The batch of log messages to send.</param>
        /// <returns>
        /// <c>false</c> for the first <c>_failCount</c> calls (events are buffered);
        /// <c>true</c> once the call count exceeds <c>_failCount</c> (events are delivered and appended to <see cref="Delivered"/>).
        /// </returns>
        protected override ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<DefaultLogContext>> events)
        {
            var callIndex = Interlocked.Increment(ref _callCount);
            if (callIndex <= _failCount)
                return new(false);

            foreach (var e in events)
                Delivered.Add(e);

            return new(true);
        }
    }
}
