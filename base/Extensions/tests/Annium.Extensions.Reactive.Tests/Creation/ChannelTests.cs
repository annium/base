using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Assertions;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Reactive.Tests.Creation;

public class ChannelTests : TestBase
{
    public ChannelTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Events_AreEmittedCorrectly()
    {
        this.Trace("start");

        // arrange
        var logger = Get<ILogger>();
        var dataSize = 100_000;
        var data = Enumerable.Range(0, dataSize).ToArray();
        var channel = Channel.CreateUnbounded<int>();
        Observable.Range(0, dataSize).WriteToChannel(channel.Writer, CancellationToken.None);
        var log = new List<int>();
        var disposeCounter = 0;

        void OnDisposed()
        {
            disposeCounter++;
        }

        var observable = ObservableExt.FromChannel(channel.Reader, OnDisposed);
        var disposable = Disposable.Box(logger);

        // act
        this.Trace("subscribe");
        disposable += observable.Subscribe(log.Add);

        // assert
        this.Trace("assert");
        await Expect.To(() => { log.IsEqual(data); });
        disposable.Dispose();
        disposeCounter.Is(0);

        this.Trace("done");
    }
}