using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared.Internal;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Tests for <see cref="BackgroundLogScheduler{TContext}"/> verifying the DisposeAsync
/// canonical order — specifically that the scheduler awaits drain of the full sink pipeline
/// before DisposeAsync returns, so that slow sinks never have queued batches dropped.
/// </summary>
public class BackgroundLogSchedulerTests
{
    /// <summary>
    /// With a sink that sleeps 500ms per batch, queue 5 batches and assert that DisposeAsync
    /// only returns after all 5 have been handled.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_WithSlowSink_AwaitsFinalBatch()
    {
        // arrange — one message per batch; sink sleeps 500ms per call
        const int batchCount = 5;
        var sink = new SlowSink(TimeSpan.FromMilliseconds(500));
        var config = new LogRouteConfiguration
        {
            // force one message per batch — minimal buffering
            BufferTime = TimeSpan.FromMilliseconds(10),
            BufferCount = 1,
        };
        var scheduler = new BackgroundLogScheduler<DefaultLogContext>(_ => true, sink, config);

        // enqueue batches — must yield briefly so the buffer flushes individual items
        for (var i = 0; i < batchCount; i++)
        {
            scheduler.Handle(BuildMessage(i));
            // small wait so Buffer operator emits this item in its own batch (BufferCount=1
            // means "flush after 1 item" — the emission still happens asynchronously)
            await Task.Delay(15, TestContext.Current.CancellationToken);
        }

        // act — dispose
        await scheduler.DisposeAsync();

        // assert — every batch was delivered
        sink.BatchCount.Is(batchCount);
    }

    /// <summary>
    /// Constructs a synthetic log message for scheduler plumbing.
    /// </summary>
    /// <param name="seq">Sequence number</param>
    /// <returns>Log message instance</returns>
    private static LogMessage<DefaultLogContext> BuildMessage(int seq) =>
        new(
            new DefaultLogContext(),
            Instant.FromUnixTimeTicks(seq),
            "test",
            "id",
            LogLevel.Info,
            0,
            $"msg-{seq}",
            null,
            string.Empty,
            new Dictionary<string, object?>(),
            "type",
            "member",
            0
        );

    /// <summary>
    /// Deliberately slow async log handler — each batch call blocks for a fixed duration.
    /// Thread-safe batch counter for assertion.
    /// </summary>
    private sealed class SlowSink(TimeSpan perBatch) : ILogHandler<DefaultLogContext>
    {
        private int _batchCount;

        public int BatchCount => Volatile.Read(ref _batchCount);

        public async ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct)
        {
            // intentionally ignore CT — the test asserts that DisposeAsync waits for the
            // sink to finish naturally even after the observable CT is cancelled
            await Task.Delay(perBatch);
            Interlocked.Increment(ref _batchCount);
        }
    }
}
