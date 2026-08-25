using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        await Bounded(wait);
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
        await Bounded(early.Task);
        (await early.Task).As<InvalidOperationException>().Message.Is("source failed");

        // and one arriving afterwards is not left subscribed to a source that will never speak again
        var late = new TaskCompletionSource();
        using var lateSubscription = tracked.Subscribe(_ => { }, _ => late.TrySetResult(), () => late.TrySetResult());
        await Bounded(late.Task);
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
        await Bounded(tcs.Task);
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
        await Bounded(tcs.Task);
        (await tcs.Task).As<InvalidOperationException>().Message.Is("handler failed");
    }

    /// <summary>
    /// Fails the test if the given task has not finished within five seconds, so a regression that turns a
    /// failure into an unbounded wait is reported as such instead of hanging the run.
    /// </summary>
    /// <param name="task">The task being bounded.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task Bounded(Task task)
    {
#pragma warning disable VSTHRD003
        var completed = await Task.WhenAny(
            task,
            Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)
        );
#pragma warning restore VSTHRD003
        (completed == task).IsTrue("a failing source must not leave the caller waiting");
    }
}
