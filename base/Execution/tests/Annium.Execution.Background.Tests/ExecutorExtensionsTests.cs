using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using OneOf;
using Xunit;

namespace Annium.Execution.Background.Tests;

/// <summary>
/// Tests for <see cref="ExecutorExtensions"/> — all 8 ExecuteAsync overloads.
/// For each overload we verify: (a) scheduled + succeeds, (b) not-scheduled returns the "no value" sentinel,
/// (c) task throws and the exception propagates to the awaiter.
/// </summary>
public class ExecutorExtensionsTests : TestBase
{
    public ExecutorExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    // -------------------------------------------------------------------------
    // ExecuteAsync(Action) — returns ValueTask<bool>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schedules a synchronous Action that succeeds; ExecuteAsync returns true
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Action_Scheduled_ReturnsTrue()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        var ran = false;
        // Explicit cast ensures the Action overload is picked (not Func<bool>)
        var result = await executor.ExecuteAsync((Action)(() => ran = true));

        await executor.DisposeAsync();

        result.IsTrue();
        ran.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// When the executor is already disposed, ExecuteAsync(Action) returns false
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Action_NotScheduled_ReturnsFalse()
    {
        this.Trace("start");

        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        await executor.DisposeAsync();

        var result = await executor.ExecuteAsync((Action)(() => { }));

        result.IsFalse();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Action throws, ExecuteAsync propagates the exception to the awaiter
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Action_TaskThrows_PropagatesException()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync((Action)(() => throw new InvalidOperationException("boom")))
            )
            .ThrowsAsync<InvalidOperationException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // ExecuteAsync(Action<CancellationToken>) — returns ValueTask<bool>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schedules a cancellable synchronous Action that succeeds; ExecuteAsync returns true
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableAction_Scheduled_ReturnsTrue()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        var ran = false;
        var result = await executor.ExecuteAsync((Action<CancellationToken>)(_ => ran = true));

        await executor.DisposeAsync();

