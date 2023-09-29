using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Execution.Tests.Background;

public class SequentialBackgroundExecutorTests : BackgroundExecutorTestBase
{
    public SequentialBackgroundExecutorTests(ITestOutputHelper outputHelper)
        : base(Executor.Background.Sequential<SequentialBackgroundExecutorTests>, outputHelper)
    {
    }

    [Fact]
    public async Task Works()
    {
        // arrange
        var size = 4;

        // act
        var result = await Works_Base(size);

        // assert
        var sequence = Enumerable.Range(0, size).SelectMany(x => new[] { x, x + size }).ToArray();
        result.IsEqual(sequence);
    }

    [Fact]
    public async Task Availability()
    {
        await Availability_Base();
    }

    [Fact]
    public async Task HandlesFailure()
    {
        await HandlesFailure_Base();
    }

    [Fact]
    public async Task Schedule_SyncAction()
    {
        await Schedule_SyncAction_Base();
    }

    [Fact]
    public async Task Schedule_SyncCancellableAction()
    {
        await Schedule_SyncCancellableAction_Base();
    }

    [Fact]
    public async Task Schedule_AsyncAction()
    {
        await Schedule_AsyncAction_Base();
    }

    [Fact]
    public async Task Schedule_AsyncCancellableAction()
    {
        await Schedule_AsyncCancellableAction_Base();
    }
}