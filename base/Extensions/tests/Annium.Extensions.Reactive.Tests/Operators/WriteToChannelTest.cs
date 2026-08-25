using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for WriteToChannel, the bridge from an observable into a channel. Anything that goes wrong here
/// shows up at the far end as a consumer that reads nothing, with no other signal.
/// </summary>
public class WriteToChannelTest
{
    /// <summary>
    /// Values emitted after subscribing reach the channel, in order.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task WriteToChannel_Emitted_ReachTheReader()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();
        var subject = new Subject<int>();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        subject.WriteToChannel(channel.Writer, cts.Token);

        // act
        subject.OnNext(1);
        subject.OnNext(2);

        // assert
        (await channel.Reader.ReadAsync(TestContext.Current.CancellationToken)).Is(1);
        (await channel.Reader.ReadAsync(TestContext.Current.CancellationToken)).Is(2);
    }

    /// <summary>
    /// A source that completes completes the channel, so a reader draining it stops rather than waiting
    /// for values nobody will write.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task WriteToChannel_SourceCompletes_CompletesTheWriter()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();
        var subject = new Subject<int>();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        subject.WriteToChannel(channel.Writer, cts.Token);

        // act
        subject.OnNext(1);
        subject.OnCompleted();

        // assert - drained first: an unbounded channel reports completion only once it is empty
        (await channel.Reader.ReadAsync(TestContext.Current.CancellationToken)).Is(1);
#pragma warning disable VSTHRD003
        await Bounded.AwaitAsync(channel.Reader.Completion);
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// A source that fails completes the channel with that failure, rather than throwing it back at
    /// whoever emitted it and leaving the reader waiting.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task WriteToChannel_SourceFails_FailsTheWriter()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();
        var subject = new Subject<int>();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        subject.WriteToChannel(channel.Writer, cts.Token);

        // act - the emitter must not be the one left to deal with this
        subject.OnError(new InvalidOperationException("source failed"));

        // assert
#pragma warning disable VSTHRD003
        await Bounded.AwaitAsync(channel.Reader.Completion);
#pragma warning restore VSTHRD003
#pragma warning disable VSTHRD003
        var error = await Wrap.It(async () => await channel.Reader.Completion).ThrowsAsync<InvalidOperationException>();
#pragma warning restore VSTHRD003
        error.Message.Is("source failed");
    }

    /// <summary>
    /// Cancelling the subscription stops the writing: values emitted afterwards are not delivered.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task WriteToChannel_Canceled_StopsWriting()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();
        var subject = new Subject<int>();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        subject.WriteToChannel(channel.Writer, cts.Token);
        subject.OnNext(1);
        (await channel.Reader.ReadAsync(TestContext.Current.CancellationToken)).Is(1);

        // act
        await cts.CancelAsync();
        subject.OnNext(2);

        // assert - nothing further arrives
        await Task.Delay(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
        channel.Reader.TryRead(out _).IsFalse("a cancelled subscription must stop writing");
    }
}
