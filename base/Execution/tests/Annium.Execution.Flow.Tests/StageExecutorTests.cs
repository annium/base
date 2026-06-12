using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Execution.Flow.Tests;

/// <summary>
/// Tests for <see cref="IStageExecutor"/> rollback semantics. Covers plan §2.13 (off-by-one on
/// the executed-stages counter) and §2.12 (dispatch <c>else if</c> in the internal
/// <c>ExecuteAsync</c> helper — exercised implicitly via the sync and async commit/rollback
/// overloads).
/// </summary>
public class StageExecutorTests
{
    /// <summary>
    /// An empty stage executor (no stages registered) must succeed immediately with an OK result.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_NoStages_ReturnsOk()
    {
        // act
        var result = await Executor.Staged().RunAsync();

        // assert
        result.IsOk.IsTrue();
    }

    /// <summary>
    /// When all stages commit successfully, none of their rollbacks run.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_AllStagesSucceed_NoRollbackInvoked()
    {
        // arrange
        var rollbackCount = 0;

        // act
        var result = await Executor
            .Staged()
            .Stage(commit: () => { }, rollback: () => rollbackCount++)
            .Stage(commit: () => { }, rollback: () => rollbackCount++)
            .RunAsync();

        // assert
        result.IsOk.IsTrue();
        rollbackCount.Is(0);
    }

    /// <summary>
    /// Regression for plan §2.13: when stage 3 of 5 throws during commit, only stages 1 and 2
    /// must roll back. Previously the counter incremented before the <c>await</c>, so the
    /// failing stage (3) was included in the rollback set — undoing work that never happened.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_5Stages_StageThreeThrows_RollsBackStagesOneAndTwoOnly()
    {
        // arrange
        var committed = new bool[5];
        var rolledBack = new bool[5];

        IStageExecutor executor = Executor.Staged();
        for (var i = 0; i < 5; i++)
        {
            var index = i; // capture
            executor = executor.Stage(
                commit: () =>
                {
                    if (index == 2) // stage 3 (0-based index 2) throws
                        throw new InvalidOperationException($"stage {index} boom");
                    committed[index] = true;
                },
                rollback: () => rolledBack[index] = true
            );
        }

        // act
        var result = await executor.RunAsync();

        // assert — result is failure
        result.IsOk.IsFalse();

        // assert — stages 0 and 1 committed, stage 2 failed before setting committed, stages 3/4 never ran
        committed[0].IsTrue();
        committed[1].IsTrue();
        committed[2].IsFalse();
        committed[3].IsFalse();
        committed[4].IsFalse();

        // assert — rollback ran ONLY for stages 0 and 1
        rolledBack[0].IsTrue();
        rolledBack[1].IsTrue();
        rolledBack[2].IsFalse();
        rolledBack[3].IsFalse();
        rolledBack[4].IsFalse();
    }

    /// <summary>
    /// Async-commit path: the <c>Func&lt;Task&gt;</c> branch of <c>ExecuteAsync</c> must be
    /// exercised. Mixed async + sync stages all commit successfully with no rollback.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_AsyncAndSyncCommits_AllRunAndNoRollback()
    {
        // arrange
        var commitOrder = new List<int>();
        var rollbackCount = 0;

        // act
        var result = await Executor
            .Staged()
            .Stage(
                commit: async () =>
                {
                    await Task.Yield();
                    commitOrder.Add(1);
                },
                rollback: () => rollbackCount++
            )
            .Stage(commit: () => commitOrder.Add(2), rollback: () => rollbackCount++)
            .Stage(
                commit: async () =>
                {
                    await Task.Yield();
                    commitOrder.Add(3);
                },
                rollback: () => rollbackCount++
            )
            .RunAsync();

        // assert
        result.IsOk.IsTrue();
        commitOrder.Count.Is(3);
        commitOrder[0].Is(1);
        commitOrder[1].Is(2);
        commitOrder[2].Is(3);
        rollbackCount.Is(0);
    }

