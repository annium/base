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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// <see cref="LogRouteConfiguration.BufferTime"/> equal to <see cref="TimeSpan.Zero"/>
    /// is degenerate (buffer-by-count-only is invalid). The constructor must reject it.
    /// </summary>
    [Fact]
    public void Ctor_BufferTimeZero_Throws()
    {
        var config = new LogRouteConfiguration { BufferTime = TimeSpan.Zero, BufferCount = 1 };

        Wrap.It(() => new BackgroundLogScheduler<DefaultLogContext>(_ => true, new NoOpSink(), config))
            .Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Negative <see cref="LogRouteConfiguration.BufferTime"/> remains invalid after the
    /// guard tightening (originally <c>&lt; Zero</c>; now <c>&lt;= Zero</c>).
    /// </summary>
    [Fact]
    public void Ctor_BufferTimeNegative_Throws()
    {
        var config = new LogRouteConfiguration { BufferTime = TimeSpan.FromMilliseconds(-1), BufferCount = 1 };

        Wrap.It(() => new BackgroundLogScheduler<DefaultLogContext>(_ => true, new NoOpSink(), config))
            .Throws<ArgumentOutOfRangeException>();
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
        /// <summary>Underlying counter incremented after each batch completes; read via <see cref="BatchCount"/>.</summary>
        private int _batchCount;

        /// <summary>Thread-safe snapshot of the number of batches processed so far.</summary>
        public int BatchCount => Volatile.Read(ref _batchCount);

        /// <summary>
        /// Sleeps for <c>perBatch</c> to simulate a slow sink, then increments the batch counter.
        /// Intentionally ignores <paramref name="ct"/> to verify that <c>DisposeAsync</c> waits
        /// for natural completion.
        /// </summary>
        /// <param name="messages">The batch of log messages to handle.</param>
        /// <param name="ct">Cancellation token (intentionally ignored by this stub).</param>
        /// <returns>A value task that completes after the simulated delay.</returns>
        public async ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct)
        {
            // intentionally ignore CT — the test asserts that DisposeAsync waits for the
            // sink to finish naturally even after the observable CT is cancelled
            await Task.Delay(perBatch);
            Interlocked.Increment(ref _batchCount);
        }
    }

    /// <summary>
    /// Minimal handler that ignores incoming batches — used by ctor-guard tests where the
    /// scheduler is expected to fail before any handler call is made.
    /// </summary>
    private sealed class NoOpSink : ILogHandler<DefaultLogContext>
    {
        /// <summary>
        /// Immediately completes without processing the batch — used by constructor-guard tests
        /// where the scheduler is expected to fail before any handler call is made.
        /// </summary>
        /// <param name="messages">The batch of log messages (ignored).</param>
        /// <param name="ct">Cancellation token (ignored).</param>
        /// <returns>A completed value task.</returns>
        public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct) =>
            ValueTask.CompletedTask;
    }
}
