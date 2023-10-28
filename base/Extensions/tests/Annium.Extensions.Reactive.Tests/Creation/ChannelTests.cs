using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Reactive.Tests.Creation;

public class ChannelTests : TestBase
{
    public ChannelTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
        var log = new List<int>();
        var disposeCounter = 0;

        this.Trace("create observable from channel");
        var observable = ObservableExt.FromChannel(channel.Reader, Disposed);
        var disposable = Disposable.Box(logger);

        // act
        this.Trace("subscribe");
        disposable += observable.Subscribe(log.Add);

        // assert
        this.Trace("assert log is complete");
        await Expect.To(() => log.Has(data.Length));

        this.Trace("assert log matches data and dispose callback is not called");
        log.IsEqual(data);
        disposeCounter.Is(0);

        this.Trace("dispose and verify dispose callback is called");
        disposable.Dispose();
        disposeCounter.Is(0);

        this.Trace("done");
        return;

        void Disposed()
        {
            this.Trace("disposed");
            disposeCounter++;
        }
    }
}
