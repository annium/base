using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Execution.Background.Tests;

public class SequentialBackgroundExecutorTests : BackgroundExecutorTestBase
{
    public SequentialBackgroundExecutorTests(ITestOutputHelper outputHelper)
        : base(Executor.Sequential<SequentialBackgroundExecutorTests>, outputHelper) { }

    [Fact]
    public async Task Works()
    {
        this.Trace("start");

        // arrange
        var size = 4;

        // act
        var result = await Works_Base(size);

        // assert
        var sequence = Enumerable.Range(0, size).SelectMany(x => new[] { x, x + size }).ToArray();
        result.IsEqual(sequence);

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
