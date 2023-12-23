using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading.Channels;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Tests.Threading.Channels;

public class ChannelReaderExtensionsTests : TestBase
{
    public ChannelReaderExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
        using var pipe = source.Reader.Pipe(target.Writer);

        // assert
        this.Trace("assert log is complete");
        await Expect.To(() => log.Has(data.Length));

        this.Trace("assert log matches data and dispose callback is not called");
        log.SequenceEqual(data).IsTrue();

        this.Trace("done");
    }
}