        result.IsTrue();
        ran.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// When the executor is already disposed, ExecuteAsync(Action{CancellationToken}) returns false
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableAction_NotScheduled_ReturnsFalse()
    {
        this.Trace("start");

        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        await executor.DisposeAsync();

        var result = await executor.ExecuteAsync((Action<CancellationToken>)(_ => { }));

        result.IsFalse();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Action{CancellationToken} throws, ExecuteAsync propagates the exception
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableAction_TaskThrows_PropagatesException()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync(
                    (Action<CancellationToken>)(_ => throw new InvalidOperationException("boom"))
                )
            )
            .ThrowsAsync<InvalidOperationException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Action{CancellationToken} throws OperationCanceledException, ExecuteAsync
    /// propagates it as OperationCanceledException to the caller (via SetCanceled on the relay TCS),
    /// not as a generic fault — distinguishes it from an ordinary exception path.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableAction_TaskThrowsOperationCanceled_PropagatesCancellation()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync(
                    (Action<CancellationToken>)(_ => throw new OperationCanceledException("intentional cancel"))
                )
            )
            .ThrowsAsync<OperationCanceledException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // ExecuteAsync(Func<T>) — returns ValueTask<OneOf<T, None>>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schedules a synchronous Func{T} that succeeds; result carries the value (IsT0)
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Func_Scheduled_ReturnsValue()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        OneOf<int, None> result = await executor.ExecuteAsync((Func<int>)(() => 42));

        await executor.DisposeAsync();

        result.IsT0.IsTrue();
        result.AsT0.Is(42);

        this.Trace("done");
    }

    /// <summary>
    /// When the executor is already disposed, ExecuteAsync(Func{T}) returns None (IsT1)
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Func_NotScheduled_ReturnsNone()
    {
        this.Trace("start");

        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        await executor.DisposeAsync();

        OneOf<int, None> result = await executor.ExecuteAsync((Func<int>)(() => 42));

        result.IsT1.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Func{T} throws, ExecuteAsync propagates the exception
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Func_TaskThrows_PropagatesException()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync((Func<int>)(() => throw new InvalidOperationException("boom")))
            )
            .ThrowsAsync<InvalidOperationException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // ExecuteAsync(Func<CancellationToken, T>) — returns ValueTask<OneOf<T, None>>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schedules a cancellable synchronous Func{T} that succeeds; result carries the value (IsT0)
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableFunc_Scheduled_ReturnsValue()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        OneOf<int, None> result = await executor.ExecuteAsync((Func<CancellationToken, int>)(_ => 7));

        await executor.DisposeAsync();

        result.IsT0.IsTrue();
        result.AsT0.Is(7);

        this.Trace("done");
    }

    /// <summary>
    /// When the executor is already disposed, ExecuteAsync(Func{CancellationToken,T}) returns None (IsT1)
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableFunc_NotScheduled_ReturnsNone()
    {
        this.Trace("start");

        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        await executor.DisposeAsync();

        OneOf<int, None> result = await executor.ExecuteAsync((Func<CancellationToken, int>)(_ => 7));

        result.IsT1.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Func{CancellationToken, T} throws, ExecuteAsync propagates the exception
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableFunc_TaskThrows_PropagatesException()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync(
                    (Func<CancellationToken, int>)(_ => throw new InvalidOperationException("boom"))
                )
            )
            .ThrowsAsync<InvalidOperationException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // ExecuteAsync(Func<ValueTask>) — returns ValueTask<bool>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schedules an async Func{ValueTask} that succeeds; ExecuteAsync returns true
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ValueTask_Scheduled_ReturnsTrue()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        var ran = false;
        var result = await executor.ExecuteAsync(
            (Func<ValueTask>)(
                async () =>
                {
                    await Task.Yield();
                    ran = true;
                }
            )
        );

        await executor.DisposeAsync();

        result.IsTrue();
        ran.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// When the executor is already disposed, ExecuteAsync(Func{ValueTask}) returns false
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ValueTask_NotScheduled_ReturnsFalse()
    {
        this.Trace("start");

        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        await executor.DisposeAsync();

        var result = await executor.ExecuteAsync((Func<ValueTask>)(async () => await Task.Yield()));

        result.IsFalse();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Func{ValueTask} throws, ExecuteAsync propagates the exception
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ValueTask_TaskThrows_PropagatesException()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync(
                    (Func<ValueTask>)(
                        async () =>
                        {
                            await Task.Yield();
                            throw new InvalidOperationException("boom");
                        }
                    )
                )
            )
            .ThrowsAsync<InvalidOperationException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // ExecuteAsync(Func<CancellationToken, ValueTask>) — returns ValueTask<bool>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schedules a cancellable async Func{ValueTask} that succeeds; ExecuteAsync returns true
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableValueTask_Scheduled_ReturnsTrue()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        var ran = false;
        var result = await executor.ExecuteAsync(
            (Func<CancellationToken, ValueTask>)(
                async _ =>
                {
                    await Task.Yield();
                    ran = true;
                }
            )
        );

        await executor.DisposeAsync();

        result.IsTrue();
        ran.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// When the executor is already disposed, ExecuteAsync(Func{CancellationToken,ValueTask}) returns false
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableValueTask_NotScheduled_ReturnsFalse()
    {
        this.Trace("start");

        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        await executor.DisposeAsync();

        var result = await executor.ExecuteAsync((Func<CancellationToken, ValueTask>)(async _ => await Task.Yield()));

        result.IsFalse();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Func{CancellationToken,ValueTask} throws, ExecuteAsync propagates the exception
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableValueTask_TaskThrows_PropagatesException()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync(
                    (Func<CancellationToken, ValueTask>)(
                        async _ =>
                        {
                            await Task.Yield();
                            throw new InvalidOperationException("boom");
                        }
                    )
                )
            )
            .ThrowsAsync<InvalidOperationException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Func{CancellationToken,ValueTask} throws OperationCanceledException,
    /// ExecuteAsync propagates it as OperationCanceledException to the caller (via SetCanceled on the
    /// relay TCS), not as a generic fault — distinguishes it from an ordinary exception path.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableValueTask_TaskThrowsOperationCanceled_PropagatesCancellation()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync(
                    (Func<CancellationToken, ValueTask>)(
                        async _ =>
                        {
                            await Task.Yield();
                            throw new OperationCanceledException("intentional cancel");
                        }
                    )
                )
            )
            .ThrowsAsync<OperationCanceledException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // ExecuteAsync(Func<ValueTask<T>>) — returns ValueTask<OneOf<T, None>>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schedules an async Func{ValueTask{T}} that succeeds; result carries the value (IsT0)
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ValueTaskOfT_Scheduled_ReturnsValue()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        OneOf<string, None> result = await executor.ExecuteAsync(
            (Func<ValueTask<string>>)(
                async () =>
                {
                    await Task.Yield();
                    return "hello";
                }
            )
        );

        await executor.DisposeAsync();

        result.IsT0.IsTrue();
        result.AsT0.Is("hello");

        this.Trace("done");
    }

    /// <summary>
    /// When the executor is already disposed, ExecuteAsync(Func{ValueTask{T}}) returns None (IsT1)
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ValueTaskOfT_NotScheduled_ReturnsNone()
    {
        this.Trace("start");

        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        await executor.DisposeAsync();

        OneOf<string, None> result = await executor.ExecuteAsync(
            (Func<ValueTask<string>>)(
                async () =>
                {
                    await Task.Yield();
                    return "hello";
                }
            )
        );

        result.IsT1.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Func{ValueTask{T}} throws, ExecuteAsync propagates the exception
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ValueTaskOfT_TaskThrows_PropagatesException()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync(
                    (Func<ValueTask<string>>)(
                        async () =>
                        {
                            await Task.Yield();
                            throw new InvalidOperationException("boom");
                        }
                    )
                )
            )
            .ThrowsAsync<InvalidOperationException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // ExecuteAsync(Func<CancellationToken, ValueTask<T>>) — returns ValueTask<OneOf<T, None>>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schedules a cancellable async Func{ValueTask{T}} that succeeds; result carries the value (IsT0)
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableValueTaskOfT_Scheduled_ReturnsValue()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        OneOf<int, None> result = await executor.ExecuteAsync(
            (Func<CancellationToken, ValueTask<int>>)(
                async _ =>
                {
                    await Task.Yield();
                    return 99;
                }
            )
        );

        await executor.DisposeAsync();

        result.IsT0.IsTrue();
        result.AsT0.Is(99);

        this.Trace("done");
    }

    /// <summary>
    /// When the executor is already disposed, ExecuteAsync(Func{CancellationToken,ValueTask{T}}) returns None (IsT1)
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableValueTaskOfT_NotScheduled_ReturnsNone()
    {
        this.Trace("start");

        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        await executor.DisposeAsync();

        OneOf<int, None> result = await executor.ExecuteAsync(
            (Func<CancellationToken, ValueTask<int>>)(
                async _ =>
                {
                    await Task.Yield();
                    return 99;
                }
            )
        );

        result.IsT1.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// When the scheduled Func{CancellationToken,ValueTask{T}} throws, ExecuteAsync propagates the exception
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CancellableValueTaskOfT_TaskThrows_PropagatesException()
    {
        this.Trace("start");

        var ct = TestContext.Current.CancellationToken;
        await using var executor = Executor.Sequential<ExecutorExtensionsTests>(Get<ILogger>());
        executor.Start(ct);

        await Wrap.It(async () =>
                await executor.ExecuteAsync(
                    (Func<CancellationToken, ValueTask<int>>)(
                        async _ =>
                        {
                            await Task.Yield();
                            throw new InvalidOperationException("boom");
                        }
                    )
                )
            )
            .ThrowsAsync<InvalidOperationException>();

        await executor.DisposeAsync();

        this.Trace("done");
    }
}
