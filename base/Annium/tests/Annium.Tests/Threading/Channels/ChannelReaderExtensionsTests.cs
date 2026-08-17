using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading.Channels;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Tests.Threading.Channels;

/// <summary>
/// Contains unit tests for <see cref="ChannelReaderExtensions"/> to verify channel piping behavior.
/// </summary>
public class ChannelReaderExtensionsTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelReaderExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ChannelReaderExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that data can be piped from one channel to another using the Pipe extension method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Pipe()
    {
        this.Trace("start");

        // arrange
        var dataSize = 100_000;
        var data = Enumerable.Range(0, dataSize).ToArray();
        var source = Channel.CreateUnbounded<int>();
        var target = Channel.CreateUnbounded<int>();

        this.Trace("write to source channel writer");
        Observable.Range(0, dataSize).WriteToChannel(source.Writer, CancellationToken.None);
        var log = new TestLog<int>();

        this.Trace("create observable from target channel reader");
        using var observable = target.Reader.AsObservable().Subscribe(log.Add);

        // act
        this.Trace("pipe");
        await using var pipe = source.Reader.Pipe(target.Writer, Logger);

        // assert
        this.Trace("assert log is complete");
        await Expect.ToAsync(() => log.Has(data.Length));

        this.Trace("assert log matches data and dispose callback is not called");
        log.SequenceEqual(data).IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that Read throws InvalidOperationException when called on an empty channel.
    /// </summary>
    [Fact]
    public void Read_EmptyChannel_ThrowsInvalidOperationException()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();

        // act & assert
        Wrap.It(() => channel.Reader.Read()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that Read returns the item written to the channel.
    /// </summary>
    [Fact]
    public void Read_ChannelWithItem_ReturnsItem()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();
        channel.Writer.TryWrite(42);

        // act
        var result = channel.Reader.Read();

        // assert
        result.Is(42);
    }

    /// <summary>
    /// Verifies that WhenEmptyAsync completes promptly when the channel is already empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenEmptyAsync_AlreadyEmptyChannel_ReturnsImmediately()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();

        // act — bounded wait of 100 ms; if WhenEmptyAsync hangs the Wait.UntilAsync will time out
        var whenEmpty = channel.Reader.WhenEmptyAsync(delay: 10, ct: TestContext.Current.CancellationToken).AsTask();
        await Wait.UntilAsync(() => whenEmpty.IsCompleted, TestContext.Current.CancellationToken);

        // assert
        whenEmpty.IsCompleted.IsTrue();
    }

    /// <summary>
    /// Verifies that WhenEmptyAsync waits until all items have been drained from the channel.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenEmptyAsync_NonEmptyChannel_WaitsUntilDrained()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();
        channel.Writer.TryWrite(1);
        channel.Writer.TryWrite(2);
        channel.Writer.TryWrite(3);

        // act — start the wait task before reading
        var whenEmpty = channel.Reader.WhenEmptyAsync(delay: 10, ct: TestContext.Current.CancellationToken).AsTask();

        // assert — task must NOT be complete while items remain
        whenEmpty.IsCompleted.IsFalse();

        // drain items one by one
        channel.Reader.Read();
        channel.Reader.Read();
        channel.Reader.Read();

        // wait for WhenEmptyAsync to notice the channel is empty
        await Wait.UntilAsync(() => whenEmpty.IsCompleted, TestContext.Current.CancellationToken);

        // assert — task completes after all items are consumed
        whenEmpty.IsCompleted.IsTrue();
    }

    /// <summary>
    /// Verifies that when a writer's TryWrite throws an unexpected exception (not OperationCanceledException
    /// or ChannelClosedException), the Pipe catches it via the general Exception branch, logs it at Error
    /// level, and the pipe can then be disposed cleanly without throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Pipe_WriterThrowsUnexpectedException_LogsErrorAndDisposesCleanly()
    {
        this.Trace("start");

        // arrange — source channel with one item ready to be read
        var source = Channel.CreateUnbounded<int>();
        source.Writer.TryWrite(42);

        // A writer whose TryWrite always throws an unexpected exception to hit the general catch branch.
        var throwingWriter = new ThrowingChannelWriter<int>(new InvalidOperationException("writer-boom"));

        // act — create the pipe; the loop will read the item and call throwingWriter.Write which throws
        var pipe = source.Reader.Pipe(throwingWriter, Logger);

        // wait for the error-log entry to appear (the exception is bridged to the logger)
        await Expect.ToAsync(
            () =>
                Logs.Any(m => m.Level == LogLevel.Error && m.Exception != null && m.Exception.Message == "writer-boom")
                    .IsTrue(),
            TestContext.Current.CancellationToken
        );

        // dispose must complete cleanly — no exception should propagate out
        await pipe.DisposeAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that WhenEmptyAsync completes without throwing when the cancellation token is cancelled
    /// while the channel still has items (the OperationCanceledException is swallowed by the catch guard).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenEmptyAsync_CancelledWhilePolling_CompletesWithoutThrowing()
    {
        // arrange — channel with items so the polling loop keeps spinning
        var channel = Channel.CreateUnbounded<int>();
        channel.Writer.TryWrite(1);
        channel.Writer.TryWrite(2);
        channel.Writer.TryWrite(3);

        using var cts = new CancellationTokenSource();

        // act — start WhenEmptyAsync with our own CTS token (NOT the runner token)
        var whenEmpty = channel.Reader.WhenEmptyAsync(delay: 10, ct: cts.Token).AsTask();

        // cancel immediately; items are still in the channel so the loop is inside the delay
        await cts.CancelAsync();

        // assert — task must complete (OCE is swallowed) and must NOT propagate any exception
        await whenEmpty;
        whenEmpty.IsCompleted.IsTrue();
        whenEmpty.IsFaulted.IsFalse();
        whenEmpty.IsCanceled.IsFalse();
    }

    /// <summary>
    /// ChannelWriter that throws a caller-supplied exception from TryWrite. Used to exercise
    /// the general-exception branch in <see cref="ChannelReaderExtensions.Pipe{T}"/>.
    /// </summary>
    private sealed class ThrowingChannelWriter<T> : ChannelWriter<T>
    {
        /// <summary>The exception instance thrown by every write attempt.</summary>
        private readonly Exception _ex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingChannelWriter{T}"/> class.
        /// </summary>
        /// <param name="ex">Exception thrown by every write attempt.</param>
        public ThrowingChannelWriter(Exception ex)
        {
            _ex = ex;
        }

        /// <summary>
        /// Always throws the configured exception to simulate an unexpected write failure.
        /// </summary>
        /// <param name="item">The item that would be written (ignored — an exception is always thrown).</param>
        /// <returns>This method never returns; it always throws.</returns>
        public override bool TryWrite(T item) => throw _ex;

        /// <summary>
        /// Returns a completed ValueTask so WaitToWriteAsync never blocks indefinitely.
        /// </summary>
        /// <param name="cancellationToken">Token to observe for cancellation (ignored in this stub).</param>
        /// <returns>A completed <see cref="ValueTask{Boolean}"/> with <c>true</c>.</returns>
        public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);
    }
}
