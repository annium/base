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
        box += Disposable.Create(() => Task.FromResult(++calls));
        box += () => ++calls;
        box += () => Task.FromResult(++calls);
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
        var asyncDisposable = Disposable.Create(() => Task.FromResult(++calls));
        void Dispose() => ++calls;
        Task AsyncDispose() => Task.FromResult(++calls);
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
    /// IDisposable that records how many times it was disposed — for detecting leaks or
    /// double-disposal in the concurrent stress test above.
    /// </summary>
    private sealed class CountingDisposable : IDisposable
    {
        private int _disposeCount;

        public int DisposeCount => Volatile.Read(ref _disposeCount);

        public void Dispose() => Interlocked.Increment(ref _disposeCount);
    }
}
