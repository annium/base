using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the TrackCompletion operator. It exists so that a subscriber arriving after the source has
/// already finished is told so, instead of attaching to a source that will never speak again.
/// </summary>
public class TrackCompletionTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrackCompletionTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public TrackCompletionTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A subscriber present while the source runs sees its values, then its completion.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task TrackCompletion_SourceCompletes_SubscriberSeesValuesThenCompletion()
    {
        // arrange
        var subject = new Subject<int>();
        var tracked = subject.TrackCompletion(Logger);
        var received = new List<int>();
        var completed = new TaskCompletionSource();
        using var subscription = tracked.Subscribe(received.Add, () => completed.TrySetResult());

        // act
        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnCompleted();

        // assert
        await Bounded(completed.Task);
        received.Has(2).At(0).Is(1);
        received.At(1).Is(2);
    }

    /// <summary>
    /// A subscriber arriving after the source completed is completed at once - the replay this operator is
    /// for. The source deliberately is not a Subject: a Subject replays its own terminal state, which would
    /// make this pass with the operator doing nothing at all.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task TrackCompletion_LateSubscriber_CompletesAtOnce()
    {
        // arrange - the source is finished before anyone subscribes to the tracked observable
        var source = new SilentAfterEnd<int>();
        var tracked = source.TrackCompletion(Logger);
        source.End();

        // act
        var completed = new TaskCompletionSource();
        using var subscription = tracked.Subscribe(_ => { }, () => completed.TrySetResult());

        // assert - bounded, because without the replay the subscriber simply waits
        await Bounded(completed.Task);
    }

    /// <summary>
    /// The same holds for a real async instance: its completion is awaitable, and stays awaitable for a
    /// caller that asks after it already happened.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task TrackCompletion_AsyncInstance_CompletionIsAwaitableTwice()
    {
        // arrange
        using var cts = new CancellationTokenSource();
        var observable = ObservableExt
            .StaticAsyncInstance<string>(
                async ctx =>
                {
                    await Task.Delay(10, ctx.Ct);

                    return () => Task.CompletedTask;
                },
                cts.Token,
                Logger
            )
            .TrackCompletion(Logger);

        // act & assert
        await Bounded(observable.WhenCompletedAsync(Logger));
        await Bounded(observable.WhenCompletedAsync(Logger));
    }

    /// <summary>
    /// Fails the test if the given task has not finished within five seconds, so a regression that turns a
    /// completion into an unbounded wait is reported as such instead of hanging the run.
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
        (completed == task).IsTrue("a completed source must not leave the caller waiting");
    }
}

/// <summary>
/// A source that speaks to whoever is subscribed at the time and ignores anyone arriving after it has
/// ended, as most live sources do.
/// </summary>
/// <typeparam name="T">The type of items emitted by this source.</typeparam>
file sealed class SilentAfterEnd<T> : IObservable<T>
{
    /// <summary>
    /// Observers subscribed while the source was still running.
    /// </summary>
    private readonly List<IObserver<T>> _observers = new();

    /// <summary>
    /// Whether the source has ended.
    /// </summary>
    private bool _ended;

    /// <summary>
    /// Subscribes an observer, unless the source has already ended.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>A disposable subscription.</returns>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (_observers)
        {
            if (!_ended)
                _observers.Add(observer);
        }

        return Disposable.Empty;
    }

    /// <summary>
    /// Ends the source, completing the observers subscribed at the time.
    /// </summary>
    public void End()
    {
        IObserver<T>[] observers;
        lock (_observers)
        {
            _ended = true;
            observers = _observers.ToArray();
            _observers.Clear();
        }

        foreach (var observer in observers)
            observer.OnCompleted();
    }
}
