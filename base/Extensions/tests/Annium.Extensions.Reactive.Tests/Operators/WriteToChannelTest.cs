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
