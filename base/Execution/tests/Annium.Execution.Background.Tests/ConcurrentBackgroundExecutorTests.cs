using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Execution.Background.Tests;

public class ConcurrentBackgroundExecutorTests : BackgroundExecutorTestBase
{
    public ConcurrentBackgroundExecutorTests(ITestOutputHelper outputHelper)
        : base(x => Executor.Concurrent<ConcurrentBackgroundExecutorTests>(x), outputHelper) { }

    [Fact]
    public async Task Works()
    {
        this.Trace("start");

        // arrange
        var size = Environment.ProcessorCount * 2;

        // act
        var result = await Works_Base(size);

        // assert
        var sequence = Enumerable.Range(0, 2 * size).ToArray();
        result.IsNotEqual(sequence);
        result.OrderBy(x => x).ToArray().IsEqual(sequence);

        this.Trace("done");
    }

    [Fact]
    public async Task Availability()
    {
        this.Trace("start");

        await Availability_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task HandlesFailure()
    {
        this.Trace("start");

        await HandlesFailure_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Schedule_SyncAction()
    {
        this.Trace("start");

        await Schedule_SyncAction_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Schedule_SyncCancellableAction()
    {
        this.Trace("start");

        await Schedule_SyncCancellableAction_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Schedule_AsyncAction()
    {
        this.Trace("start");

        await Schedule_AsyncAction_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Schedule_AsyncCancellableAction()
    {
        this.Trace("start");

        await Schedule_AsyncCancellableAction_Base();

        this.Trace("done");
    }
}
