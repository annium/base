using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for <see cref="Disposable"/> to verify disposable behavior.
/// </summary>
public class DisposableTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DisposableTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that adding disposables to an async disposable box works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_Add_Works()
    {
        // arrange
        var box = Disposable.AsyncBox(Get<ILogger>());
        var calls = 0;

        // act
        box += Disposable.Create(() => ++calls);
        box += Disposable.Create(() =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        });
        box += () => ++calls;
        box += () =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        };
        await box.DisposeAsync();

        // assert
        calls.Is(4);
    }

    /// <summary>
    /// Verifies that removing disposables from an async disposable box works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_Remove_Works()
    {
        // arrange
        var box = Disposable.AsyncBox(Get<ILogger>());
        var calls = 0;

        // act
        var disposable = Disposable.Create(() => ++calls);
        var asyncDisposable = Disposable.Create(() =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        });
        void Dispose() => ++calls;
        ValueTask AsyncDispose()
        {
            ++calls;
            return ValueTask.CompletedTask;
        }
        box += disposable;
        box -= disposable;
        box += asyncDisposable;
        box -= asyncDisposable;
        box += Dispose;
        box -= Dispose;
        box += AsyncDispose;
        box -= AsyncDispose;
        await box.DisposeAsync();

        // assert
        calls.Is(0);
    }

    /// <summary>
    /// Verifies that resetting an async disposable box works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_Reset_Works()
    {
        // arrange
        var box = Disposable.AsyncBox(Get<ILogger>());

        // act
        await box.DisposeAndResetAsync();

        // assert
        box.IsDisposed.IsFalse();
    }

    /// <summary>
    /// Verifies that adding disposables to a disposable box works correctly.
    /// </summary>
    [Fact]
    public void Disposable_Add_Works()
    {
        // arrange
        var box = Disposable.Box(Get<ILogger>());
        var calls = 0;

        // act
        box += Disposable.Create(() => ++calls);
        box += () => ++calls;
        box.Dispose();

        // assert
        calls.Is(2);
    }

    /// <summary>
    /// Verifies that removing disposables from a disposable box works correctly.
    /// </summary>
    [Fact]
    public void Disposable_Remove_Works()
    {
        // arrange
        var box = Disposable.Box(Get<ILogger>());
        var calls = 0;

        // act
        var disposable = Disposable.Create(() => ++calls);
        void Dispose() => ++calls;
        box += disposable;
        box -= disposable;
        box += Dispose;
        box -= Dispose;
        box.Dispose();

        // assert
        calls.Is(0);
    }

    /// <summary>
    /// Verifies that resetting a disposable box works correctly.
    /// </summary>
    [Fact]
    public void Disposable_Reset_Works()
    {
        // arrange
        var box = Disposable.Box(Get<ILogger>());

        // act
        box.DisposeAndReset();

        // assert
        box.IsDisposed.IsFalse();
    }

    /// <summary>
    /// Stress test for AC8: concurrent <c>Add</c> operations racing with a single
    /// <c>DisposeAsync</c> must result in every Add either (a) being accepted and its
    /// disposable invoked exactly once during dispose, or (b) rejected with
    /// <see cref="ObjectDisposedException"/>. No leaks, no double-dispose.
    /// </summary>
    [Fact]
    public async Task AsyncDisposable_ConcurrentAddDuringDispose_AllDisposedOrRejected()
    {
        // arrange
        const int addCount = 100;
        var box = Disposable.AsyncBox(Get<ILogger>());
        var disposables = Enumerable.Range(0, addCount).Select(_ => new CountingDisposable()).ToArray();
        var rejected = 0;
        var ct = TestContext.Current.CancellationToken;

        // act — fire all Adds in parallel, concurrently with a single DisposeAsync.
        // The race window is microseconds; some Adds land before dispose (accepted,
        // disposed during the dispose pass), some after (rejected with ObjectDisposedException).
        var addTasks = disposables
            .Select(d =>
                Task.Run(
                    () =>
                    {
                        try
                        {
                            box += d;
                        }
                        catch (ObjectDisposedException)
                        {
                            Interlocked.Increment(ref rejected);
                        }
                    },
                    ct
                )
            )
            .ToArray();

        var disposeTask = Task.Run(async () => await box.DisposeAsync(), ct);

        await Task.WhenAll(addTasks.Concat(new[] { disposeTask }));

        // assert — no double-dispose; every disposable is either disposed exactly once
        // (accepted) or never disposed (rejected). accepted + rejected must equal addCount.
        var accepted = disposables.Count(d => d.DisposeCount == 1);
        var untouched = disposables.Count(d => d.DisposeCount == 0);
        var doubleDisposed = disposables.Count(d => d.DisposeCount > 1);

        doubleDisposed.Is(0);
        accepted.Is(addCount - rejected);
        untouched.Is(rejected);
    }

    /// <summary>
    /// Verifies that calling <c>Dispose()</c> on a <c>DisposableBox</c> twice is idempotent (review T9).
    /// </summary>
    [Fact]
    public void Disposable_DoubleDispose_IsIdempotent()
    {
        var probe = new CountingDisposable();
        var box = Disposable.Box(Logger);
        box += probe;

        box.Dispose();
        box.Dispose();

        // The probe was inside the box; it must have been disposed exactly once even though we
        // called box.Dispose() twice. The lock guard in DisposeBase short-circuits the second call.
        probe.DisposeCount.Is(1);
    }

    /// <summary>
    /// Verifies that calling <c>DisposeAsync()</c> on an <c>AsyncDisposableBox</c> twice is idempotent (review T9).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_DoubleDispose_IsIdempotent()
    {
        var probe = new CountingDisposable();
        var box = Disposable.AsyncBox(Logger);
        box += probe;

        await box.DisposeAsync();
        await box.DisposeAsync();

        probe.DisposeCount.Is(1);
    }

    /// <summary>
    /// Verifies that when one async-disposable throws during <c>DisposeAsync</c>, the exception propagates
    /// (review T8 — exception-during-dispose).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_OneAsyncDisposableThrows_ExceptionPropagates()
    {
        var box = Disposable.AsyncBox(Logger);
        box += new ThrowingAsyncDisposable(new InvalidOperationException("dispose-boom"));

        var ex = await Wrap.It(async () => await box.DisposeAsync()).ThrowsAsync<InvalidOperationException>();
        ex.Message.Is("dispose-boom");
    }

    /// <summary>
    /// Stress test: concurrent sync Add operations racing with a single Dispose must result in every
    /// Add either (a) being accepted and its disposable invoked exactly once during dispose, or
    /// (b) rejected with <see cref="ObjectDisposedException"/>. No leaks, no double-dispose.
    /// </summary>
    [Fact]
    public async Task Disposable_ConcurrentAddDuringDispose_AllDisposedOrRejected()
    {
        // arrange
        const int addCount = 200;
        var box = Disposable.Box(Get<ILogger>());
        var rejected = 0;
        var disposables = Enumerable.Range(0, addCount).Select(_ => new CountingDisposable()).ToArray();
        var ct = TestContext.Current.CancellationToken;

        // act — fire all Adds in parallel, concurrently with a single Dispose.
        var addTasks = disposables
            .Select(d =>
                Task.Run(
                    () =>
                    {
                        try
                        {
                            box += d;
                        }
                        catch (ObjectDisposedException)
                        {
                            Interlocked.Increment(ref rejected);
                        }
                    },
                    ct
                )
            )
            .ToArray();

        var disposeTask = Task.Run(() => box.Dispose(), ct);

        await Task.WhenAll(addTasks.Concat(new[] { disposeTask }));

        // assert — no double-dispose; accepted + rejected == addCount.
        var accepted = disposables.Count(d => d.DisposeCount == 1);
        var untouched = disposables.Count(d => d.DisposeCount == 0);
        var doubleDisposed = disposables.Count(d => d.DisposeCount > 1);

        doubleDisposed.Is(0);
        accepted.Is(addCount - rejected);
        untouched.Is(rejected);
    }

    /// <summary>
    /// Verifies that adding a disposable to a <c>DisposableBox</c> after it has been disposed
    /// throws <see cref="ObjectDisposedException"/>.
    /// </summary>
    [Fact]
    public void DisposableBox_AddAfterDispose_ThrowsObjectDisposedException()
    {
        // arrange
        var box = Disposable.Box(Get<ILogger>());
        box.Dispose();

        // act + assert
        Wrap.It(() => { box += Disposable.Create(() => { }); }).Throws<ObjectDisposedException>();
    }

    /// <summary>
    /// Verifies that after a DisposeAndResetAsync the original entries are not disposed again on
    /// the next DisposeAsync — only the newly-added entries fire.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_Reset_PreviousEntriesNotRedisposed()
    {
        // arrange
        var box = Disposable.AsyncBox(Get<ILogger>());
        var calls = 0;

        // act — first round: add 2 disposables, dispose and reset
        box += Disposable.Create(() => ++calls);
        box += Disposable.Create(() => ++calls);
        await box.DisposeAndResetAsync();

        var callsAfterFirstDispose = calls;

        // add one more disposable in round 2 and dispose
        box += Disposable.Create(() => ++calls);
        await box.DisposeAsync();

        // assert — first two must not fire again; only the third one fires in round 2.
        callsAfterFirstDispose.Is(2);
        calls.Is(3);
    }

    /// <summary>
    /// Verifies that after <c>DisposeAndResetAsync</c> only newly-added entries are disposed on
    /// the subsequent DisposeAsync — the original async dispose lambda fires exactly once.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_DisposeAndReset_ThenAddAndDispose_OnlyNewEntriesDisposed()
    {
        // arrange
        var box = Disposable.AsyncBox(Get<ILogger>());
        var firstCalls = 0;
        var secondCalls = 0;

        // act — first round: async dispose lambda
        box += () =>
        {
            ++firstCalls;
            return ValueTask.CompletedTask;
        };
        await box.DisposeAndResetAsync();

        // second round: new async dispose lambda
        box += () =>
        {
            ++secondCalls;
            return ValueTask.CompletedTask;
        };
        await box.DisposeAsync();

        // assert — each lambda fires exactly once
        firstCalls.Is(1);
        secondCalls.Is(1);
    }

    /// <summary>
    /// IDisposable that records how many times it was disposed — for detecting leaks or
    /// double-disposal in the concurrent stress test above.
    /// </summary>
    private sealed class CountingDisposable : IDisposable
    {
        private int _disposeCount;

        public int DisposeCount => Volatile.Read(ref _disposeCount);

        public void Dispose() => Interlocked.Increment(ref _disposeCount);
    }

    /// <summary>
    /// IAsyncDisposable that throws the given exception when disposed — used to verify exception
    /// propagation through <c>AsyncDisposableBox.DisposeAsync</c>.
    /// </summary>
    private sealed class ThrowingAsyncDisposable : IAsyncDisposable
    {
        private readonly Exception _ex;

        public ThrowingAsyncDisposable(Exception ex)
        {
            _ex = ex;
        }

        public ValueTask DisposeAsync() => ValueTask.FromException(_ex);
    }
}
