using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Extensions;

/// <summary>
/// Tests for the channel extensions functionality in reactive extensions.
/// </summary>
public class ChannelExtensionsTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public ChannelExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A channel that finishes completes the observable, so a consumer awaiting its end is not left waiting
    /// for a channel nobody will write to again.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Completed_Channel_CompletesTheObservable()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();
        var completed = new TaskCompletionSource();
        using var subscription = channel.Reader.AsObservable().Subscribe(_ => { }, () => completed.TrySetResult());

        // act
        await channel.Writer.WriteAsync(1, TestContext.Current.CancellationToken);
        channel.Writer.Complete();

        // assert
        await Bounded.AwaitAsync(completed.Task);
    }

    /// <summary>
    /// A channel completed with a failure hands that failure on rather than passing for a clean end.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Failed_Channel_FailsTheObservable()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();
        var failure = new TaskCompletionSource<Exception>();
        using var subscription = channel
            .Reader.AsObservable()
            .Subscribe(_ => { }, e => failure.TrySetResult(e), () => { });

        // act
        channel.Writer.Complete(new InvalidOperationException("writer failed"));

        // assert
        await Bounded.AwaitAsync(failure.Task);
        (await failure.Task).As<InvalidOperationException>().Message.Is("writer failed");
    }

    /// <summary>
    /// Tests that events are emitted correctly when converting a channel reader to an observable,
    /// including proper disposal behavior.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Events_AreEmittedCorrectly()
    {
        this.Trace("start");

        // arrange
        var logger = Get<ILogger>();
        var dataSize = 100_000;
        var data = Enumerable.Range(0, dataSize).ToArray();
        var channel = Channel.CreateUnbounded<int>();

        // written directly rather than through WriteToChannel: that ends the channel with its source, and
        // this test is about a channel that is still open when its subscription is disposed
        this.Trace("write to channel");
        foreach (var item in data)
            channel.Writer.TryWrite(item);
        var log = new TestLog<int>();
        var disposeCounter = 0;

        this.Trace("await");
        await Task.Delay(50, TestContext.Current.CancellationToken);

        this.Trace("create observable from channel");
        var observable = channel.Reader.AsObservable(HandleDisposed);
        var disposable = Disposable.Box(logger);

        // act
        this.Trace("subscribe");
        disposable += observable.Subscribe(log.Add);

        // assert
        // the reader is drained asynchronously - waiting for the count rather than asserting it outright
        // keeps this a failure rather than a race if reading ever stops completing synchronously
        this.Trace("assert log is complete");
        await Expect.ToAsync(() => log.Has(data.Length));

        this.Trace("assert log matches data and dispose callback is not called");
        log.SequenceEqual(data).IsTrue();
        disposeCounter.Is(0);

        this.Trace("dispose and verify dispose callback is called");
        await disposable.DisposeAsync();
        await Expect.ToAsync(() => disposeCounter.Is(1));

        this.Trace("done");
        return;

        void HandleDisposed()
        {
            this.Trace("disposed");
            disposeCounter++;
        }
    }
}
