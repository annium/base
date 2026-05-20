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
        var whenEmpty = channel.Reader.WhenEmptyAsync(delay: 10, ct: TestContext.Current.CancellationToken);
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
        var whenEmpty = channel.Reader.WhenEmptyAsync(delay: 10, ct: TestContext.Current.CancellationToken);

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
}
