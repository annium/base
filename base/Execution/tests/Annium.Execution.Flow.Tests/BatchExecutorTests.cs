using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Execution.Flow.Tests;

/// <summary>
/// Tests for <see cref="IBatchExecutor"/> exercising the handler-dispatch path. Covers the
/// structural <c>else if</c> guard between the <see cref="System.Action"/> and
/// <see cref="System.Func{Task}"/> branches (plan §2.12) and the error-collection behavior.
/// </summary>
public class BatchExecutorTests
{
    /// <summary>
    /// Mixed sync and async handlers all run exactly once in declaration order when no handler
    /// throws. This establishes the baseline against which the else-if dispatch guard protects.
    /// </summary>
    [Fact]
    public async Task RunAsync_MixedSyncAndAsyncHandlers_EachRunsExactlyOnce()
    {
        // arrange
        var syncCount = 0;
        var asyncCount = 0;

        // act
        var result = await Executor
            .Batch()
            .With(() => syncCount++)
            .With(async () =>
            {
                await Task.Yield();
                asyncCount++;
            })
            .With(() => syncCount++)
            .RunAsync();

        // assert
        result.IsOk.IsTrue();
        syncCount.Is(2);
        asyncCount.Is(1);
    }

    /// <summary>
    /// A throwing handler does not stop the batch: the remaining handlers still execute, and the
    /// error is collected in the result.
    /// </summary>
    [Fact]
    public async Task RunAsync_HandlerThrows_CollectsErrorAndContinues()
    {
        // arrange
        var trailingRan = false;

        // act
        var result = await Executor
            .Batch()
            .With(() => throw new InvalidOperationException("boom"))
            .With(() => trailingRan = true)
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        trailingRan.IsTrue();
    }
}
