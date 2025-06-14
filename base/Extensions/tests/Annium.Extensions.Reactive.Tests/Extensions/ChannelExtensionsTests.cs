using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Extensions.Reactive.Extensions;
using Annium.Extensions.Reactive.Operators;
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

        this.Trace("write to channel");
        Observable.Range(0, dataSize).WriteToChannel(channel.Writer, CancellationToken.None);
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
        this.Trace("assert log is complete");
        log.Has(data.Length);

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
