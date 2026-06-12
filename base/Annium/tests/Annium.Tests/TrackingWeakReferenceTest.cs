using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for the TrackingWeakReference class.
/// </summary>
public class TrackingWeakReferenceTest
{
    /// <summary>
    /// Verifies that TrackingWeakReference correctly tracks object collection and raises the OnCollected event.
    /// The event fires off the finalizer thread (queued to the ThreadPool), so we wait for it via a signal.
    /// </summary>
    [Fact]
    public void TrackingWeakReference_Works()
    {
        // arrange
        using var collected = new ManualResetEventSlim(initialState: false);
        var counter = 0;
        object target;
        ITrackingWeakReference<object> reference = default!;
        Wrap(() =>
        {
            target = new object();
            reference = TrackingWeakReference.Get(target);
            reference.OnCollected += () =>
            {
                Interlocked.Increment(ref counter);
                collected.Set();
            };
        });

        // act
        target = default!;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // assert
        reference.IsAlive.IsFalse();

        // act
        reference = default!;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // assert - OnCollected runs on the ThreadPool, so wait for the signal before reading the counter.
        collected.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken).IsTrue();
        counter.Is(1);
    }

    /// <summary>
    /// Verifies that a subscriber that throws during OnCollected does not prevent a non-throwing
    /// subscriber registered before it from firing. Exceptions thrown inside the ThreadPool workitem
    /// are swallowed by the finalizer's catch block, so neither subscriber can crash the process.
    /// The non-throwing subscriber is registered first so it fires before the throwing one.
    /// </summary>
    [Fact]
    public void TrackingWeakReference_SubscriberException_NonThrowingSubscriberStillFires()
    {
        // arrange — signal to detect whether the non-throwing subscriber ran
        using var fired = new ManualResetEventSlim(initialState: false);
        object target;
        ITrackingWeakReference<object> reference = default!;
        Wrap(() =>
        {
            target = new object();
            reference = TrackingWeakReference.Get(target);

            // Register non-throwing subscriber FIRST so it runs before the throwing one.
            // Multicast delegates fire in registration order; once the throwing one aborts,
            // the outer try/catch in the workitem swallows it — so the signal is already set.
            reference.OnCollected += () => fired.Set();
            reference.OnCollected += () => throw new InvalidOperationException("subscriber-boom");
        });

        // act — drop the target and the reference so the finalizer runs
        target = default!;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        reference = default!;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // assert — non-throwing subscriber fired; process survived the throwing one
        fired.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken).IsTrue();
    }

    /// <summary>
    /// Wraps an action to prevent inlining, ensuring proper garbage collection behavior.
    /// </summary>
    /// <param name="wrap">The action to wrap.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Wrap(Action wrap) => wrap();
}
