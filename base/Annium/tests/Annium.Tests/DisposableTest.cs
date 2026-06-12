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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// <returns>A task that represents the asynchronous test.</returns>
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
        Wrap.It(() =>
            {
                box += Disposable.Create(() => { });
            })
            .Throws<ObjectDisposedException>();
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
    /// AsyncDisposer (via <c>Disposable.Create(Func&lt;ValueTask&gt;)</c>): second DisposeAsync must be a no-op.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposer_DoubleDispose_HandleCalledOnlyOnce()
    {
        var calls = 0;
        var disposable = Disposable.Create(() =>
        {
            Interlocked.Increment(ref calls);
            return ValueTask.CompletedTask;
        });

        await disposable.DisposeAsync();
        await disposable.DisposeAsync();

        calls.Is(1);
    }

    /// <summary>
    /// AsyncDisposer: the handle must be invoked on the first DisposeAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposer_HandleInvoked_OnFirstDisposeOnly()
    {
        var invoked = false;
        var disposable = Disposable.Create(() =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });

        invoked.IsFalse();
        await disposable.DisposeAsync();
        invoked.IsTrue();
    }

    /// <summary>
    /// AsyncDisposer: concurrent DisposeAsync calls must invoke the handle exactly once.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposer_ConcurrentDispose_HandleCalledExactlyOnce()
    {
        var calls = 0;
        var disposable = Disposable.Create(() =>
        {
            Interlocked.Increment(ref calls);
            return ValueTask.CompletedTask;
        });
        var ct = TestContext.Current.CancellationToken;

        var disposeTasks = Enumerable
            .Range(0, 32)
            .Select(_ => Task.Run(async () => await disposable.DisposeAsync(), ct))
            .ToArray();

        await Task.WhenAll(disposeTasks);

        calls.Is(1);
    }

    /// <summary>
    /// Disposer (via <c>Disposable.Create(Action)</c>): second Dispose must be a no-op.
    /// </summary>
    [Fact]
    public void Disposer_DoubleDispose_HandleCalledOnlyOnce()
    {
        var calls = 0;
        var disposable = Disposable.Create(() => Interlocked.Increment(ref calls));

        disposable.Dispose();
        disposable.Dispose();

        calls.Is(1);
    }

    /// <summary>
    /// Disposer: the handle must be invoked on the first Dispose.
    /// </summary>
    [Fact]
    public void Disposer_HandleInvoked_OnFirstDisposeOnly()
    {
        var invoked = false;
        var disposable = Disposable.Create(() => invoked = true);

        invoked.IsFalse();
        disposable.Dispose();
        invoked.IsTrue();
    }

    /// <summary>
    /// Disposer: concurrent Dispose calls must invoke the handle exactly once.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Disposer_ConcurrentDispose_HandleCalledExactlyOnce()
    {
        var calls = 0;
        var disposable = Disposable.Create(() => Interlocked.Increment(ref calls));
        var ct = TestContext.Current.CancellationToken;

        var disposeTasks = Enumerable.Range(0, 32).Select(_ => Task.Run(disposable.Dispose, ct)).ToArray();

        await Task.WhenAll(disposeTasks);

        calls.Is(1);
    }

    /// <summary>
    /// DisposableReference: second DisposeAsync must be a no-op (idempotency guard).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DisposableReference_DoubleDispose_CallbackCalledOnlyOnce()
    {
        var calls = 0;
        var reference = Disposable.Reference(
            "live",
            () =>
            {
                Interlocked.Increment(ref calls);
                return ValueTask.CompletedTask;
            }
        );

        await reference.DisposeAsync();
        await reference.DisposeAsync();

        calls.Is(1);
    }

    /// <summary>
    /// DisposableReference: dispose callback runs BEFORE <c>Value</c> is nulled (the documented invariant).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DisposableReference_CallbackRunsBeforeValueNulled()
    {
        var observed = "";
        var reference = default(IDisposableReference<string>)!;
        reference = Disposable.Reference(
            "live",
            () =>
            {
                // ReSharper disable once AccessToModifiedClosure — the test relies on `reference` being assigned before DisposeAsync is called.
                observed = reference.Value;
                return ValueTask.CompletedTask;
            }
        );

        await reference.DisposeAsync();

        observed.Is("live");
    }

    /// <summary>
    /// DisposableReference: after DisposeAsync, <c>Value</c> is the default for its type.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DisposableReference_ValueIsNullAfterDispose()
    {
        var reference = Disposable.Reference("live");
        reference.Value.Is("live");

        await reference.DisposeAsync();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract — we deliberately check the post-dispose state.
        (reference.Value is null).IsTrue();
    }

    /// <summary>
    /// Verifies that when two async teardowns each throw during DisposeAsync, every registered teardown
    /// still runs (a fault in one does not skip the other), and awaiting the box's DisposeAsync surfaces
    /// the first fault unwrapped — Task.WhenAll observes both faults, but awaiting it yields the first.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_TwoAsyncTeardownsThrow_AllRunAndFirstFaultSurfaces()
    {
        // arrange
        var box = Disposable.AsyncBox(Logger);
        var disposed = 0;
        box += Disposable.Create(() =>
        {
            Interlocked.Increment(ref disposed);
            throw new InvalidOperationException("fault-1");
        });
        box += Disposable.Create(() =>
        {
            Interlocked.Increment(ref disposed);
            throw new ArgumentException("fault-2");
        });

        // act & assert — both teardowns run despite throwing; awaiting Task.WhenAll surfaces the first
        // fault unwrapped (not an AggregateException)
        await Wrap.It(async () => await box.DisposeAsync()).ThrowsAsync<InvalidOperationException>();
        disposed.Is(2);
    }

    /// <summary>
    /// Verifies that += with an IEnumerable of IAsyncDisposable registers all items
    /// and they are all disposed when DisposeAsync is called.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_AddBatchAsyncDisposables_AllDisposed()
    {
        // arrange
        var box = Disposable.AsyncBox(Logger);
        var calls = 0;
        var d1 = Disposable.Create(() =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        });
        var d2 = Disposable.Create(() =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        });
        var d3 = Disposable.Create(() =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        });

        // act
        box += (IEnumerable<IAsyncDisposable>)new IAsyncDisposable[] { d1, d2, d3 };
        await box.DisposeAsync();

        // assert
        calls.Is(3);
    }

    /// <summary>
    /// Verifies that -= with an IEnumerable of IAsyncDisposable removes the specified items
    /// so they are NOT disposed, while remaining items are disposed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_RemoveBatchAsyncDisposables_OnlyRemainingDisposed()
    {
        // arrange
        var box = Disposable.AsyncBox(Logger);
        var calls = 0;
        var d1 = Disposable.Create(() =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        });
        var d2 = Disposable.Create(() =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        });
        var d3 = Disposable.Create(() =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        });

        box += (IEnumerable<IAsyncDisposable>)new IAsyncDisposable[] { d1, d2, d3 };

        // act — remove d1 and d3, leaving only d2
        box -= (IEnumerable<IAsyncDisposable>)new IAsyncDisposable[] { d1, d3 };
        await box.DisposeAsync();

        // assert — only d2 was disposed
        calls.Is(1);
    }

    /// <summary>
    /// Verifies that += with an IEnumerable of Func&lt;ValueTask&gt; registers all functions
    /// and all are invoked when DisposeAsync is called.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_AddBatchAsyncDisposes_AllInvoked()
    {
        // arrange
        var box = Disposable.AsyncBox(Logger);
        var calls = 0;

        Func<ValueTask> f1 = () =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        };
        Func<ValueTask> f2 = () =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        };
        Func<ValueTask> f3 = () =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        };

        // act
        box += (IEnumerable<Func<ValueTask>>)new Func<ValueTask>[] { f1, f2, f3 };
        await box.DisposeAsync();

        // assert
        calls.Is(3);
    }

    /// <summary>
    /// Verifies that -= with an IEnumerable of Func&lt;ValueTask&gt; removes the specified functions
    /// so they are NOT invoked, while remaining functions are still invoked.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_RemoveBatchAsyncDisposes_OnlyRemainingInvoked()
    {
        // arrange
        var box = Disposable.AsyncBox(Logger);
        var calls = 0;

        Func<ValueTask> f1 = () =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        };
        Func<ValueTask> f2 = () =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        };
        Func<ValueTask> f3 = () =>
        {
            ++calls;
            return ValueTask.CompletedTask;
        };

        box += (IEnumerable<Func<ValueTask>>)new Func<ValueTask>[] { f1, f2, f3 };

        // act — remove f1 and f3, leaving only f2
        box -= (IEnumerable<Func<ValueTask>>)new Func<ValueTask>[] { f1, f3 };
        await box.DisposeAsync();

        // assert — only f2 was invoked
        calls.Is(1);
    }

    /// <summary>
    /// IDisposable that records how many times it was disposed — for detecting leaks or
    /// double-disposal in the concurrent stress test above.
    /// </summary>
    private sealed class CountingDisposable : IDisposable
    {
        /// <summary>Backing field tracking the number of times <see cref="Dispose"/> has been called.</summary>
        private int _disposeCount;

        /// <summary>Gets the number of times this instance has been disposed.</summary>
        public int DisposeCount => Volatile.Read(ref _disposeCount);

        /// <summary>Increments the dispose count to record that this instance was disposed.</summary>
        public void Dispose() => Interlocked.Increment(ref _disposeCount);
    }

    /// <summary>
    /// IAsyncDisposable that throws the given exception when disposed — used to verify exception
    /// propagation through <c>AsyncDisposableBox.DisposeAsync</c>.
    /// </summary>
    private sealed class ThrowingAsyncDisposable : IAsyncDisposable
    {
        /// <summary>The exception to surface when <see cref="DisposeAsync"/> is called.</summary>
        private readonly Exception _ex;

        /// <summary>Initializes a new instance with the exception to throw on dispose.</summary>
        /// <param name="ex">The exception that <see cref="DisposeAsync"/> will return as a faulted task.</param>
        public ThrowingAsyncDisposable(Exception ex)
        {
            _ex = ex;
        }

        /// <summary>Returns a faulted <see cref="ValueTask"/> containing the configured exception.</summary>
        /// <returns>A faulted <see cref="ValueTask"/> that carries the stored exception.</returns>
        public ValueTask DisposeAsync() => ValueTask.FromException(_ex);
    }
}
