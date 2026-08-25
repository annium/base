using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Creation.ObservableInstance;

/// <summary>
/// Tests for the StaticObservableInstance functionality in the reactive extensions.
/// </summary>
public class StaticObservableInstanceTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StaticObservableInstanceTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public StaticObservableInstanceTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that events are emitted correctly from a static observable instance,
    /// including proper retry behavior and error handling.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Events_AreEmittedCorrectly()
    {
        // arrange
        var log1 = new List<Sample>();
        var log2 = new List<Sample>();
        var errors = new List<Exception>();
        var disposeCounter = 0;
        var instance = ObservableExt
            .StaticAsyncInstance<Sample>(
                async ctx =>
                {
                    for (var i = 0; i < 5; i++)
                    {
                        await Task.Delay(100);
                        ctx.OnNext(new Sample(i));
                        if (i == 2)
                            ctx.OnError(new ArgumentOutOfRangeException(nameof(i)));
                    }

                    return async () =>
                    {
                        await Task.Delay(5);
                        Interlocked.Increment(ref disposeCounter);
                    };
                },
                CancellationToken.None,
                Get<ILogger>()
            )
            .Do(_ => { }, errors.Add)
            .Retry()
            .Catch(Observable.Empty<Sample>());
        instance.Subscribe(log1.Add);
        instance.Subscribe(log2.Add);

        await Bounded.AwaitAsync(instance.ToTask(TestContext.Current.CancellationToken));

        log1.Has(5);
        log2.Has(5);
        for (var i = 0; i < log1.Count; i++)
            log1[i].Is(log2[i]);
        errors.Has(3);
        var error = errors[0];
        foreach (var err in errors.Skip(1))
            err.Is(error);
        disposeCounter.Is(1);
    }

    /// <summary>
    /// The factory runs once per instance. Subscribing again after it finished used to start a second run
    /// over the first run's disposal state, which failed on its first call and then failed again trying to
    /// report that failure - leaving the new subscriber with no values, no completion and no error, and an
    /// exception nobody observed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Resubscribed_AfterItFinished_RunsTheFactoryOnceAndSaysSo()
    {
        // arrange
        var runs = 0;
        var instance = ObservableExt.StaticAsyncInstance<int>(
            async ctx =>
            {
                Interlocked.Increment(ref runs);
                await Task.Delay(10, ctx.Ct);

                return () => Task.CompletedTask;
            },
            TestContext.Current.CancellationToken,
            Get<ILogger>()
        );

        var first = new TaskCompletionSource();
        var subscription = instance.Subscribe(_ => { }, () => first.TrySetResult());
        await Bounded.AwaitAsync(first.Task);
        // VSTHRD103: IDisposable is the only shape Rx subscriptions offer
#pragma warning disable VSTHRD103
        subscription.Dispose();
#pragma warning restore VSTHRD103

        // act - a second consumer arrives after the run is over
        var second = new TaskCompletionSource();
        using var lateSubscription = instance.Subscribe(_ => { }, () => second.TrySetResult());

        // assert
        await Bounded.AwaitAsync(second.Task);
        Volatile.Read(ref runs).Is(1, "the factory must not run again for a later subscriber");
    }

    /// <summary>
    /// A subscriber arriving after the factory failed is told about the failure, not that the sequence
    /// completed normally. Replaying the wrong terminal notification is the same silent swallow as
    /// replaying none.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Resubscribed_AfterItFailed_ReportsTheFailure()
    {
        // arrange
        var instance = ObservableExt.StaticAsyncInstance<int>(
            _ => throw new InvalidOperationException("factory failed"),
            TestContext.Current.CancellationToken,
            Get<ILogger>()
        );

        var first = new TaskCompletionSource<Exception>();
        var subscription = instance.Subscribe(_ => { }, e => first.TrySetResult(e), () => { });
        await Bounded.AwaitAsync(first.Task);
        // VSTHRD103: IDisposable is the only shape Rx subscriptions offer
#pragma warning disable VSTHRD103
        subscription.Dispose();
#pragma warning restore VSTHRD103

        // act - a second consumer arrives after the failure
        var second = new TaskCompletionSource<Exception?>();
        using var lateSubscription = instance.Subscribe(
            _ => { },
            e => second.TrySetResult(e),
            () => second.TrySetResult(null)
        );

        // assert
        await Bounded.AwaitAsync(second.Task);
        var error = await second.Task;
        error.IsNotDefault("a subscriber arriving after a failure must hear about the failure");
        error.As<InvalidOperationException>().Message.Is("factory failed");
    }

    /// <summary>
    /// A failure in the dispose delegate the factory returned reaches the subscribers that are there at
    /// the time. Reporting it went through the same guard that disposal had just armed, so the report
    /// itself threw and the subscribers were left waiting on a run that had already ended.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DisposeThrows_ReachesTheSubscribersPresentAtTheTime()
    {
        // arrange
        var instance = ObservableExt.StaticAsyncInstance<int>(
            _ => Task.FromResult<Func<Task>>(() => throw new InvalidOperationException("dispose failed")),
            TestContext.Current.CancellationToken,
            Get<ILogger>()
        );

        // act
        var terminal = new TaskCompletionSource<Exception?>();
        using var subscription = instance.Subscribe(
            _ => { },
            e => terminal.TrySetResult(e),
            () => terminal.TrySetResult(null)
        );

        // assert
        await Bounded.AwaitAsync(terminal.Task);
        var error = await terminal.Task;
        error.IsNotDefault("a failing disposal must reach the subscribers, not silence them");
        error.As<InvalidOperationException>().Message.Is("dispose failed");
    }

    /// <summary>
    /// A subscriber arriving while the terminal notification is being delivered is told about it too. The
    /// run is marked finished before that notification goes out precisely so this subscriber takes the
    /// already-ended path instead of joining a run with nothing left to say.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Subscribed_WhileTerminalNotificationIsDelivered_IsAlsoTold()
    {
        // arrange
        var instance = ObservableExt.StaticAsyncInstance<int>(
            _ => Task.FromResult<Func<Task>>(() => Task.CompletedTask),
            TestContext.Current.CancellationToken,
            Get<ILogger>()
        );

        var late = new TaskCompletionSource();
        var lateNotifications = 0;

        // act - the second subscriber arrives from inside the first one's completion callback, which is
        // the only way to land inside that window deterministically
        using var subscription = instance.Subscribe(
            _ => { },
            () =>
                instance.Subscribe(
                    _ => { },
                    () =>
                    {
                        Interlocked.Increment(ref lateNotifications);
                        late.TrySetResult();
                    }
                )
        );

        // assert
        await Bounded.AwaitAsync(late.Task);
        Volatile.Read(ref lateNotifications).Is(1, "the late subscriber must be completed exactly once");
    }

    /// <summary>
    /// A sample data class used for testing observable events.
    /// </summary>
    private class Sample
    {
        /// <summary>
        /// Gets the integer value of this sample.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sample"/> class.
        /// </summary>
        /// <param name="value">The integer value for this sample.</param>
        public Sample(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns a string representation of this sample.
        /// </summary>
        /// <returns>The string representation of the sample value.</returns>
        public override string ToString() => Value.ToString();
    }
}
