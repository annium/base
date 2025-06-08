using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Execution.Background.Tests;

/// <summary>
/// Tests for the parallel background executor implementation
/// </summary>
public class ParallelBackgroundExecutorTests : BackgroundExecutorTestBase
{
    public ParallelBackgroundExecutorTests(ITestOutputHelper outputHelper)
        : base(Executor.Parallel<ParallelBackgroundExecutorTests>, outputHelper) { }

    /// <summary>
    /// Tests that the parallel executor processes tasks correctly with potential out-of-order execution
    /// </summary>
    /// <returns>A task representing the test operation</returns>
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

    /// <summary>
    /// Tests the executor's availability state throughout its lifecycle
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Availability()
    {
        this.Trace("start");

        await Availability_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that the executor handles task failures gracefully
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task HandlesFailure()
    {
        this.Trace("start");

        await HandlesFailure_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of synchronous action tasks
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Schedule_SyncAction()
    {
        this.Trace("start");

        await Schedule_SyncAction_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of synchronous action tasks with cancellation support
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Schedule_SyncCancellableAction()
    {
        this.Trace("start");

        await Schedule_SyncCancellableAction_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of asynchronous action tasks
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Schedule_AsyncAction()
    {
        this.Trace("start");

        await Schedule_AsyncAction_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of asynchronous action tasks with cancellation support
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Schedule_AsyncCancellableAction()
    {
        this.Trace("start");

        await Schedule_AsyncCancellableAction_Base();

        this.Trace("done");
    }
}
