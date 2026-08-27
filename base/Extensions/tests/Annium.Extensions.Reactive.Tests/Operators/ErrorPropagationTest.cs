using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests what these operators do with a source that fails. Each subscribes to its source and hands values
/// on; a subscription that ignores OnError leaves the failure with nowhere to go — the downstream observer
/// never learns of it, and anyone awaiting completion waits for a sequence that has already ended.
/// </summary>
public class ErrorPropagationTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorPropagationTest"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ErrorPropagationTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Awaiting completion of a failing source raises the failure rather than waiting forever.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task WhenCompletedAsync_SourceFails_Throws()
    {
        // arrange - the failure arrives after subscription, from another thread, as a real source's would
        var subject = new Subject<int>();
        var wait = subject.WhenCompletedAsync(Logger);
        _ = Task.Run(
            () => subject.OnError(new InvalidOperationException("source failed")),
            TestContext.Current.CancellationToken
        );

        // act & assert - bounded, because the defect being pinned is an unbounded wait
        await Bounded.AwaitAsync(wait);
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await wait).ThrowsAsync<InvalidOperationException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// A failing source reaches the subscriber of DoParallelAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task DoParallelAsync_SourceFails_ForwardsError() =>
        AssertForwardsError(source => source.DoParallelAsync(_ => Task.CompletedTask));

    /// <summary>
    /// A failing source reaches the subscriber of DoSequentialAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task DoSequentialAsync_SourceFails_ForwardsError() =>
        AssertForwardsError(source => source.DoSequentialAsync(_ => Task.CompletedTask));

    /// <summary>
    /// A failing source reaches the subscriber of SelectParallelAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task SelectParallelAsync_SourceFails_ForwardsError() =>
        AssertForwardsError(source => source.SelectParallelAsync(x => Task.FromResult(x)));

    /// <summary>
    /// A failing source reaches the subscriber of SelectSequentialAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task SelectSequentialAsync_SourceFails_ForwardsError() =>
        AssertForwardsError(source => source.SelectSequentialAsync(x => Task.FromResult(x)));

    /// <summary>
    /// A throwing handler reaches the subscriber of DoParallelAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task DoParallelAsync_HandlerThrows_ForwardsError() =>
        AssertForwardsHandlerFailure(source =>
            source.DoParallelAsync(_ => throw new InvalidOperationException("handler failed"))
        );

    /// <summary>
    /// A throwing handler reaches the subscriber of DoSequentialAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task DoSequentialAsync_HandlerThrows_ForwardsError() =>
        AssertForwardsHandlerFailure(source =>
            source.DoSequentialAsync(_ => throw new InvalidOperationException("handler failed"))
        );

    /// <summary>
    /// A throwing selector reaches the subscriber of SelectParallelAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task SelectParallelAsync_SelectorThrows_ForwardsError() =>
        AssertForwardsHandlerFailure(source =>
            source.SelectParallelAsync<int, int>(_ => throw new InvalidOperationException("handler failed"))
        );

    /// <summary>
    /// A throwing selector reaches the subscriber of SelectSequentialAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task SelectSequentialAsync_SelectorThrows_ForwardsError() =>
        AssertForwardsHandlerFailure(source =>
            source.SelectSequentialAsync<int, int>(_ => throw new InvalidOperationException("handler failed"))
        );

    /// <summary>
    /// A handler that fails partway through a source that then finishes still reports the failure. The
    /// source's completion and the handler's failure both tear the operator down; only one terminal
    /// notification reaches the observer, and it has to be the failure.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task DoSequentialAsync_HandlerThrowsMidSequence_ReportsTheFailure() =>
        AssertFailureBeatsCompletion(
            source =>
                source.DoSequentialAsync(x =>
                    x == 3 ? throw new InvalidOperationException("handler failed") : Task.CompletedTask
                ),
            sequential: true
        );

    /// <summary>
    /// The same for SelectSequentialAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task SelectSequentialAsync_SelectorThrowsMidSequence_ReportsTheFailure() =>
        AssertFailureBeatsCompletion(
            source =>
                source.SelectSequentialAsync(x =>
                    x == 3 ? throw new InvalidOperationException("handler failed") : Task.FromResult(x)
                ),
            sequential: true
        );

    /// <summary>
    /// The same for DoParallelAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task DoParallelAsync_HandlerThrowsMidSequence_ReportsTheFailure() =>
        AssertFailureBeatsCompletion(
            source =>
                source.DoParallelAsync(x =>
                    x == 3 ? throw new InvalidOperationException("handler failed") : Task.CompletedTask
                ),
            sequential: false
        );

    /// <summary>
    /// The same for SelectParallelAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public Task SelectParallelAsync_SelectorThrowsMidSequence_ReportsTheFailure() =>
        AssertFailureBeatsCompletion(
            source =>
                source.SelectParallelAsync(x =>
                    x == 3 ? throw new InvalidOperationException("handler failed") : Task.FromResult(x)
                ),
            sequential: false
        );

    /// <summary>
    /// A tracked source that fails tells its subscribers so, and does not leave a later subscriber waiting
    /// for a sequence that has already ended.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task TrackCompletion_SourceFails_ReachesSubscribersAndTerminates()
    {
        // arrange
        var subject = new Subject<int>();
        var tracked = subject.TrackCompletion(Logger);
        var early = new TaskCompletionSource<Exception>();
        using var subscription = tracked.Subscribe(_ => { }, e => early.TrySetResult(e), () => { });

        // act
        subject.OnError(new InvalidOperationException("source failed"));

        // assert - the subscriber present at the time hears about it
        await Bounded.AwaitAsync(early.Task);
        (await early.Task).As<InvalidOperationException>().Message.Is("source failed");

        // and one arriving afterwards is not left subscribed to a source that will never speak again
        var late = new TaskCompletionSource();
        using var lateSubscription = tracked.Subscribe(_ => { }, _ => late.TrySetResult(), () => late.TrySetResult());
        await Bounded.AwaitAsync(late.Task);
    }

    /// <summary>
    /// Subscribes to the operator under test and asserts the source's failure reaches the subscriber.
    /// </summary>
    /// <param name="apply">Applies the operator to a source.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertForwardsError(Func<IObservable<int>, IObservable<int>> apply)
    {
        // arrange
        var tcs = new TaskCompletionSource<Exception>();
        var subject = new Subject<int>();
        using var subscription = apply(subject).Subscribe(_ => { }, e => tcs.TrySetResult(e), () => { });

        // act
        subject.OnError(new InvalidOperationException("source failed"));

        // assert
        await Bounded.AwaitAsync(tcs.Task);
        (await tcs.Task).As<InvalidOperationException>().Message.Is("source failed");
    }

    /// <summary>
    /// Subscribes to the operator under test and asserts a failure raised by the caller's own handler
    /// reaches the subscriber. These operators run the handler on an executor built with a VoidLogger, so
    /// a handler that throws is discarded twice over unless the operator forwards it: the item vanishes
    /// and nothing is written down.
    /// </summary>
    /// <param name="apply">Applies the operator to a source.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertForwardsHandlerFailure(Func<IObservable<int>, IObservable<int>> apply)
    {
        // arrange
        var tcs = new TaskCompletionSource<Exception>();
        var subject = new Subject<int>();
        using var subscription = apply(subject).Subscribe(_ => { }, e => tcs.TrySetResult(e), () => { });

        // act
        subject.OnNext(1);

        // assert
        await Bounded.AwaitAsync(tcs.Task);
        (await tcs.Task).As<InvalidOperationException>().Message.Is("handler failed");
    }

    /// <summary>
    /// Runs the operator over a source that emits five values and completes at once, with the handler
    /// failing on the third, and asserts the observer is told about the failure rather than about the
    /// completion that was in flight at the same time.
    /// </summary>
    /// <param name="apply">Applies the operator to a source.</param>
    /// <param name="sequential">Whether the operator runs its handler on a sequential executor.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertFailureBeatsCompletion(
        Func<IObservable<int>, IObservable<int>> apply,
        bool sequential
    )
    {
        // arrange - Range emits and completes synchronously, so the completion is scheduled while the
        // handler for the third value has not run yet
        var terminal = new TaskCompletionSource<Exception?>();
        var received = new List<int>();
        var afterTerminal = 0;
        // counted rather than only awaited: a TaskCompletionSource takes the first answer and drops the
        // rest, so a second terminal notification - which the observer must never see - looks identical
        // to none at all from the awaiting side
        var terminals = 0;
        using var subscription = apply(Observable.Range(1, 5))
            .Subscribe(
                x =>
                {
                    if (terminal.Task.IsCompleted)
                        Interlocked.Increment(ref afterTerminal);
                    lock (received)
                        received.Add(x);
                },
                e =>
                {
                    Interlocked.Increment(ref terminals);
                    terminal.TrySetResult(e);
                },
                () =>
                {
                    Interlocked.Increment(ref terminals);
                    terminal.TrySetResult(null);
                }
            );

        // assert
        await Bounded.AwaitAsync(terminal.Task);
        var error = await terminal.Task;
        error.IsNotDefault("the handler failure must not be lost to the source's completion");
        error.As<InvalidOperationException>().Message.Is("handler failed");

        // nothing may arrive after the sequence has ended, whichever executor is underneath
        Volatile.Read(ref afterTerminal).Is(0, "no value may be emitted after the terminal notification");

        // the source and the failing handler both end the sequence, and they can do so at once - the
        // observer is still owed exactly one notification. Given a moment for a second one to land
        await Task.Delay(100);
        Volatile.Read(ref terminals).Is(1, "the observer must be told the sequence ended exactly once");

        // and on the sequential executor nothing after the failing item runs at all - items go one at a
        // time, so the failure is seen before the next one starts
        if (sequential)
            lock (received)
                received.Contains(4).IsFalse("nothing after the failing item may run on a sequential executor");
    }
}