    /// <summary>
    /// Async commit that throws must trigger rollback of previously-committed stages (exercises
    /// the <c>Func&lt;Task&gt;</c> branch of <c>ExecuteAsync</c> combined with the
    /// counter-after-await fix).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_AsyncCommitThrows_PriorStagesRollBack()
    {
        // arrange
        var rolledBack = new bool[3];

        // act
        var result = await Executor
            .Staged()
            .Stage(commit: () => { }, rollback: () => rolledBack[0] = true)
            .Stage(
                commit: async () =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("async boom");
                },
                rollback: () => rolledBack[1] = true
            )
            .Stage(commit: () => { }, rollback: () => rolledBack[2] = true)
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        rolledBack[0].IsTrue();
        rolledBack[1].IsFalse();
        rolledBack[2].IsFalse();
    }

    /// <summary>
    /// When the very first stage throws during commit, no stage committed successfully and no
    /// rollback runs.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_FirstStageThrows_NoRollbackInvoked()
    {
        // arrange
        var rollbackCount = 0;

        // act
        var result = await Executor
            .Staged()
            .Stage(commit: () => throw new InvalidOperationException("boom"), rollback: () => rollbackCount++)
            .Stage(commit: () => { }, rollback: () => rollbackCount++)
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        rollbackCount.Is(0);
    }

    /// <summary>
    /// Stage(Action commit, Func&lt;ValueTask&gt; rollback): when stage 2 commit throws, stage 1's
    /// async rollback must be awaited and its side-effect observed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_SyncCommitAsyncRollback_CommitFails_AsyncRollbackRuns()
    {
        // arrange
        var asyncRollbackRan = false;

        // act
        var result = await Executor
            .Staged()
            .Stage(
                commit: () => { },
                rollback: async () =>
                {
                    await Task.Yield();
                    asyncRollbackRan = true;
                }
            )
            .Stage(commit: () => throw new InvalidOperationException("stage 2 boom"), rollback: () => { })
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        asyncRollbackRan.IsTrue();
    }

    /// <summary>
    /// Stage(Func&lt;ValueTask&gt; commit, Func&lt;ValueTask&gt; rollback): when stage 2 async commit throws,
    /// stage 1's async rollback must be awaited and its side-effect observed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_AsyncCommitAsyncRollback_CommitFails_AsyncRollbackRuns()
    {
        // arrange
        var asyncRollbackRan = false;

        // act
        var result = await Executor
            .Staged()
            .Stage(
                commit: async () => await Task.Yield(),
                rollback: async () =>
                {
                    await Task.Yield();
                    asyncRollbackRan = true;
                }
            )
            .Stage(
                commit: async () =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("async stage 2 boom");
                },
                rollback: () => { }
            )
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        asyncRollbackRan.IsTrue();
    }

    /// <summary>
    /// When a rollback itself throws, the error is collected into the result alongside the
    /// original commit error, and sibling rollbacks (for other committed stages) still run —
    /// i.e. a throwing rollback does not abort the remaining rollbacks.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_RollbackThrows_CollectsBothErrorsAndContinuesRemainingRollbacks()
    {
        // arrange
        var siblingRollbackRan = false;

        // act — three stages commit; stage 3's commit throws; stage 2's rollback also throws;
        // stage 1's rollback (siblingRollbackRan) must still run.
        var result = await Executor
            .Staged()
            .Stage(commit: () => { }, rollback: () => siblingRollbackRan = true)
            .Stage(commit: () => { }, rollback: () => throw new InvalidOperationException("rollback boom"))
            .Stage(commit: () => throw new InvalidOperationException("commit boom"), rollback: () => { })
            .RunAsync();

        // assert — overall failure
        result.IsOk.IsFalse();

        // both the commit error and the rollback error are recorded
        result.PlainErrors.Has(2);

        // sibling rollback still ran despite stage-2's rollback having thrown
        siblingRollbackRan.IsTrue();
    }

    /// <summary>
    /// Stage(Action commit) with no rollback: when commit throws RunAsync must return a failed
    /// result without propagating the exception — the absent rollback is a no-op.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_SyncCommitNoRollback_CommitThrows_ResultNotOkNoException()
    {
        // act
        var result = await Executor
            .Staged()
            .Stage(commit: () => throw new InvalidOperationException("sync boom"))
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        result.PlainErrors.Has(1);
    }

    /// <summary>
    /// Stage(Func&lt;ValueTask&gt; commit) with no rollback: when the async commit throws RunAsync
    /// must return a failed result without propagating the exception.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_AsyncCommitNoRollback_CommitThrows_ResultNotOkNoException()
    {
        // act
        var result = await Executor
            .Staged()
            .Stage(commit: async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("async boom");
            })
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        result.PlainErrors.Has(1);
    }

    /// <summary>
    /// Stage(<see cref="Func{ValueTask}"/> commit, <see cref="Action"/> rollback): when stage 1's
    /// async commit succeeds and stage 2's commit throws, stage 1's sync <see cref="Action"/>
    /// rollback must run.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_AsyncCommitSyncRollback_PriorStageRollbackRuns()
    {
        // arrange
        var rolledBack = false;

        // act
        var result = await Executor
            .Staged()
            .Stage(commit: async () => await Task.Yield(), rollback: (Action)(() => rolledBack = true))
            .Stage(commit: () => throw new InvalidOperationException("stage 2 boom"), rollback: () => { })
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        rolledBack.IsTrue();
    }
}
