using System;
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
    /// When all stages commit successfully, none of their rollbacks run.
    /// </summary>
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
    [Fact]
    public async Task RunAsync_AsyncAndSyncCommits_AllRunAndNoRollback()
    {
        // arrange
        var commitOrder = new System.Collections.Generic.List<int>();
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
    [Fact]
    public async Task RunAsync_FirstStageThrows_NoRollbackInvoked()
    {
        // arrange
        var rollbackCount = 0;

        // act
        var result = await Executor
            .Staged()
            .Stage(
                commit: () => throw new InvalidOperationException("boom"),
                rollback: () => rollbackCount++
            )
            .Stage(commit: () => { }, rollback: () => rollbackCount++)
            .RunAsync();

        // assert
        result.IsOk.IsFalse();
        rollbackCount.Is(0);
    }
}
